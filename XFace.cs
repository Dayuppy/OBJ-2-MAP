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

// Decompiled source. Needs refactoring

using System;
using System.Collections.Generic;

namespace OBJ2MAP
{
  public class XFace
  {
    public List<int> VertIdx;
    public List<int> UVIdx;
    public List<XVector> Verts;
    public List<XUV> UVs;

    public XVector Normal = null!;
    public string TexName = null!;

    public XFace()
    {
            this.Verts = new List<XVector>();
            this.UVs = new List<XUV>();
      this.VertIdx = new List<int>();
      this.UVIdx = new List<int>();
      this.Normal = null!;
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

    /// <summary>
    /// Checks if this face is coplanar with another face within the specified tolerance.
    /// Used for quad detection and face merging optimization.
    /// </summary>
    /// <param name="other">The other face to compare with</param>
    /// <param name="tolerance">Tolerance for coplanarity check (default: 0.001)</param>
    /// <returns>True if faces are coplanar within tolerance</returns>
    public bool IsCoplanar(XFace other, double tolerance = 0.001)
    {
        if (this.Normal == null || other.Normal == null) return false;
        
        // Check if normals are parallel (dot product close to 1 or -1)
        double dotProduct = Math.Abs(XVector.Dot(this.Normal, other.Normal));
        if (Math.Abs(dotProduct - 1.0) > tolerance) return false;
        
        // Check if a vertex from other face lies on this face's plane
        if (this.Verts.Count == 0 || other.Verts.Count == 0) return false;
        
        XVector planePoint = this.Verts[0];
        XVector testPoint = other.Verts[0];
        
        // Calculate distance from test point to this face's plane
        XVector pointDiff = XVector.Subtract(testPoint, planePoint);
        double distance = Math.Abs(XVector.Dot(pointDiff, this.Normal));
        
        return distance <= tolerance;
    }

    /// <summary>
    /// Finds shared edges between this face and another face.
    /// Returns the indices of vertices that form shared edges.
    /// </summary>
    /// <param name="other">The other face to check for shared edges</param>
    /// <param name="tolerance">Tolerance for vertex comparison (default: 0.001)</param>
    /// <returns>List of shared edge vertex pairs (thisVertIndex1, thisVertIndex2, otherVertIndex1, otherVertIndex2)</returns>
    public List<(int thisVert1, int thisVert2, int otherVert1, int otherVert2)> FindSharedEdges(XFace other, double tolerance = 0.001)
    {
        var sharedEdges = new List<(int, int, int, int)>();
        
        for (int i = 0; i < this.Verts.Count; i++)
        {
            int nextI = (i + 1) % this.Verts.Count;
            XVector v1 = this.Verts[i];
            XVector v2 = this.Verts[nextI];
            
            for (int j = 0; j < other.Verts.Count; j++)
            {
                int nextJ = (j + 1) % other.Verts.Count;
                XVector ov1 = other.Verts[j];
                XVector ov2 = other.Verts[nextJ];
                
                // Check if edge (v1,v2) matches edge (ov1,ov2) in either direction
                if ((VerticesAreEqual(v1, ov1, tolerance) && VerticesAreEqual(v2, ov2, tolerance)) ||
                    (VerticesAreEqual(v1, ov2, tolerance) && VerticesAreEqual(v2, ov1, tolerance)))
                {
                    sharedEdges.Add((i, nextI, j, nextJ));
                }
            }
        }
        
        return sharedEdges;
    }

    /// <summary>
    /// Checks if two vertices are equal within the specified tolerance.
    /// </summary>
    private bool VerticesAreEqual(XVector v1, XVector v2, double tolerance)
    {
        return Math.Abs(v1.x - v2.x) <= tolerance &&
               Math.Abs(v1.y - v2.y) <= tolerance &&
               Math.Abs(v1.z - v2.z) <= tolerance;
    }

    /// <summary>
    /// Attempts to merge this triangular face with another triangular face to form a quad.
    /// This is used for brush optimization - converting two triangle brushes into one quad brush.
    /// </summary>
    /// <param name="other">The other triangular face to merge with</param>
    /// <param name="tolerance">Tolerance for geometric comparisons</param>
    /// <returns>A new XFace representing the merged quad, or null if merge is not possible</returns>
    public XFace? TryMergeToQuad(XFace other, double tolerance = 0.001)
    {
        // Both faces must be triangles
        if (this.Verts.Count != 3 || other.Verts.Count != 3) return null;
        
        // Faces must be coplanar
        if (!IsCoplanar(other, tolerance)) return null;
        
        // Find shared edges
        var sharedEdges = FindSharedEdges(other, tolerance);
        if (sharedEdges.Count != 1) return null; // Must share exactly one edge
        
        var sharedEdge = sharedEdges[0];
        
        // Get the non-shared vertices from each triangle
        var thisNonSharedVerts = new List<int>();
        var otherNonSharedVerts = new List<int>();
        
        for (int i = 0; i < 3; i++)
        {
            if (i != sharedEdge.thisVert1 && i != sharedEdge.thisVert2)
                thisNonSharedVerts.Add(i);
        }
        
        for (int i = 0; i < 3; i++)
        {
            if (i != sharedEdge.otherVert1 && i != sharedEdge.otherVert2)
                otherNonSharedVerts.Add(i);
        }
        
        if (thisNonSharedVerts.Count != 1 || otherNonSharedVerts.Count != 1) return null;
        
        // Create the quad by ordering vertices properly
        // We want to create a quad with proper winding order
        var quadVerts = new List<XVector>();
        var quadVertIdx = new List<int>();
        var quadUVs = new List<XUV>();
        var quadUVIdx = new List<int>();
        
        // Start with the non-shared vertex from the first triangle
        int startVert = thisNonSharedVerts[0];
        quadVerts.Add(this.Verts[startVert]);
        if (this.VertIdx.Count > startVert) quadVertIdx.Add(this.VertIdx[startVert]);
        if (this.UVs.Count > startVert) quadUVs.Add(this.UVs[startVert]);
        if (this.UVIdx.Count > startVert) quadUVIdx.Add(this.UVIdx[startVert]);
        
        // Add the shared edge vertices in order
        quadVerts.Add(this.Verts[sharedEdge.thisVert1]);
        if (this.VertIdx.Count > sharedEdge.thisVert1) quadVertIdx.Add(this.VertIdx[sharedEdge.thisVert1]);
        if (this.UVs.Count > sharedEdge.thisVert1) quadUVs.Add(this.UVs[sharedEdge.thisVert1]);
        if (this.UVIdx.Count > sharedEdge.thisVert1) quadUVIdx.Add(this.UVIdx[sharedEdge.thisVert1]);
        
        quadVerts.Add(this.Verts[sharedEdge.thisVert2]);
        if (this.VertIdx.Count > sharedEdge.thisVert2) quadVertIdx.Add(this.VertIdx[sharedEdge.thisVert2]);
        if (this.UVs.Count > sharedEdge.thisVert2) quadUVs.Add(this.UVs[sharedEdge.thisVert2]);
        if (this.UVIdx.Count > sharedEdge.thisVert2) quadUVIdx.Add(this.UVIdx[sharedEdge.thisVert2]);
        
        // Add the non-shared vertex from the other triangle
        int otherVert = otherNonSharedVerts[0];
        quadVerts.Add(other.Verts[otherVert]);
        if (other.VertIdx.Count > otherVert) quadVertIdx.Add(other.VertIdx[otherVert]);
        if (other.UVs.Count > otherVert) quadUVs.Add(other.UVs[otherVert]);
        if (other.UVIdx.Count > otherVert) quadUVIdx.Add(other.UVIdx[otherVert]);
        
        // Create the merged quad face
        var quadFace = new XFace();
        quadFace.Verts = quadVerts;
        quadFace.VertIdx = quadVertIdx;
        quadFace.UVs = quadUVs;
        quadFace.UVIdx = quadUVIdx;
        quadFace.TexName = this.TexName; // Use texture from first face
        
        // Compute normal for the quad
        quadFace.ComputeNormal();
        
        // Validate that the quad is properly formed (non-degenerate)
        if (!IsValidQuad(quadFace, tolerance)) return null;
        
        return quadFace;
    }

    /// <summary>
    /// Validates that a quad face is properly formed and non-degenerate.
    /// </summary>
    /// <param name="quadFace">The quad face to validate</param>
    /// <param name="tolerance">Tolerance for geometric validation</param>
    /// <returns>True if the quad is valid</returns>
    private bool IsValidQuad(XFace quadFace, double tolerance)
    {
        if (quadFace.Verts.Count != 4) return false;
        
        // Check that vertices are not collinear
        for (int i = 0; i < 4; i++)
        {
            var v1 = quadFace.Verts[i];
            var v2 = quadFace.Verts[(i + 1) % 4];
            var v3 = quadFace.Verts[(i + 2) % 4];
            
            // Calculate vectors
            var vec1 = XVector.Subtract(v2, v1);
            var vec2 = XVector.Subtract(v3, v2);
            
            // Check if cross product is non-zero (not collinear)
            var cross = XVector.Cross(vec1, vec2);
            if (cross.GetLength() < tolerance) return false;
        }
        
        // Check that the quad is roughly planar
        var normal1 = XVector.Cross(
            XVector.Subtract(quadFace.Verts[1], quadFace.Verts[0]),
            XVector.Subtract(quadFace.Verts[2], quadFace.Verts[0])
        );
        normal1.Normalize();
        
        var normal2 = XVector.Cross(
            XVector.Subtract(quadFace.Verts[2], quadFace.Verts[0]),
            XVector.Subtract(quadFace.Verts[3], quadFace.Verts[0])
        );
        normal2.Normalize();
        
        double normalDot = Math.Abs(XVector.Dot(normal1, normal2));
        return Math.Abs(normalDot - 1.0) <= tolerance;
    }
}
}
