//	Copyright (c) 2015, Warren Marshall <warren@warrenmarshall.biz>
//	Copyright (c) 2015, Aleksander Marhall
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
using System.Text;
using System.IO;
using System.Globalization;
using MathNet.Numerics.LinearAlgebra;
using System.Diagnostics;

namespace OBJ2MAP
{
    /// <summary>
    /// MAP creation - Valve 220 format
    /// </summary>
    class MAPCreation
	{
        // HACK
        private static StreamWriter logStream;

        // HACK - keeping for compatibility but using new interface
        private static MainFormCompat.IProgressTracker mainFormTracker;
        public static bool bAxisAligned = false;

        public static void SetForm(MainFormCompat.IProgressTracker progressTracker)
        {
            mainFormTracker = progressTracker;
        }

        private static void Log(string message, params object[] args)
        {
            Console.WriteLine(message, args);
            if (logStream != null)
                logStream.WriteLine(message, args);
        }

        private static void SetLogger(StreamWriter stream)
        {
            logStream = stream;
        }

        static XVector[] AxisArray = new XVector[6]
        {
            new XVector(1.0, 0.0, 0.0),
            new XVector(-1.0, 0.0, 0.0),
            new XVector(0.0, 1.0, 0.0),
            new XVector(0.0, -1.0, 0.0),
            new XVector(0.0, 0.0, 1.0),
            new XVector(0.0, 0.0, -1.0)
        };

        public static void LoadOBJ(MainFormCompat.IProgressTracker progressTracker,
									string[] fileLines,
								   MainFormCompat.MainForm.EGRP egrp,
								   StreamWriter streamWriter,
								   ref List<XVector> _Vertices,
								   ref List<XFace> _Faces,
                                   ref List<XUV> _UVs,
                                   ref List<XBrush> _Brushes,
								   float scale,
								   char[] separator1,
								   char[] separator2)
		{
            SetLogger(streamWriter);

            progressTracker?.UpdateProgress("Loading OBJ File...");

            //List<XUV> uvs = new List<XUV>();
            string texname = null;
            
            //  Helper variables
            string TrimmedLine = "";
            string[] strArray = new string[0];
            string[] strArray1 = new string[0];
            string[] strArray2 = new string[0];
            XBrush B;
            XVector xvector;
            double num6;
            string fullpath;
            XUV uv;
            XFace xface;

            foreach (string Line in fileLines)
			{
				progressTracker?.UpdateProgress();

				TrimmedLine = Line.Trim();

				//streamWriter.WriteLine(string.Format("# OBJ Line: {0}", (object)TrimmedLine));
				if (!TrimmedLine.StartsWith("# ") && TrimmedLine.Length != 0)
				{
					if (egrp == MainFormCompat.MainForm.EGRP.Undefined && (TrimmedLine.StartsWith("o ") || TrimmedLine.StartsWith("g ")))
					{
						egrp = !TrimmedLine.StartsWith("g ") ? MainFormCompat.MainForm.EGRP.Ungrouped : MainFormCompat.MainForm.EGRP.Grouped;
					}

					if (TrimmedLine.StartsWith("g ") && egrp == MainFormCompat.MainForm.EGRP.Grouped || TrimmedLine.StartsWith("o ") && egrp == MainFormCompat.MainForm.EGRP.Ungrouped)
					{
						if (_Faces.Count > 0)
						{
							B = new XBrush();
							_Brushes.Add( B );
							B.Faces = _Faces;
						}
						_Faces = new List<XFace>();
					}
					if (TrimmedLine.StartsWith("v "))
					{
						strArray = TrimmedLine.Split(separator1, StringSplitOptions.RemoveEmptyEntries);
						if (strArray.Length == 4)
						{
							xvector = new XVector(double.Parse(strArray[1], CultureInfo.InvariantCulture), double.Parse(strArray[2], CultureInfo.InvariantCulture), double.Parse(strArray[3], CultureInfo.InvariantCulture));
							num6 = xvector.y;
							xvector.y = -xvector.z;
							xvector.z = num6;
							xvector.x *= (double)scale / 100.0;
							xvector.y *= (double)scale / 100.0;
							xvector.z *= (double)scale / 100.0;
							_Vertices.Add(xvector);
						}
					}
                    if (TrimmedLine.StartsWith("usemtl "))
                    {
                        strArray = TrimmedLine.Split(separator1, StringSplitOptions.RemoveEmptyEntries);
                        fullpath = strArray[1];
                        texname = Path.GetFileNameWithoutExtension(fullpath);
                        texname = texname.ToLowerInvariant();
                        if (texname.Length > 16)
                        {
                            texname = texname.Substring(0, 16);
                        }
                        Log("Will search for texture {0}", texname);
                    }
                    if (TrimmedLine.StartsWith("vt "))
                    {
                        strArray = TrimmedLine.Split(separator1, StringSplitOptions.RemoveEmptyEntries);
                        if (strArray.Length >= 3)
                        {
                            uv = new XUV(double.Parse(strArray[1], CultureInfo.InvariantCulture), double.Parse(strArray[2], CultureInfo.InvariantCulture));
                            uv.V = -uv.V;
                            _UVs.Add(uv);
                            //uvs.Add(uv);
                        }
                        else
                        {
                            Log("Invaild 'vt' line");
                        }
                    }
                    if (TrimmedLine.StartsWith("f "))
					{
						xface = new XFace();
						strArray1 = TrimmedLine.Split(separator1, StringSplitOptions.RemoveEmptyEntries);
						for (int index = 1; index < strArray1.Length; ++index)
						{
							strArray2 = strArray1[index].Split(separator2, StringSplitOptions.RemoveEmptyEntries);
							xface.VertIdx.Add(int.Parse(strArray2[0]) - 1);

                            if (strArray2.Length >= 2)
                            {
                                xface.UVIdx.Add(int.Parse(strArray2[1]) - 1);
                            }
						}

                        xface.ComputeNormal(ref _Vertices);

                        if (bAxisAligned)
                        {
                            int finalNormalIdx = -1;
                            double lastDotProduct = -999.0;
                            for (int idx = 0; idx < 6; ++idx)
                            {
                                double normalDotProduct = XVector.Dot(AxisArray[idx], xface.Normal);
                                if (normalDotProduct > lastDotProduct)
                                {
                                    lastDotProduct = normalDotProduct;
                                    finalNormalIdx = idx;
                                }
                            }
                            xface.Normal = AxisArray[finalNormalIdx];
                        }

                        // Populate actual verts/uvs
                        //List<XVector> vertsCopy = new List<XVector>(_Vertices); //  TODO: investigate this
                        //xface.Verts = new List<XVector>(from idx in xface.VertIdx select vertsCopy[idx]);

                        foreach (int vertIdx in xface.VertIdx)
                        {
                            xface.Verts.Add(_Vertices[vertIdx]);
                        }

                        if (_UVs != null && _UVs.Count > 0)
                        {
                            foreach (int idx in xface.UVIdx)
                            {
                                xface.UVs.Add(_UVs[idx]);
                            }
                        }

                        //xface.UVs = new List<XUV>(from idx in xface.UVIdx select uvs[idx]);
                        xface.TexName = texname;
                        _Faces.Add(xface);
					}
				}

                //  Reset variables
                TrimmedLine = "";
                strArray = new string[0];
                strArray1 = new string[0];
                strArray2 = new string[0];
            }

            SetLogger(null);
            //SaveOBJ(_Vertices, _UVs, _Faces);
        }

