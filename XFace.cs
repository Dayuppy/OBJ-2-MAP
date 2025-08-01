﻿//	Copyright (c) 2015, Warren Marshall <warren@warrenmarshall.biz>
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

// Decompiled source. Needs refactoring

using System.Collections.Generic;

namespace OBJ2MAP
{
  public class XFace
  {
    public List<int> VertIdx;
    public List<int> UVIdx;
    public List<XVector> Verts;
    public List<XUV> UVs;

    public XVector Normal;
    public string TexName;

    public XFace()
    {
            this.Verts = new List<XVector>();
            this.UVs = new List<XUV>();
      this.VertIdx = new List<int>();
      this.UVIdx = new List<int>();
      this.Normal = (XVector) null;
    }

    public XVector ComputeNormal(ref List<XVector> _Vertices)
    {
      this.Normal = new XVector();
      XVector _A1 = _Vertices[this.VertIdx[0]];
      XVector _B = _Vertices[this.VertIdx[1]];
      XVector _A2 = _Vertices[this.VertIdx[2]];
      this.Normal = XVector.Cross(XVector.Subtract(_A1, _B), XVector.Subtract(_A2, _B));
      this.Normal.Normalize();
      return this.Normal;
    }

    public void ComputeNormal()
    {
        this.Normal = XVector.Cross(XVector.Subtract(Verts[0], Verts[1]), XVector.Subtract(Verts[2], Verts[1]));
        this.Normal.Normalize();
    }
    }
}
