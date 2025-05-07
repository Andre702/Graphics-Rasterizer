

namespace RasteryzatorInator.MathLibrary
{
    public struct Vector2
    {
        public float X;
        public float Y;

        public static readonly Vector2 Zero = new Vector2(0, 0);
        public static readonly Vector2 One = new Vector2(1, 1);
        public static readonly Vector2 UnitX = new Vector2(1, 0);
        public static readonly Vector2 UnitY = new Vector2(0, 1);

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float Length()
        {
            return MathF.Sqrt(X * X + Y * Y);
        }

        public float LengthSquared()
        {
            return X * X + Y * Y;
        }

        public void Normalize()
        {
            float length = Length();
            if (length > 0.00001f) // Epsilon
            {
                X /= length;
                Y /= length;
            }
            else
            {
                X = 0;
                Y = 0;
            }
        }

        public Vector2 Normalized()
        {
            float length = Length();
            if (length > 0.00001f) // Epsilon
            {
                return new Vector2(X / length, Y / length);
            }
            return Zero;
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Vector2(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t
            );
        }

        public static float Dot(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator *(Vector2 a, float scalar)
        {
            return new Vector2(a.X * scalar, a.Y * scalar);
        }

        public static Vector2 operator *(float scalar, Vector2 a)
        {
            return new Vector2(a.X * scalar, a.Y * scalar);
        }

        public static Vector2 operator /(Vector2 a, float scalar)
        {
            if (Math.Abs(scalar) < 0.00001f) // Epsilon
                return Zero;
            return new Vector2(a.X / scalar, a.Y / scalar);
        }

        public static bool operator ==(Vector2 a, Vector2 b)
        {
            return Math.Abs(a.X - b.X) < 0.00001f && Math.Abs(a.Y - b.Y) < 0.00001f; // Porównanie z Epsilon
        }

        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            return obj is Vector2 vector && this == vector;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