        public static void SaveOBJ(List<XVector> _Vertices,
                                    List<XUV> _UVs,
                                    List<XFace> _Faces)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"C:\test_export.obj"))
            {
                file.WriteLine("o test_obj");
                foreach (XVector vertex in _Vertices)
                {
                    double tmpVertex = vertex.y;
                    vertex.y = vertex.z;
                    vertex.z = -tmpVertex;
                    file.WriteLine("v " + vertex.x.ToString("f6",CultureInfo.InvariantCulture) + " " + vertex.y.ToString("f6", CultureInfo.InvariantCulture) + " " + vertex.z.ToString("f6", CultureInfo.InvariantCulture));
                }

                foreach (XUV uv in _UVs)
                {
                    file.WriteLine("vt " + (uv.U.ToString("f6", CultureInfo.InvariantCulture)) + " " + (uv.U.ToString("f6", CultureInfo.InvariantCulture)));
                }

                foreach (XFace face in _Faces)
                {
                    file.WriteLine("f " + (face.VertIdx[0] + 1) + " " + (face.VertIdx[1] + 1) + " " + (face.VertIdx[2] + 1));
                }
                file.Close();
            }

        }

        private static string ByteArrayToString(byte[] input)
        {
            // count the number of bytes before a 0 byte
            int len = 0;
            for (; len < input.Length; len++)
            {
                if (input[len] == (byte)0) break;
            }

            var result = System.Text.Encoding.ASCII.GetString(input, 0, len);
            return result;
        }

        private static Dictionary<string, Tuple<int, int>> TextureSizesFromWad(string filename)
        {
            var result = new Dictionary<string, Tuple<int, int>>();

            try
            {
                using (var f = new FileStream(filename, FileMode.Open))
                {
                    var br = new BinaryReader(f);
                    //http://www.gamers.org/dEngine/quake/spec/quake-spec34/qkspec_7.htm
                    var magic = br.ReadBytes(4);
                    if (!magic.SequenceEqual(from ch in "WAD2" select (byte)ch))
                    {
                        Log("Invalid wad file {0}", filename);
                        return result;
                    }
                    var numentries = br.ReadInt32();
                    var diroffset = br.ReadInt32();
                    f.Seek(diroffset, SeekOrigin.Begin);

                    for (int i = 0; i < numentries; i++)
                    {
                        var offset = br.ReadInt32();
                        var dsize = br.ReadInt32();
                        var size = br.ReadInt32();
                        var type = br.ReadByte();
                        var cmprs = br.ReadByte();
                        var pad = br.ReadInt16();
                        var name = br.ReadBytes(16);

                        var current_offset = f.Position;
                        // Seek to the miptex lump
                        f.Seek(offset, SeekOrigin.Begin);

                        var miptex_name = ByteArrayToString(br.ReadBytes(16));
                        var width = br.ReadInt32();
                        var height = br.ReadInt32();

                        // We don't need the rest of the miptex, just the width and height

                        if (!String.IsNullOrWhiteSpace(miptex_name))
                        {
                            result[miptex_name.ToLowerInvariant()] = new Tuple<int, int>(width, height);
                        }

                        // Seek back to the directory
                        f.Seek(current_offset, SeekOrigin.Begin);
                    }
                }

            }
            catch (Exception e)
            {
                Log("Error loading wad {0}: {1}", filename, e.ToString());
            }

            return result;
        }

        /// <summary>
        /// Scans all wads in the given directory, and returns a dictionary of texture names
        /// mapped to (width,height) tuples. Texture names are converted to lowercase.
        /// </summary>
        private static Dictionary<string, Tuple<int, int>> TextureSizesInDirectory(string dir)
        {
            var result = new Dictionary<string, Tuple<int, int>>();
            if (Directory.Exists(dir))
            {
                foreach (var wadfile in Directory.GetFiles(dir, "*.wad"))
                {
                    var texturesizes = TextureSizesFromWad(wadfile);
                    foreach (var tex in texturesizes)
                    {
                        if (result.ContainsKey(tex.Key))
                        {
                            Log("Warning, {0} is in multiple wads.", tex.Key);
                        }
                        else
                        {
                            result[tex.Key] = tex.Value;
                        }
                    }
                }
            }
            return result;
        }

        public static void CheckTexVecs(XFace xface, TexCoords coords, double width, double height)
        {
            for (int i = 0; i < 3; i++)
            {
                var vert = Vector<double>.Build.DenseOfArray(new double[] { xface.Verts[i].x, xface.Verts[i].y, xface.Verts[i].z });
                var uv_expected = xface.UVs[i];

                var uv_actual = new XUV((vert.DotProduct(coords.right.SubVector(0, 3)) / (width * coords.xscale)) + (coords.right[3] / width),
                                        (vert.DotProduct(coords.up.SubVector(0, 3)) / (height * coords.yscale)) + (coords.up[3] / height));

                if (Math.Abs(uv_actual.U - uv_expected.U) > 0.01
                    || Math.Abs(uv_actual.V - uv_expected.V) > 0.01)
                {
                   Log("Error computing texture vectors");
                }
            }
        }


        public static Vector<double> Vec(params double[] elems)
        {
            return Vector<double>.Build.DenseOfArray(elems);
        }
        public static Vector<double> Vec(XVector xvector)
        {
            return Vec(xvector.x, xvector.y, xvector.z);
        }

        public static bool Close(double a, double b)
        {
            return Math.Abs(a - b) < 0.01;
        }

        public static bool Close(Vector<double> a, Vector<double> b)
        {
            foreach (var element in a.EnumerateIndexed())
            {
                if (!Close(element.Item2, b[element.Item1]))
                    return false;
            }
            return true;
        }

        static Matrix<double> CrossProductMatrix(Vector<double> a)
        {
            // https://en.wikipedia.org/wiki/Rotation_matrix#Rotation_matrix_from_axis_and_angle
            return Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { 0,    -a[2], a[1] },
                { a[2], 0,     -a[0] },
                { -a[1], a[0], 0 }
            });
        }

        static Matrix<double> SelfTensorProduct(Vector<double> u)
        {
            // https://en.wikipedia.org/wiki/Rotation_matrix#Rotation_matrix_from_axis_and_angle
            return Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { u[0] * u[0], u[0] * u[1], u[0] * u[2] },
                { u[0] * u[1], u[1] * u[1], u[1] * u[2] },
                { u[0] * u[2], u[1] * u[2], u[2] * u[2] },
            });
        }

        static Matrix<double> RotationMatrix(Vector<double> axis, double angle)
        {
            axis = axis.Normalize(2);

            // https://en.wikipedia.org/wiki/Rotation_matrix#Rotation_matrix_from_axis_and_angle
            return Math.Cos(angle) * Matrix<double>.Build.DenseIdentity(3, 3)
                + Math.Sin(angle) * CrossProductMatrix(axis)
                + (1 - Math.Cos(angle)) * SelfTensorProduct(axis);
        }

        static Vector<double> RotateAboutVector(Vector<double> axis, double angle, Vector<double> vec)
        {
            return RotationMatrix(axis, angle) * vec;
        }

        public struct TexCoords
        {
            public Vector<double> up;    // 4-vector
            public Vector<double> right; // 4-vector
            public double xscale;
            public double yscale;                       
        }

        public static TexCoords TexCoordsForFace(List<XVector> verts, List<XUV> uvs, XVector faceNormal, double texturewidth, double textureheight)
        {
            double width = texturewidth;
            double height = textureheight;
            var V = verts;
            var T = uvs;

            // Set up "2d world" coordinate system with the 01 edge along the X axis.
            // To do this all we need are the lengths of the 01 and 02 edges, and the
            // 01 to 02 edge angle.

            var world01 = XVector.Subtract(V[1], V[0]);
            var world02 = XVector.Subtract(V[2], V[0]);
            var world01_02Angle = Math.Acos(XVector.Dot(world01.Normalized(), world02.Normalized())); // without sign
            
            // Get the correct sign of world01_02Angle. see http://stackoverflow.com/questions/5188561/signed-angle-between-two-3d-vectors-with-same-origin-within-the-same-plane
            var world01_02Cross = XVector.Cross(world01, world02);
            if (XVector.Dot(faceNormal, world01_02Cross) < 0)
            {
                world01_02Angle = -world01_02Angle;
            }

            var world01_2d = Vec(world01.GetLength(), 0);
            var world02_2d = Vec(Math.Cos(world01_02Angle), Math.Sin(world01_02Angle)) * world02.GetLength();

            Debug.Assert(Close(Math.Acos(world01_2d.Normalize(2).DotProduct(world02_2d.Normalize(2))), Math.Abs(world01_02Angle)));
            Debug.Assert(Close(world02_2d.L2Norm(), world02.GetLength()));

            // Translate 01 and 02 vectors in texture space to the origin.

            var tex01 = new XUV(T[1].U - T[0].U, T[1].V - T[0].V);
            var tex02 = new XUV(T[2].U - T[0].U, T[2].V - T[0].V);

            // Scale tex coords from [0,1] to [0,width/height]
            tex01.U *= width;
            tex02.U *= width;
            tex01.V *= height;
            tex02.V *= height;


            // Find an affine transformation X to convert 
            // world01_2d and world02_2d to their respective UV coords

            /*

            a = world01_2d
            b = world02_2d

            p = tex01
            q = tex02


            [ px ]   [ m11 m12 0 ] [ ax ]
            [ py ] = [ m21 m22 0 ] [ ay ]
            [ 1  ]   [ 0   0   1 ] [ 1  ]

            [ qx ]   [ m11 m12 0 ] [ bx ]
            [ qy ] = [ m21 m22 0 ] [ by ]
            [ 1  ]   [ 0   0   1 ] [ 1  ]
      

            px = ax * m11 + ay * m12
            py = ax * m21 + ay * m22
            qx = bx * m11 + by * m12
            qy = bx * m21 + by * m22


            [ px ]   [ ax ay 0  0  ] [ m11 ]
            [ py ] = [ 0  0  ax ay ] [ m12 ]
            [ qx ]   [ bx by 0  0  ] [ m21 ]
            [ qy ]   [ 0  0  bx by ] [ m22 ]
        
            */

            var TexCoordsVec = Vec(
                tex01.U,
                tex01.V,
                tex02.U,
                tex02.V
            );
            var World2DMatrix = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { world01_2d[0], world01_2d[1], 0            , 0             },
                { 0            , 0            , world01_2d[0], world01_2d[1] },
                { world02_2d[0], world02_2d[1], 0            , 0             },
                { 0            , 0            , world02_2d[0], world02_2d[1] },
            });
            var MCoeffs = World2DMatrix.Solve(TexCoordsVec);

            // Test MCoeffs
            {
                var Xform = Matrix<double>.Build.DenseOfArray(new double[,]
                {
                { MCoeffs[0], MCoeffs[1] },
                { MCoeffs[2], MCoeffs[3] },
                });

                Debug.Assert(Close(Vec(tex01.U, tex01.V), Xform.Multiply(world01_2d)));
                Debug.Assert(Close(Vec(tex02.U, tex02.V), Xform.Multiply(world02_2d)));
            }

            var right_2dworld = MCoeffs.SubVector(0, 2);
            var up_2dworld = MCoeffs.SubVector(2, 2);            

            // These are the final scale values
            var scalex = 1 / right_2dworld.L2Norm();
            var scaley = 1 / up_2dworld.L2Norm();
            
            // Get the angles of the texture axes. These are in the 2d world coordinate system,
            // so they're relative to the 01 vector.
            var up_2dworld_angle = Math.Atan2(up_2dworld[1], up_2dworld[0]);
            var right_2dworld_angle = Math.Atan2(right_2dworld[1], right_2dworld[0]);

            // Recreate the texture axes in 3d world coordinates, using the angles from the 01 edge.
            var up = RotateAboutVector(Vec(faceNormal), up_2dworld_angle, Vec(world01)).Normalize(2);
            var rt = RotateAboutVector(Vec(faceNormal), right_2dworld_angle, Vec(world01)).Normalize(2);

            // Now we just need the offsets.

            var up_full = Vec(up[0], up[1], up[2], 0);
            var rt_full = Vec(rt[0], rt[1], rt[2], 0);

            {
                var test_s = Vec(V[0]).DotProduct(rt) / (width * scalex);
                var test_t = Vec(V[0]).DotProduct(up) / (height * scaley);

                rt_full[3] = (T[0].U - test_s) * width;
                up_full[3] = (T[0].V - test_t) * height;
            }
            
            var result = new TexCoords
            {
                up = up_full,
                right = rt_full,
                xscale = scalex,
                yscale = scaley
            };
            
            return result;
        }

        public static TexCoords TexCoordsForFace(XFace xface, double texwidth, double texheight)
        {
            var result = TexCoordsForFace(xface.Verts, xface.UVs, xface.Normal, texwidth, texheight);

            CheckTexVecs(xface, result, texwidth, texheight);
            return result;
        }

        private static object f(double d)
        {
            return (object)d.ToString("F5", CultureInfo.InvariantCulture);
        }

        public static string DefaultTexCoordsString(string texName)
        {
            return String.Format("{0} [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1", texName);
        }

        public static string TexCoordsStringForFace(XFace xface, Dictionary<string, Tuple<int, int>> texSizes, string fallbackTextureName)
        {
            var texname = xface.TexName;
            var size = new Tuple<int, int>(64, 64);

            if (!String.IsNullOrWhiteSpace(texname))
            {
                if (texSizes.ContainsKey(texname.ToLowerInvariant()))
                {
                    size = texSizes[texname.ToLowerInvariant()];
                }
                else
                {
                    Log("No texture info! Will use SKIP and standard 64x64 size for UV", texname);
                    texname = progressTracker?.GetVisibleTextureName() ?? "DEFAULT";
                }
            }
            else
            {
                Log("No texture info! Will use SKIP and standard 64x64 size for UV", texname);
                texname = progressTracker?.GetVisibleTextureName() ?? "DEFAULT";
            }

            //  If manual texture size selected
            if (progressTracker?.IsWadSearchSizeSelected() == true)
            {
                size = progressTracker.GetWadSearchSize();
            }

            var vecs = TexCoordsForFace(xface, size.Item1, size.Item2);
            var svec = vecs.right;
            var tvec = vecs.up;

            var str = String.Format("{0} [ {1} {2} {3} {4} ] [ {5} {6} {7} {8} ] 0 {9} {10}",
                texname,
                f(svec[0]), f(svec[1]), f(svec[2]), f(svec[3]),
                f(tvec[0]), f(tvec[1]), f(tvec[2]), f(tvec[3]),
                f(vecs.xscale), f(vecs.yscale));

            return str;

            /*
                else
                {
                    Log("Couldn't find texture {0} in a wad. Ignoring the UV coordinates from the OBJ.", texname);

                    return DefaultTexCoordsString(fallbackTextureName);
                }
            */
        }

        public static void testTextureCoordinates()
        {
            // Trivial
            {
                XFace f = new XFace();
                f.Verts.Add(new XVector(0, 0, 0)); //origin
                f.Verts.Add(new XVector(0, 0, 32)); //up
                f.Verts.Add(new XVector(32, 0, 32)); //up and right
                f.UVs.Add(new XUV(0, 0));
                f.UVs.Add(new XUV(0, 0.5)); // 64x64 texture
                f.UVs.Add(new XUV(0.5, 0.5));
                f.ComputeNormal();
                var coords = MAPCreation.TexCoordsForFace(f, 64, 64);
            }

            // Trivial - Scaled up 2x
            {
                XFace f = new XFace();
                f.Verts.Add(new XVector(0, 0, 0)); //origin
                f.Verts.Add(new XVector(0, 0, 64)); //up
                f.Verts.Add(new XVector(64, 0, 64)); //up and right
                f.UVs.Add(new XUV(0, 0));
                f.UVs.Add(new XUV(0, 0.5)); // 64x64 texture, we're only using 1/2 in each dimension, so 2x size scaling
                f.UVs.Add(new XUV(0.5, 0.5));
                f.ComputeNormal();
                var coords = MAPCreation.TexCoordsForFace(f, 64, 64);
            }

            // Trivial - Sheared
            {
                XFace f = new XFace();
                f.Verts.Add(new XVector(0, 0, 0)); //origin
                f.Verts.Add(new XVector(64, 0, 64)); //up
                f.Verts.Add(new XVector(128, 0, 64)); //up and right
                f.UVs.Add(new XUV(0, 0));
                f.UVs.Add(new XUV(0, -1));
                f.UVs.Add(new XUV(-1, -1)); // UVs still cover the full face
                f.ComputeNormal();
                var coords = MAPCreation.TexCoordsForFace(f, 64, 64);
            }

            // from xonotic walker:
            {
                XFace f = new XFace();
                f.Verts.Add(new XVector(55.9375, -36.046875, 71.921875));
                f.Verts.Add(new XVector(53.25, -36.296875, 72.015625));
                f.Verts.Add(new XVector(56.875, -39.40625, 73.5));

                f.UVs.Add(new XUV(-0.518167, 0.818346));
                f.UVs.Add(new XUV(-0.440147, 0.812692));
                f.UVs.Add(new XUV(-0.521813, 0.712665));

                f.ComputeNormal();

                var coords = MAPCreation.TexCoordsForFace(f, 64, 64);
            }
        }

        public static void AddBrushesToMAP(
                                string MAPFilename,
                                StreamWriter logStream,
                                MainFormCompat.MainForm.EConvOption econvOption,
								List<XVector> _Vertices,
								List<XFace> _Faces,
								List<XBrush> _Brushes,
								string format,
								ref StringBuilder _MAPText,
								string _VisibleTextureName,
								string _HiddenTextureName,
								double _Scalar,
                                ref StreamWriter mapFile,
                                MainFormCompat.IProgressTracker progressTracker = null
		                        )
		{
            SetLogger(logStream);

            // Search all wads in the same directory as MAPFilename, we need the texture
            // sizes to be able to use the UV coords in the output map file.
            var waddirectory = Path.GetDirectoryName(MAPFilename);

            //  If WAD path option selected, overwrite it with proper path
            if (progressTracker?.IsWadSearchPathSelected() == true)
            {
                if (!string.IsNullOrEmpty(progressTracker.GetWadSearchPath()))
                {
                    waddirectory = progressTracker.GetWadSearchPath();
                }
            }

            
            var texSizes = TextureSizesInDirectory(waddirectory);

            progressTracker?.UpdateProgress( "Adding Brushes to MAP...");
			int BrushCount = 0;
            int MaxBrushCount = 0;
            int ProgressValue = 0;

            //  Helper variables
            XVector xvector1;
            XVector xvector2;
            XVector xvector3;
            XVector xvector4;
            XVector xvector5;
            XVector xvector6;
            XVector xvector7;
            XVector xvector8;
            XVector xvector9;

            XVector _A;
            XVector _B;
            XVector _B1;
            XVector _B2;

            List<XVector> list3;
            List<XVector> list4;

            switch (econvOption)
			{
				case MainFormCompat.MainForm.EConvOption.Extrusion:
					using (List<XBrush>.Enumerator enumerator = _Brushes.GetEnumerator())
					{
                        //Count amount of brushes
                        for (int i = 0; i < _Brushes.Count; i++)
                            foreach (XFace xface in _Brushes[i].Faces)
                                MaxBrushCount++;

						while (enumerator.MoveNext())
						{
							//_MainForm.UpdateProgress(string.Format("Adding Brush {0:n0} to MAP...", BrushCount++));

							foreach (XFace xface in enumerator.Current.Faces)
							{
                                ProgressValue = MaxBrushCount > 0 ? (int)Math.Floor((float)BrushCount / (float)MaxBrushCount * 100f) : 0;
                                progressTracker?.UpdateProgress(string.Format("Adding Brush {0:n0} / {1:n0} to MAP...", BrushCount++, MaxBrushCount), ProgressValue);

                                _B = XVector.Multiply(xface.Normal, _Scalar);
								list3 = new List<XVector>();
								list4 = new List<XVector>();
								foreach (int index in xface.VertIdx)
								{
									_A = new XVector(_Vertices[index]);
									list3.Add(_A);
									list4.Add(XVector.Add(_A, _B));
								}
								_MAPText.AppendLine("{");
								xvector1 = list3[0];
								xvector2 = list3[1];
								xvector3 = list3[2];
								_MAPText.Append(string.Format("\t( {0} {1} {2} ) ", (object)xvector1.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector1.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector1.z.ToString(format, CultureInfo.InvariantCulture)));
								_MAPText.Append(string.Format("( {0} {1} {2} ) ", (object)xvector3.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector3.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector3.z.ToString(format, CultureInfo.InvariantCulture)));
                                if (xface.UVs != null && xface.UVs.Count > 0)
                                {
                                    _MAPText.AppendLine(string.Format("( {0} {1} {2} ) {3}", (object)xvector2.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.z.ToString(format, CultureInfo.InvariantCulture), TexCoordsStringForFace(xface, texSizes, _VisibleTextureName)));
                                }
                                else
                                {
                                    _MAPText.AppendLine(string.Format("( {0} {1} {2} ) {3}", (object)xvector2.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.z.ToString(format, CultureInfo.InvariantCulture), DefaultTexCoordsString(_VisibleTextureName)));
                                }
                                xvector4 = list4[2];
								xvector5 = list4[1];
								xvector6 = list4[0];
								_MAPText.Append(string.Format("\t( {0} {1} {2} ) ", (object)xvector4.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector4.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector4.z.ToString(format, CultureInfo.InvariantCulture)));
								_MAPText.Append(string.Format("( {0} {1} {2} ) ", (object)xvector6.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector6.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector6.z.ToString(format, CultureInfo.InvariantCulture)));
								_MAPText.AppendLine(string.Format("( {0} {1} {2} ) {3}", (object)xvector5.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector5.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector5.z.ToString(format, CultureInfo.InvariantCulture), DefaultTexCoordsString(_HiddenTextureName)));
								for (int index = 0; index < list3.Count; ++index)
								{
									xvector7 = list3[index];
									xvector8 = list3[(index + 1) % list3.Count];
									xvector9 = list4[index];
									_MAPText.Append(string.Format("\t( {0} {1} {2} ) ", (object)xvector7.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector7.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector7.z.ToString(format, CultureInfo.InvariantCulture)));
									_MAPText.Append(string.Format("( {0} {1} {2} ) ", (object)xvector8.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector8.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector8.z.ToString(format, CultureInfo.InvariantCulture)));
									_MAPText.AppendLine(string.Format("( {0} {1} {2} ) {3}", (object)xvector9.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector9.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector9.z.ToString(format, CultureInfo.InvariantCulture), DefaultTexCoordsString(_HiddenTextureName)));
								}
								_MAPText.AppendLine("}");

                                //if (BrushCount % 100 == 0)
                                //{
                                //mapFile.Write(_MAPText);
                                //_MAPText = "";
                                //}
                            }
                        }
						break;
					}
				case MainFormCompat.MainForm.EConvOption.Spikes:
					using (List<XBrush>.Enumerator enumerator = _Brushes.GetEnumerator())
					{
                        //Count amount of brushes
                        for (int i = 0; i < _Brushes.Count; i++)
                            foreach (XFace xface in _Brushes[i].Faces)
                                MaxBrushCount++;

						while (enumerator.MoveNext())
						{
							//_MainForm.UpdateProgress(string.Format("Adding Brush {0:n0} to MAP...", BrushCount++));
							foreach (XFace xface in enumerator.Current.Faces)
							{
                                ProgressValue = MaxBrushCount > 0 ? (int)Math.Floor((float)BrushCount / (float)MaxBrushCount * 100f) : 0;
                                progressTracker?.UpdateProgress(string.Format("Adding Brush {0:n0} / {1:n0} to MAP...", BrushCount++, MaxBrushCount),ProgressValue);
								_B1 = XVector.Multiply(xface.Normal, _Scalar);
                                list3 = xface.Verts;
								_MAPText.AppendLine("{");
								xvector1 = list3[0];
								xvector2 = list3[1];
								xvector3 = list3[2];
								_MAPText.Append(string.Format("\t( {0} {1} {2} ) ", (object)xvector1.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector1.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector1.z.ToString(format, CultureInfo.InvariantCulture)));
								_MAPText.Append(string.Format("( {0} {1} {2} ) ", (object)xvector3.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector3.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector3.z.ToString(format, CultureInfo.InvariantCulture)));
                                if (xface.UVs != null && xface.UVs.Count > 0)
                                {
                                    _MAPText.AppendLine(string.Format("( {0} {1} {2} ) {3}", (object)xvector2.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.z.ToString(format, CultureInfo.InvariantCulture), TexCoordsStringForFace(xface, texSizes, _VisibleTextureName)));
                                }
                                else
                                {
                                    _MAPText.AppendLine(string.Format("( {0} {1} {2} ) {3}", (object)xvector2.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.z.ToString(format, CultureInfo.InvariantCulture), DefaultTexCoordsString(_VisibleTextureName)));
                                }
                                xvector4 = new XVector();
								foreach (int index in xface.VertIdx)
								{
									_B2 = new XVector(_Vertices[index]);
									xvector4 = XVector.Add(xvector4, _B2);
								}
								xvector5 = XVector.Add(XVector.Divide(xvector4, (double)xface.VertIdx.Count), _B1);
								for (int index = 0; index < list3.Count; ++index)
								{
									xvector6 = list3[index];
									xvector7 = list3[(index + 1) % list3.Count];
									_MAPText.Append(string.Format("\t( {0} {1} {2} ) ", (object)xvector5.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector5.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector5.z.ToString(format, CultureInfo.InvariantCulture)));
									_MAPText.Append(string.Format("( {0} {1} {2} ) ", (object)xvector6.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector6.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector6.z.ToString(format, CultureInfo.InvariantCulture)));
									_MAPText.AppendLine(string.Format("( {0} {1} {2} ) {3}", (object)xvector7.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector7.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector7.z.ToString(format, CultureInfo.InvariantCulture), DefaultTexCoordsString(_HiddenTextureName)));
								}
								_MAPText.AppendLine("}");

                                //if (BrushCount % 100 == 0)
                                //{
                                //mapFile.Write(_MAPText);
                                //_MAPText = "";
                                //}
                            }
						}
						break;
					}
				case MainFormCompat.MainForm.EConvOption.OptimizedSpikes:
					using (List<XBrush>.Enumerator enumerator = _Brushes.GetEnumerator())
					{
                        Log("Starting Optimized Spikes conversion with quad detection...");
                        
                        //Count amount of brushes - this will be updated after optimization
                        for (int i = 0; i < _Brushes.Count; i++)
                            foreach (XFace xface in _Brushes[i].Faces)
                                MaxBrushCount++;

						while (enumerator.MoveNext())
						{
                            var currentBrush = enumerator.Current;
                            
                            // Apply quad optimization to faces in this brush
                            var originalFaces = currentBrush.Faces;
                            var analysis = QuadOptimizer.AnalyzeOptimizationPotential(originalFaces);
                            
                            Log("Brush optimization analysis: {0} triangles, {1} potential merges, {2:F1}% reduction", 
                                analysis.OriginalTriangleCount, analysis.PotentialMerges, analysis.OptimizationPercentage);
                            
                            var optimizedFaces = QuadOptimizer.OptimizeFacesToQuads(originalFaces);
                            
                            // Validate the optimization
                            var validation = QuadOptimizer.ValidateOptimization(originalFaces, optimizedFaces);
                            if (!validation.IsValid)
                            {
                                Log("Warning: Optimization validation failed, using original faces");
                                optimizedFaces = originalFaces;
                            }
                            else
                            {
                                Log("Optimization successful: {0} -> {1} faces", validation.OriginalFaceCount, validation.OptimizedFaceCount);
                            }
                            
							//_MainForm.UpdateProgress(string.Format("Adding Brush {0:n0} to MAP...", BrushCount++));
							foreach (XFace xface in optimizedFaces)
							{
                                ProgressValue = MaxBrushCount > 0 ? (int)Math.Floor((float)BrushCount / (float)MaxBrushCount * 100f) : 0;
                                progressTracker?.UpdateProgress(string.Format("Adding Optimized Brush {0:n0} / {1:n0} to MAP...", BrushCount++, MaxBrushCount),ProgressValue);
								
                                _B1 = XVector.Multiply(xface.Normal, _Scalar);
                                list3 = xface.Verts;
								_MAPText.AppendLine("{");
                                
                                // Handle both triangular and quad faces
                                if (list3.Count == 3)
                                {
                                    // Original triangle spike logic
                                    xvector1 = list3[0];
                                    xvector2 = list3[1];
                                    xvector3 = list3[2];
                                    _MAPText.Append(string.Format("\t( {0} {1} {2} ) ", (object)xvector1.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector1.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector1.z.ToString(format, CultureInfo.InvariantCulture)));
                                    _MAPText.Append(string.Format("( {0} {1} {2} ) ", (object)xvector3.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector3.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector3.z.ToString(format, CultureInfo.InvariantCulture)));
                                    if (xface.UVs != null && xface.UVs.Count > 0)
                                    {
                                        _MAPText.AppendLine(string.Format("( {0} {1} {2} ) {3}", (object)xvector2.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.z.ToString(format, CultureInfo.InvariantCulture), TexCoordsStringForFace(xface, texSizes, _VisibleTextureName)));
                                    }
                                    else
                                    {
                                        _MAPText.AppendLine(string.Format("( {0} {1} {2} ) {3}", (object)xvector2.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.z.ToString(format, CultureInfo.InvariantCulture), DefaultTexCoordsString(_VisibleTextureName)));
                                    }
                                }
                                else if (list3.Count == 4)
                                {
                                    // Optimized quad face logic - create a single quad face instead of triangle spike
                                    // For quads, we create a flat brush instead of a spike for better optimization
                                    xvector1 = list3[0];
                                    xvector2 = list3[1];
                                    xvector3 = list3[2];
                                    var xvector4_quad = list3[3];
                                    
                                    // Create the quad face in proper winding order
                                    _MAPText.Append(string.Format("\t( {0} {1} {2} ) ", (object)xvector1.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector1.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector1.z.ToString(format, CultureInfo.InvariantCulture)));
                                    _MAPText.Append(string.Format("( {0} {1} {2} ) ", (object)xvector3.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector3.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector3.z.ToString(format, CultureInfo.InvariantCulture)));
                                    if (xface.UVs != null && xface.UVs.Count > 0)
                                    {
                                        _MAPText.AppendLine(string.Format("( {0} {1} {2} ) {3}", (object)xvector2.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.z.ToString(format, CultureInfo.InvariantCulture), TexCoordsStringForFace(xface, texSizes, _VisibleTextureName)));
                                    }
                                    else
                                    {
                                        _MAPText.AppendLine(string.Format("( {0} {1} {2} ) {3}", (object)xvector2.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.z.ToString(format, CultureInfo.InvariantCulture), DefaultTexCoordsString(_VisibleTextureName)));
                                    }
                                }
                                else
                                {
                                    // Handle other polygon types by triangulating
                                    Log("Warning: Face with {0} vertices found, using first 3 vertices", list3.Count);
                                    xvector1 = list3[0];
                                    xvector2 = list3[1];
                                    xvector3 = list3[2];
                                    _MAPText.Append(string.Format("\t( {0} {1} {2} ) ", (object)xvector1.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector1.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector1.z.ToString(format, CultureInfo.InvariantCulture)));
                                    _MAPText.Append(string.Format("( {0} {1} {2} ) ", (object)xvector3.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector3.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector3.z.ToString(format, CultureInfo.InvariantCulture)));
                                    _MAPText.AppendLine(string.Format("( {0} {1} {2} ) {3}", (object)xvector2.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.z.ToString(format, CultureInfo.InvariantCulture), DefaultTexCoordsString(_VisibleTextureName)));
                                }
                                
                                // Calculate centroid for spike/extrusion point
                                xvector4 = new XVector();
								foreach (var vert in xface.Verts)
								{
									xvector4 = XVector.Add(xvector4, vert);
								}
								xvector5 = XVector.Add(XVector.Divide(xvector4, (double)xface.Verts.Count), _B1);
                                
								// Create side faces
								for (int index = 0; index < list3.Count; ++index)
								{
									xvector6 = list3[index];
									xvector7 = list3[(index + 1) % list3.Count];
									_MAPText.Append(string.Format("\t( {0} {1} {2} ) ", (object)xvector5.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector5.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector5.z.ToString(format, CultureInfo.InvariantCulture)));
									_MAPText.Append(string.Format("( {0} {1} {2} ) ", (object)xvector6.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector6.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector6.z.ToString(format, CultureInfo.InvariantCulture)));
									_MAPText.AppendLine(string.Format("( {0} {1} {2} ) {3}", (object)xvector7.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector7.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector7.z.ToString(format, CultureInfo.InvariantCulture), DefaultTexCoordsString(_HiddenTextureName)));
								}
								_MAPText.AppendLine("}");

                                //if (BrushCount % 100 == 0)
                                //{
                                //mapFile.Write(_MAPText);
                                //_MAPText = "";
                                //}
                            }
						}
						break;
					}
				default:
					using (List<XBrush>.Enumerator enumerator = _Brushes.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							XBrush current = enumerator.Current;
							_MAPText.AppendLine("{");
                            //_MainForm.UpdateProgress(string.Format("Adding Brush {0:n0} to MAP...", BrushCount++));
                            ProgressValue = MaxBrushCount > 0 ? (int)Math.Floor((float)BrushCount / (float)MaxBrushCount * 100f) : 0;
                            progressTracker?.UpdateProgress(string.Format("Adding Brush {0:n0} / {1:n0} to MAP...", BrushCount++, _Brushes.Count),ProgressValue);

							foreach (XFace xface in current.Faces)
							{
								xvector1 = _Vertices[xface.VertIdx[0]];
								xvector2 = _Vertices[xface.VertIdx[2]];
								xvector3 = _Vertices[xface.VertIdx[1]];
								_MAPText.Append(string.Format("\t( {0} {1} {2} ) ", (object)xvector1.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector1.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector1.z.ToString(format, CultureInfo.InvariantCulture)));
								_MAPText.Append(string.Format("( {0} {1} {2} ) ", (object)xvector2.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector2.z.ToString(format, CultureInfo.InvariantCulture)));
                                if (xface.UVs != null && xface.UVs.Count > 0)
                                {
                                    _MAPText.AppendLine(string.Format("( {0} {1} {2} ) {3}", (object)xvector3.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector3.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector3.z.ToString(format, CultureInfo.InvariantCulture), TexCoordsStringForFace(xface, texSizes, _VisibleTextureName)));
                                }
                                else
                                {
                                    _MAPText.AppendLine(string.Format("( {0} {1} {2} ) {3}", (object)xvector3.x.ToString(format, CultureInfo.InvariantCulture), (object)xvector3.y.ToString(format, CultureInfo.InvariantCulture), (object)xvector3.z.ToString(format, CultureInfo.InvariantCulture), DefaultTexCoordsString(_VisibleTextureName)));
                                }
                            }
							_MAPText.AppendLine("}");

                            //if (BrushCount % 100 == 0)
                            //{
                            //mapFile.Write(_MAPText);
                            //_MAPText = "";
                            //}
                        }
                    }
                    break;
            }

            SetLogger(null);
        }
	}
}
