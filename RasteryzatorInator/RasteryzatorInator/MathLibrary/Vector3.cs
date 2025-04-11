using System;
using System.Collections.Generic;
namespace RasteryzatorInator.MathLibrary;

public struct Vector3
{
    public float X, Y, Z;

    public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }

    public static Vector3 Zero => new Vector3(0, 0, 0);
    public static Vector3 One => new Vector3(1, 1, 1);
    public static Vector3 UnitX => new Vector3(1, 0, 0);
    public static Vector3 UnitY => new Vector3(0, 1, 0);
    public static Vector3 UnitZ => new Vector3(0, 0, 1);
    public static Vector3 Up => UnitY;
    public static Vector3 Down => -UnitY;
    public static Vector3 Left => -UnitX;
    public static Vector3 Right => UnitX;
    public static Vector3 Forward => -UnitZ;
    public static Vector3 Backward => UnitZ;

    public float LengthSquared() => X * X + Y * Y + Z * Z;
    public float Length() => MathF.Sqrt(LengthSquared());

    public void Normalize()
    {
        float len = Length();
        if (MathF.Abs(len) < Values.Epsilon) return;
        float invLen = 1.0f / len;
        X *= invLen; Y *= invLen; Z *= invLen;
    }

    public Vector3 Normalized()
    {
        float len = Length();
        if (MathF.Abs(len) < Values.Epsilon) return Zero;
        float invLen = 1.0f / len;
        return new Vector3(X * invLen, Y * invLen, Z * invLen);
    }

    public static float Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    public static Vector3 Cross(Vector3 a, Vector3 b) =>
        new Vector3(a.Y * b.Z - a.Z * b.Y,
                  a.Z * b.X - a.X * b.Z,
                  a.X * b.Y - a.Y * b.X);

    public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3 operator -(Vector3 a) => new Vector3(-a.X, -a.Y, -a.Z);
    public static Vector3 operator *(Vector3 a, float scalar) => new Vector3(a.X * scalar, a.Y * scalar, a.Z * scalar);
    public static Vector3 operator *(float scalar, Vector3 a) => a * scalar;
    public static Vector3 operator /(Vector3 a, float scalar)
    {
        if (MathF.Abs(scalar) < Values.Epsilon) throw new DivideByZeroException();
        float inv = 1.0f / scalar;
        return new Vector3(a.X * inv, a.Y * inv, a.Z * inv);
    }

    public override string ToString() => $"({X:F3}, {Y:F3}, {Z:F3})";
}
