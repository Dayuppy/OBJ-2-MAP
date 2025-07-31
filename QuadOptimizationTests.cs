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
using System.IO;

namespace OBJ2MAP
{
    /// <summary>
    /// Test class for validating quad optimization functionality.
    /// This creates simple test cases to verify that the optimization logic works correctly.
    /// </summary>
    public static class QuadOptimizationTests
    {
        /// <summary>
        /// Runs basic tests to validate quad optimization functionality.
        /// Creates simple triangle pairs that should merge into quads.
        /// </summary>
        public static void RunBasicTests()
        {
            Console.WriteLine("=== QuadOptimization Basic Tests ===");
            
            TestCoplanarTriangleMerge();
            TestNonCoplanarTriangles();
            TestNoSharedEdge();
            TestComplexGeometry();
            
            Console.WriteLine("=== Tests Completed ===");
        }

        /// <summary>
        /// Test merging two coplanar triangles that share an edge into a quad.
        /// This is the primary optimization case.
        /// </summary>
        private static void TestCoplanarTriangleMerge()
        {
            Console.WriteLine("\nTest: Coplanar Triangle Merge");
            
            // Create two triangles that share an edge on the XY plane
            var triangle1 = new XFace();
            triangle1.Verts.Add(new XVector(0, 0, 0));   // Vertex 0
            triangle1.Verts.Add(new XVector(1, 0, 0));   // Vertex 1 (shared)
            triangle1.Verts.Add(new XVector(1, 1, 0));   // Vertex 2 (shared)
            triangle1.VertIdx.Add(0);
            triangle1.VertIdx.Add(1);
            triangle1.VertIdx.Add(2);
            triangle1.ComputeNormal();
            
            var triangle2 = new XFace();
            triangle2.Verts.Add(new XVector(1, 0, 0));   // Vertex 0 (shared)
            triangle2.Verts.Add(new XVector(2, 0, 0));   // Vertex 1
            triangle2.Verts.Add(new XVector(1, 1, 0));   // Vertex 2 (shared)
            triangle2.VertIdx.Add(1);
            triangle2.VertIdx.Add(3);
            triangle2.VertIdx.Add(2);
            triangle2.ComputeNormal();
            
            // Test coplanarity
            bool isCoplanar = triangle1.IsCoplanar(triangle2);
            Console.WriteLine($"  Triangles are coplanar: {isCoplanar}");
            
            // Test shared edges
            var sharedEdges = triangle1.FindSharedEdges(triangle2);
            Console.WriteLine($"  Shared edges found: {sharedEdges.Count}");
            
            // Test merge
            var mergedQuad = triangle1.TryMergeToQuad(triangle2);
            if (mergedQuad != null)
            {
                Console.WriteLine($"  ✓ Successfully merged into quad with {mergedQuad.Verts.Count} vertices");
            }
            else
            {
                Console.WriteLine($"  ✗ Failed to merge triangles into quad");
            }
        }

        /// <summary>
        /// Test that non-coplanar triangles are not merged.
        /// </summary>
        private static void TestNonCoplanarTriangles()
        {
            Console.WriteLine("\nTest: Non-Coplanar Triangles");
            
            // Create two triangles on different planes
            var triangle1 = new XFace();
            triangle1.Verts.Add(new XVector(0, 0, 0));
            triangle1.Verts.Add(new XVector(1, 0, 0));
            triangle1.Verts.Add(new XVector(1, 1, 0));
            triangle1.ComputeNormal();
            
            var triangle2 = new XFace();
            triangle2.Verts.Add(new XVector(1, 0, 0));
            triangle2.Verts.Add(new XVector(2, 0, 1));   // Different Z coordinate
            triangle2.Verts.Add(new XVector(1, 1, 1));   // Different Z coordinate
            triangle2.ComputeNormal();
            
            bool isCoplanar = triangle1.IsCoplanar(triangle2);
            Console.WriteLine($"  Triangles are coplanar: {isCoplanar}");
            
            var mergedQuad = triangle1.TryMergeToQuad(triangle2);
            if (mergedQuad == null)
            {
                Console.WriteLine($"  ✓ Correctly rejected non-coplanar triangles");
            }
            else
            {
                Console.WriteLine($"  ✗ Incorrectly merged non-coplanar triangles");
            }
        }

