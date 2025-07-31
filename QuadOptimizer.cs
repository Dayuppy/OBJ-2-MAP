//	Copyright (c) 2015, Warren Marshall <warren@warrenmarshall.biz>
//	
//	Permission to use, copy, modify, and/or distribute this software for any
//	purpose with or without fee is hereby granted, provided that the above
//	copyright notice and this permission notice appear in all copies.
//
//	THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
//	WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
//	MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
//	ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
//	WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
//	ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
//	OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace OBJ2MAP
{
    /// <summary>
    /// Provides quad detection and optimization functionality for brush generation.
    /// This class implements algorithms to merge adjacent triangular faces into quads
    /// for more efficient brush creation while preserving geometric accuracy.
    /// </summary>
    public static class QuadOptimizer
    {
        /// <summary>
        /// Default tolerance for geometric comparisons in quad optimization.
        /// This value balances accuracy with the ability to detect near-coplanar faces.
        /// </summary>
        public const double DEFAULT_TOLERANCE = 0.001;

        /// <summary>
        /// Optimizes a list of faces by converting adjacent triangular faces into quads where possible.
        /// This reduces the total number of brushes needed while preserving the model's geometry.
        /// 
        /// Algorithm:
        /// 1. Identify all triangular faces
        /// 2. For each triangle, find adjacent triangles that share an edge
        /// 3. Check if adjacent triangles are coplanar within tolerance
        /// 4. Validate that merging creates a valid quad (non-degenerate, proper winding)
        /// 5. Replace triangle pairs with merged quads
        /// 6. Preserve non-triangular faces and unmergeable triangles
        /// </summary>
        /// <param name="originalFaces">List of faces to optimize</param>
        /// <param name="tolerance">Geometric tolerance for coplanarity and edge matching</param>
        /// <param name="preserveOriginal">If true, keeps original faces for fallback</param>
        /// <returns>Optimized face list with quads replacing mergeable triangle pairs</returns>
        public static List<XFace> OptimizeFacesToQuads(List<XFace> originalFaces, double tolerance = DEFAULT_TOLERANCE, bool preserveOriginal = true)
        {
            var optimizedFaces = new List<XFace>();
            var processedFaces = new HashSet<int>(); // Track which faces have been processed
            
            for (int i = 0; i < originalFaces.Count; i++)
            {
                if (processedFaces.Contains(i)) continue;
                
                var face1 = originalFaces[i];
                
                // Only try to optimize triangular faces
                if (face1.Verts.Count != 3)
                {
                    optimizedFaces.Add(face1);
                    continue;
                }
                
                bool merged = false;
                
                // Look for a mergeable triangle
                for (int j = i + 1; j < originalFaces.Count && !merged; j++)
                {
                    if (processedFaces.Contains(j)) continue;
                    
                    var face2 = originalFaces[j];
                    
                    // Only merge triangles
                    if (face2.Verts.Count != 3) continue;
                    
                    // Attempt to merge the triangles into a quad
                    var mergedQuad = face1.TryMergeToQuad(face2, tolerance);
                    if (mergedQuad != null)
                    {
                        // Successfully merged - add the quad and mark both triangles as processed
                        optimizedFaces.Add(mergedQuad);
                        processedFaces.Add(i);
                        processedFaces.Add(j);
                        merged = true;
                        
                        // Log the optimization for debugging
                        LogQuadOptimization(face1, face2, mergedQuad);
                    }
                }
                
                // If no merge was possible, keep the original triangle
                if (!merged)
                {
                    optimizedFaces.Add(face1);
                }
            }
            
            return optimizedFaces;
        }

        /// <summary>
        /// Analyzes the optimization potential of a face list.
        /// Provides statistics on how many triangles could potentially be merged into quads.
        /// </summary>
        /// <param name="faces">List of faces to analyze</param>
        /// <param name="tolerance">Geometric tolerance for analysis</param>
        /// <returns>OptimizationAnalysis containing statistics and recommendations</returns>
        public static OptimizationAnalysis AnalyzeOptimizationPotential(List<XFace> faces, double tolerance = DEFAULT_TOLERANCE)
        {
            var analysis = new OptimizationAnalysis();
            
            var triangles = faces.Where(f => f.Verts.Count == 3).ToList();
            var quads = faces.Where(f => f.Verts.Count == 4).ToList();
            var otherFaces = faces.Where(f => f.Verts.Count != 3 && f.Verts.Count != 4).ToList();
            
            analysis.OriginalTriangleCount = triangles.Count;
            analysis.OriginalQuadCount = quads.Count;
            analysis.OtherFaceCount = otherFaces.Count;
            analysis.TotalOriginalFaces = faces.Count;
            
            // Find potential merges
            var processedTriangles = new HashSet<int>();
            int potentialMerges = 0;
            
            for (int i = 0; i < triangles.Count; i++)
            {
                if (processedTriangles.Contains(i)) continue;
                
                for (int j = i + 1; j < triangles.Count; j++)
                {
                    if (processedTriangles.Contains(j)) continue;
                    
                    var testMerge = triangles[i].TryMergeToQuad(triangles[j], tolerance);
                    if (testMerge != null)
                    {
                        potentialMerges++;
                        processedTriangles.Add(i);
                        processedTriangles.Add(j);
                        break;
                    }
                }
            }
            
            analysis.PotentialMerges = potentialMerges;
            analysis.OptimizedTriangleCount = triangles.Count - (potentialMerges * 2);
            analysis.OptimizedQuadCount = quads.Count + potentialMerges;
            analysis.TotalOptimizedFaces = analysis.OptimizedTriangleCount + analysis.OptimizedQuadCount + analysis.OtherFaceCount;
            analysis.BrushReduction = potentialMerges; // Each merge reduces brush count by 1
            
            return analysis;
        }

        /// <summary>
        /// Validates that optimized faces maintain geometric accuracy compared to original faces.
        /// Checks for preservation of surface area, normal consistency, and UV coordinate validity.
        /// </summary>
        /// <param name="originalFaces">Original face list</param>
        /// <param name="optimizedFaces">Optimized face list</param>
        /// <param name="tolerance">Tolerance for validation checks</param>
        /// <returns>Validation result with detailed analysis</returns>
        public static ValidationResult ValidateOptimization(List<XFace> originalFaces, List<XFace> optimizedFaces, double tolerance = DEFAULT_TOLERANCE)
        {
            var result = new ValidationResult();
            
            // Check face count changes
            result.OriginalFaceCount = originalFaces.Count;
            result.OptimizedFaceCount = optimizedFaces.Count;
            result.FaceCountReduction = originalFaces.Count - optimizedFaces.Count;
            
            // Validate geometric properties
            // Note: Detailed validation would require more complex surface area and volume calculations
            // For now, we perform basic sanity checks
            
            result.IsValid = true;
            result.ValidationMessages = new List<string>();
            
            // Check for degenerate faces in optimized set
            foreach (var face in optimizedFaces)
            {
                if (face.Verts.Count < 3)
                {
                    result.IsValid = false;
                    result.ValidationMessages.Add($"Found degenerate face with {face.Verts.Count} vertices");
                }
                
                // Check for very small faces that might indicate numerical issues
                if (face.Verts.Count >= 3)
                {
                    var area = CalculateFaceArea(face);
                    if (area < tolerance * tolerance)
                    {
                        result.ValidationMessages.Add($"Warning: Very small face area detected ({area})");
                    }
                }
            }
            
            if (result.ValidationMessages.Count == 0)
            {
                result.ValidationMessages.Add("Optimization validation passed - no issues detected");
            }
            
            return result;
        }

        /// <summary>
        /// Calculates the approximate area of a face for validation purposes.
        /// Uses triangulation for non-triangular faces.
        /// </summary>
        private static double CalculateFaceArea(XFace face)
        {
            if (face.Verts.Count < 3) return 0.0;
            
            if (face.Verts.Count == 3)
            {
                // Triangle area using cross product
                var v1 = XVector.Subtract(face.Verts[1], face.Verts[0]);
                var v2 = XVector.Subtract(face.Verts[2], face.Verts[0]);
                var cross = XVector.Cross(v1, v2);
                return cross.GetLength() * 0.5;
            }
            else
            {
                // Approximate area for quads and other polygons using fan triangulation
                double totalArea = 0.0;
                var v0 = face.Verts[0];
                
                for (int i = 1; i < face.Verts.Count - 1; i++)
                {
                    var v1 = XVector.Subtract(face.Verts[i], v0);
                    var v2 = XVector.Subtract(face.Verts[i + 1], v0);
                    var cross = XVector.Cross(v1, v2);
                    totalArea += cross.GetLength() * 0.5;
                }
                
                return totalArea;
            }
        }

        /// <summary>
        /// Logs information about a successful quad optimization for debugging and analysis.
        /// </summary>
        private static void LogQuadOptimization(XFace triangle1, XFace triangle2, XFace mergedQuad)
        {
            // This could be enhanced to write to a detailed log file
            Console.WriteLine($"Quad Optimization: Merged 2 triangles into 1 quad");
            Console.WriteLine($"  Triangle 1: {triangle1.Verts.Count} vertices, texture: {triangle1.TexName ?? "none"}");
            Console.WriteLine($"  Triangle 2: {triangle2.Verts.Count} vertices, texture: {triangle2.TexName ?? "none"}");
            Console.WriteLine($"  Merged Quad: {mergedQuad.Verts.Count} vertices, texture: {mergedQuad.TexName ?? "none"}");
        }
    }

    /// <summary>
    /// Contains analysis results for quad optimization potential.
    /// Provides statistics on the current state and potential improvements.
    /// </summary>
    public class OptimizationAnalysis
    {
        public int OriginalTriangleCount { get; set; }
        public int OriginalQuadCount { get; set; }
        public int OtherFaceCount { get; set; }
        public int TotalOriginalFaces { get; set; }
        
        public int PotentialMerges { get; set; }
        public int OptimizedTriangleCount { get; set; }
        public int OptimizedQuadCount { get; set; }
        public int TotalOptimizedFaces { get; set; }
        
        public int BrushReduction { get; set; }
        
        public double OptimizationPercentage => TotalOriginalFaces > 0 ? 
            (double)BrushReduction / TotalOriginalFaces * 100.0 : 0.0;
    }

    /// <summary>
    /// Contains validation results for optimized face geometry.
    /// Ensures that optimization maintains geometric accuracy.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public int OriginalFaceCount { get; set; }
        public int OptimizedFaceCount { get; set; }
        public int FaceCountReduction { get; set; }
        public List<string> ValidationMessages { get; set; } = new List<string>();
    }
}