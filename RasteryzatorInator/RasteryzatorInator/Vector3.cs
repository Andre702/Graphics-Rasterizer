using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RasteryzatorInator
{
    internal class Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public float R { get => X; set => X = value; }
        public float G { get => Y; set => Y = value; }
        public float B { get => Z; set => Z = value; }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 Zero => new Vector3(0, 0, 0);

        public static readonly Vector3 DefaultUp = new Vector3(0, 1, 0);

        public float this[int index]
        {
            get => index switch { 0 => X, 1 => Y, 2 => Z, _ => throw new IndexOutOfRangeException() };
            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public float Length()
        {
            return MathF.Sqrt(LengthSquared());
        }

        public float LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        public void Normalize()
        {
            float len = Length();
            if (len == 0)
            {
                throw new DivideByZeroException();
            }
            float invLen = 1.0f / len;
            X *= invLen;
            Y *= invLen;
            Z *= invLen;
        }

        public Vector3 Normalized()
        {
            float len = Length();
            if (len == 0)
            {
                throw new DivideByZeroException();
            }
            float invLen = 1.0f / len;
            return new Vector3(X*invLen, Y*invLen, Z*invLen);
        }


        public static float Dot(Vector3 v1, Vector3 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        public static Vector3 Cross(Vector3 v1, Vector3 v2)
        {
            return new Vector3(
                v1.Y * v2.Z - v1.Z * v2.Y,
                v1.Z * v2.X - v1.X * v2.Z,
                v1.X * v2.Y - v1.Y * v2.X
            );
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector3 operator *(Vector3 v, float scalar)
        {
            return new Vector3(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        public static Vector3 operator *(float scalar, Vector3 v)
        {
            return v * scalar;
        }

        public static Vector3 operator /(Vector3 v, float scalar)
        {
            if (scalar == 0) throw new DivideByZeroException();
            float inv = 1.0f / scalar;
            return new Vector3(v.X * inv, v.Y * inv, v.Z * inv);
        }

        public static Vector3 operator -(Vector3 v)
        {
            return new Vector3(-v.X, -v.Y, -v.Z);
        }

        public static Vector4 operator *(Matrix4 m, Vector3 v)
        {
            return m * new Vector4(v, 1.0f);
        }
    }
}