        /// <summary>
        /// Test that triangles without shared edges are not merged.
        /// </summary>
        private static void TestNoSharedEdge()
        {
            Console.WriteLine("\nTest: No Shared Edge");
            
            // Create two separate triangles
            var triangle1 = new XFace();
            triangle1.Verts.Add(new XVector(0, 0, 0));
            triangle1.Verts.Add(new XVector(1, 0, 0));
            triangle1.Verts.Add(new XVector(0.5, 1, 0));
            triangle1.ComputeNormal();
            
            var triangle2 = new XFace();
            triangle2.Verts.Add(new XVector(2, 0, 0));
            triangle2.Verts.Add(new XVector(3, 0, 0));
            triangle2.Verts.Add(new XVector(2.5, 1, 0));
            triangle2.ComputeNormal();
            
            var sharedEdges = triangle1.FindSharedEdges(triangle2);
            Console.WriteLine($"  Shared edges found: {sharedEdges.Count}");
            
            var mergedQuad = triangle1.TryMergeToQuad(triangle2);
            if (mergedQuad == null)
            {
                Console.WriteLine($"  ✓ Correctly rejected triangles with no shared edge");
            }
            else
            {
                Console.WriteLine($"  ✗ Incorrectly merged triangles with no shared edge");
            }
        }

        /// <summary>
        /// Test optimization on a more complex set of faces.
        /// </summary>
        private static void TestComplexGeometry()
        {
            Console.WriteLine("\nTest: Complex Geometry Optimization");
            
            var faces = new List<XFace>();
            
            // Create a quad made of two triangles (should be optimized)
            var tri1 = CreateTriangle(new XVector(0, 0, 0), new XVector(1, 0, 0), new XVector(1, 1, 0));
            var tri2 = CreateTriangle(new XVector(0, 0, 0), new XVector(1, 1, 0), new XVector(0, 1, 0));
            faces.Add(tri1);
            faces.Add(tri2);
            
            // Add a standalone triangle (should not be optimized)
            var tri3 = CreateTriangle(new XVector(2, 0, 0), new XVector(3, 0, 0), new XVector(2.5, 1, 0));
            faces.Add(tri3);
            
            // Add an existing quad (should remain unchanged)
            var quad = new XFace();
            quad.Verts.Add(new XVector(0, 2, 0));
            quad.Verts.Add(new XVector(1, 2, 0));
            quad.Verts.Add(new XVector(1, 3, 0));
            quad.Verts.Add(new XVector(0, 3, 0));
            quad.ComputeNormal();
            faces.Add(quad);
            
            Console.WriteLine($"  Original faces: {faces.Count}");
            
            // Analyze optimization potential
            var analysis = QuadOptimizer.AnalyzeOptimizationPotential(faces);
            Console.WriteLine($"  Original triangles: {analysis.OriginalTriangleCount}");
            Console.WriteLine($"  Original quads: {analysis.OriginalQuadCount}");
            Console.WriteLine($"  Potential merges: {analysis.PotentialMerges}");
            Console.WriteLine($"  Optimization percentage: {analysis.OptimizationPercentage:F1}%");
            
            // Apply optimization
            var optimizedFaces = QuadOptimizer.OptimizeFacesToQuads(faces);
            Console.WriteLine($"  Optimized faces: {optimizedFaces.Count}");
            
            // Validate optimization
            var validation = QuadOptimizer.ValidateOptimization(faces, optimizedFaces);
            Console.WriteLine($"  Validation passed: {validation.IsValid}");
            Console.WriteLine($"  Face count reduction: {validation.FaceCountReduction}");
            
            if (validation.FaceCountReduction == 1) // Should reduce by 1 (2 triangles -> 1 quad)
            {
                Console.WriteLine($"  ✓ Correct optimization achieved");
            }
            else
            {
                Console.WriteLine($"  ✗ Unexpected optimization result");
            }
        }

        /// <summary>
        /// Helper method to create a triangle face.
        /// </summary>
        private static XFace CreateTriangle(XVector v1, XVector v2, XVector v3)
        {
            var face = new XFace();
            face.Verts.Add(v1);
            face.Verts.Add(v2);
            face.Verts.Add(v3);
            face.VertIdx.Add(0);
            face.VertIdx.Add(1);
            face.VertIdx.Add(2);
            face.ComputeNormal();
            return face;
        }

        /// <summary>
        /// Creates a simple test OBJ content with two triangles that can be merged.
        /// This can be used to test the full optimization pipeline.
        /// </summary>
        public static string CreateTestOBJContent()
        {
            return @"# Test OBJ for quad optimization
o TestQuad
v 0.0 0.0 0.0
v 1.0 0.0 0.0
v 1.0 1.0 0.0
v 0.0 1.0 0.0
vt 0.0 0.0
vt 1.0 0.0
vt 1.0 1.0
vt 0.0 1.0
f 1/1 2/2 3/3
f 1/1 3/3 4/4
";
        }

        /// <summary>
        /// Creates a test OBJ file for integration testing.
        /// </summary>
        public static void CreateTestOBJFile(string filename)
        {
            File.WriteAllText(filename, CreateTestOBJContent());
            Console.WriteLine($"Created test OBJ file: {filename}");
        }
    }
}