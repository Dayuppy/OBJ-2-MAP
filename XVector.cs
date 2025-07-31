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

namespace OBJ2MAP
{
    public class XVector
    {
        public double x;
        public double y;
        public double z;

        public XVector()
        {
            x = y = z = 0.0;
        }

        public XVector(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public XVector(XVector other)
        {
            x = other.x;
            y = other.y;
            z = other.z;
        }

        public XVector Normalize()
        {
            double sizeSquared = GetSizeSquared();
            if (sizeSquared == 0.0)
                return this;
            x /= sizeSquared;
            y /= sizeSquared;
            z /= sizeSquared;
            return this;
        }

        public XVector Normalized()
        {
            var copy = new XVector(this);
            copy.Normalize();
            return copy;
        }

        public double GetSizeSquared()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        public double GetLength()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        public static XVector Cross(XVector a, XVector b)
        {
            return new XVector(
                a.y * b.z - a.z * b.y,
                a.z * b.x - a.x * b.z,
                a.x * b.y - a.y * b.x
            );
        }

        public static double Dot(XVector a, XVector b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static XVector Subtract(XVector a, XVector b)
        {
            return new XVector(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static XVector Add(XVector a, XVector b)
        {
            return new XVector(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static XVector Multiply(XVector vector, double scalar)
        {
            return new XVector(vector.x * scalar, vector.y * scalar, vector.z * scalar);
        }

        public static XVector Divide(XVector vector, double scalar)
        {
            return new XVector(vector.x / scalar, vector.y / scalar, vector.z / scalar);
        }
    }
}
