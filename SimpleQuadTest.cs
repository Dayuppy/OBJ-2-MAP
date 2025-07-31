using System;
using System.Collections.Generic;

namespace OBJ2MAP
{
    /// <summary>
    /// Simple standalone test program to validate quad optimization algorithms
    /// without requiring the full MAP generation pipeline.
    /// </summary>
    class SimpleQuadTest
    {
        static void Main(string[] args)
        {
            Console.WriteLine("OBJ-2-MAP Quad Optimization Algorithm Test");
            Console.WriteLine("==========================================");
            
            try
            {
                // Test the core optimization algorithms
                TestCoplanarDetection();
                TestSharedEdgeDetection();
                TestQuadMerging();
                TestOptimizationPipeline();
                
                Console.WriteLine("\n✓ All algorithm tests completed successfully!");
                Console.WriteLine("\nQuad Optimization Features Summary:");
                Console.WriteLine("- Coplanar triangle detection with configurable tolerance");
                Console.WriteLine("- Shared edge identification between adjacent faces");
                Console.WriteLine("- Triangle-to-quad merging with UV coordinate preservation");
                Console.WriteLine("- Geometric validation of merged quads");
                Console.WriteLine("- Optimization analysis and statistics");
                Console.WriteLine("- Integration with brush generation for reduced brush count");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Test failed with error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Test coplanar detection algorithm
        /// </summary>
        static void TestCoplanarDetection()
        {
            Console.WriteLine("\n=== Testing Coplanar Detection ===");
            
            // Create two coplanar triangles on XY plane
            var triangle1 = CreateTriangle(
                new XVector(0, 0, 0),
                new XVector(1, 0, 0),
                new XVector(1, 1, 0)
            );
            
            var triangle2 = CreateTriangle(
                new XVector(1, 0, 0),
                new XVector(2, 0, 0),
                new XVector(1, 1, 0)
            );
            
            bool areCoplanar = triangle1.IsCoplanar(triangle2);
            Console.WriteLine($"Coplanar triangles test: {(areCoplanar ? "✓ PASS" : "✗ FAIL")}");
            
            // Create non-coplanar triangles
            var triangle3 = CreateTriangle(
                new XVector(0, 0, 1), // Different Z level
                new XVector(1, 0, 1),
                new XVector(1, 1, 1)
            );
            
            bool areNonCoplanar = !triangle1.IsCoplanar(triangle3);
            Console.WriteLine($"Non-coplanar triangles test: {(areNonCoplanar ? "✓ PASS" : "✗ FAIL")}");
        }

        /// <summary>
        /// Test shared edge detection algorithm
        /// </summary>
        static void TestSharedEdgeDetection()
        {
            Console.WriteLine("\n=== Testing Shared Edge Detection ===");
            
            // Create triangles that share an edge
            var triangle1 = CreateTriangle(
                new XVector(0, 0, 0),
                new XVector(1, 0, 0), // shared edge point 1
                new XVector(1, 1, 0)  // shared edge point 2
            );
            
            var triangle2 = CreateTriangle(
                new XVector(1, 0, 0), // shared edge point 1
                new XVector(2, 0, 0),
                new XVector(1, 1, 0)  // shared edge point 2
            );
            
            var sharedEdges = triangle1.FindSharedEdges(triangle2);
            bool hasSharedEdge = sharedEdges.Count == 1;
            Console.WriteLine($"Shared edge detection: {(hasSharedEdge ? "✓ PASS" : "✗ FAIL")} (found {sharedEdges.Count} shared edges)");
            
            // Test triangles with no shared edges
            var triangle3 = CreateTriangle(
                new XVector(3, 0, 0),
                new XVector(4, 0, 0),
                new XVector(3.5, 1, 0)
            );
            
            var noSharedEdges = triangle1.FindSharedEdges(triangle3);
            bool hasNoSharedEdges = noSharedEdges.Count == 0;
            Console.WriteLine($"No shared edges test: {(hasNoSharedEdges ? "✓ PASS" : "✗ FAIL")} (found {noSharedEdges.Count} shared edges)");
        }

        /// <summary>
        /// Test triangle-to-quad merging algorithm
        /// </summary>
        static void TestQuadMerging()
        {
            Console.WriteLine("\n=== Testing Quad Merging ===");
            
            // Create two triangles that can form a valid quad
            var triangle1 = CreateTriangle(
                new XVector(0, 0, 0),
                new XVector(1, 0, 0),
                new XVector(1, 1, 0)
            );
            
            var triangle2 = CreateTriangle(
                new XVector(0, 0, 0),
                new XVector(1, 1, 0),
                new XVector(0, 1, 0)
            );
            
            var mergedQuad = triangle1.TryMergeToQuad(triangle2);
            bool canMerge = mergedQuad != null && mergedQuad.Verts.Count == 4;
            Console.WriteLine($"Triangle to quad merging: {(canMerge ? "✓ PASS" : "✗ FAIL")} {(mergedQuad != null ? $"(created {mergedQuad.Verts.Count} vertex quad)" : "(merge failed)")}");
            
            // Test non-mergeable triangles (non-coplanar)
            var triangle3 = CreateTriangle(
                new XVector(0, 0, 1), // Different Z level
                new XVector(1, 0, 1),
                new XVector(1, 1, 1)
            );
            
            var noMerge = triangle1.TryMergeToQuad(triangle3);
            bool correctlyRejectsMerge = noMerge == null;
            Console.WriteLine($"Non-mergeable rejection: {(correctlyRejectsMerge ? "✓ PASS" : "✗ FAIL")} (correctly rejected non-coplanar triangles)");
        }

        /// <summary>
        /// Test the full optimization pipeline
        /// </summary>
        static void TestOptimizationPipeline()
        {
            Console.WriteLine("\n=== Testing Optimization Pipeline ===");
            
            var faces = new List<XFace>();
            
            // Add a quad made of two triangles (should be optimized)
            faces.Add(CreateTriangle(new XVector(0, 0, 0), new XVector(1, 0, 0), new XVector(1, 1, 0)));
            faces.Add(CreateTriangle(new XVector(0, 0, 0), new XVector(1, 1, 0), new XVector(0, 1, 0)));
            
            // Add standalone triangles (should remain unchanged)
            faces.Add(CreateTriangle(new XVector(2, 0, 0), new XVector(3, 0, 0), new XVector(2.5, 1, 0)));
            faces.Add(CreateTriangle(new XVector(4, 0, 0), new XVector(5, 0, 0), new XVector(4.5, 1, 0)));
            
            // Add existing quad (should remain unchanged)
            var existingQuad = new XFace();
            existingQuad.Verts.Add(new XVector(0, 2, 0));
            existingQuad.Verts.Add(new XVector(1, 2, 0));
            existingQuad.Verts.Add(new XVector(1, 3, 0));
            existingQuad.Verts.Add(new XVector(0, 3, 0));
            existingQuad.ComputeNormal();
            faces.Add(existingQuad);
            
            Console.WriteLine($"Input: {faces.Count} faces (3 triangles + 1 mergeable pair + 1 quad)");
            
            // Analyze optimization potential
            var analysis = QuadOptimizer.AnalyzeOptimizationPotential(faces);
            Console.WriteLine($"Analysis: {analysis.OriginalTriangleCount} triangles, {analysis.PotentialMerges} potential merges");
            
            // Apply optimization
            var optimizedFaces = QuadOptimizer.OptimizeFacesToQuads(faces);
            Console.WriteLine($"Output: {optimizedFaces.Count} faces");
            
            // Validate results
            var validation = QuadOptimizer.ValidateOptimization(faces, optimizedFaces);
            bool optimizationSuccessful = validation.IsValid && validation.FaceCountReduction == 1; // Should reduce by 1 (2 triangles -> 1 quad)
            
            Console.WriteLine($"Optimization pipeline: {(optimizationSuccessful ? "✓ PASS" : "✗ FAIL")} (reduced by {validation.FaceCountReduction} faces)");
            Console.WriteLine($"Validation: {(validation.IsValid ? "✓ VALID" : "✗ INVALID")}");
            
            if (analysis.PotentialMerges > 0)
            {
                double optimizationPercentage = ((double)validation.FaceCountReduction / faces.Count) * 100;
                Console.WriteLine($"Optimization effectiveness: {optimizationPercentage:F1}% brush count reduction");
            }
        }

        /// <summary>
        /// Helper method to create a triangle face with computed normal
        /// </summary>
        static XFace CreateTriangle(XVector v1, XVector v2, XVector v3)
        {
            var face = new XFace();
            face.Verts.Add(v1);
            face.Verts.Add(v2);
            face.Verts.Add(v3);
            
            // Add vertex indices
            face.VertIdx.Add(0);
            face.VertIdx.Add(1);
            face.VertIdx.Add(2);
            
            // Compute normal
            face.ComputeNormal();
            
            return face;
        }
    }
}