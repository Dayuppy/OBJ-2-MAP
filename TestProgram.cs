using System;
using System.IO;

namespace OBJ2MAP
{
    /// <summary>
    /// Simple test program to validate quad optimization functionality
    /// without requiring the full WPF UI.
    /// </summary>
    class TestProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine("OBJ-2-MAP Quad Optimization Test Program");
            Console.WriteLine("=========================================");
            
            try
            {
                // Run basic unit tests
                QuadOptimizationTests.RunBasicTests();
                
                // Create a test OBJ file
                string testObjPath = "/tmp/test_quad.obj";
                QuadOptimizationTests.CreateTestOBJFile(testObjPath);
                
                // Test the full pipeline if the file was created successfully
                if (File.Exists(testObjPath))
                {
                    Console.WriteLine("\n=== Integration Test ===");
                    TestFullPipeline(testObjPath);
                }
                
                Console.WriteLine("\n✓ All tests completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Test failed with error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Tests the full OBJ loading and optimization pipeline.
        /// </summary>
        static void TestFullPipeline(string objFilePath)
        {
            Console.WriteLine($"Testing full pipeline with file: {objFilePath}");
            
            try
            {
                // Read the test OBJ file
                var fileLines = File.ReadAllLines(objFilePath);
                
                // Initialize data structures
                var vertices = new System.Collections.Generic.List<XVector>();
                var faces = new System.Collections.Generic.List<XFace>();
                var uvs = new System.Collections.Generic.List<XUV>();
                var brushes = new System.Collections.Generic.List<XBrush>();
                
                // Create a simple progress tracker
                var progressTracker = new MainFormCompat.ConsoleProgressTracker();
                
                // Load the OBJ file
                using (var stringWriter = new StringWriter())
                {
                    MAPCreation.LoadOBJ(
                        progressTracker,
                        fileLines,
                        MainFormCompat.MainForm.EGRP.Undefined,
                        new StreamWriter(stringWriter.BaseStream),
                        ref vertices,
                        ref faces,
                        ref uvs,
                        ref brushes,
                        100.0f, // scale
                        new char[] { ' ' }, // separator1
                        new char[] { '/' }  // separator2
                    );
                }
                
                Console.WriteLine($"Loaded: {vertices.Count} vertices, {faces.Count} faces, {brushes.Count} brushes");
                
                // Test optimization on the loaded faces
                if (faces.Count > 0)
                {
                    var analysis = QuadOptimizer.AnalyzeOptimizationPotential(faces);
                    Console.WriteLine($"Optimization analysis:");
                    Console.WriteLine($"  Triangles: {analysis.OriginalTriangleCount}");
                    Console.WriteLine($"  Quads: {analysis.OriginalQuadCount}");
                    Console.WriteLine($"  Potential merges: {analysis.PotentialMerges}");
                    Console.WriteLine($"  Potential reduction: {analysis.OptimizationPercentage:F1}%");
                    
                    var optimizedFaces = QuadOptimizer.OptimizeFacesToQuads(faces);
                    var validation = QuadOptimizer.ValidateOptimization(faces, optimizedFaces);
                    
                    Console.WriteLine($"Optimization result:");
                    Console.WriteLine($"  Original faces: {validation.OriginalFaceCount}");
                    Console.WriteLine($"  Optimized faces: {validation.OptimizedFaceCount}");
                    Console.WriteLine($"  Reduction: {validation.FaceCountReduction}");
                    Console.WriteLine($"  Valid: {validation.IsValid}");
                }
                
                Console.WriteLine("✓ Integration test completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Integration test failed: {ex.Message}");
                throw;
            }
        }
    }
}