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

using System.Collections.Generic;

namespace OBJ2MAP
{
    public class XFace
    {
        public List<int> VertIdx { get; set; }
        public List<int> UVIdx { get; set; }
        public List<XVector> Verts { get; set; }
        public List<XUV> UVs { get; set; }

        public XVector? Normal { get; set; }
        public string? TexName { get; set; }

        public XFace()
        {
            Verts = new List<XVector>();
            UVs = new List<XUV>();
            VertIdx = new List<int>();
            UVIdx = new List<int>();
            Normal = null;
        }

        public XVector ComputeNormal(ref List<XVector> vertices)
        {
            Normal = new XVector();
            var vertex1 = vertices[VertIdx[0]];
            var vertex2 = vertices[VertIdx[1]];
            var vertex3 = vertices[VertIdx[2]];
            Normal = XVector.Cross(XVector.Subtract(vertex1, vertex2), XVector.Subtract(vertex3, vertex2));
            Normal.Normalize();
            return Normal;
        }

        public void ComputeNormal()
        {
            Normal = XVector.Cross(XVector.Subtract(Verts[0], Verts[1]), XVector.Subtract(Verts[2], Verts[1]));
            Normal.Normalize();
        }
    }
}
