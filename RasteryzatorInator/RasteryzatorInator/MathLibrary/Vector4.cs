namespace RasteryzatorInator.MathLibrary
{
    public struct Vector4
    {
        public float X, Y, Z, W;

        public Vector4(float x, float y, float z, float w) { X = x; Y = y; Z = z; W = w; }
        public Vector4(Vector3 v, float w) : this(v.X, v.Y, v.Z, w) { }

        public static Vector4 Zero => new Vector4(0, 0, 0, 0);
        public static Vector4 One => new Vector4(1, 1, 1, 1);

        public float this[int index]
        {
            get => index switch 
            { 
                0 => X, 
                1 => Y, 
                2 => Z, 
                3 => W, 
                _ => throw new IndexOutOfRangeException() 
            };

            set 
            { 
                switch (index) 
                { 
                    case 0: X = value; break; 
                    case 1: Y = value; break; 
                    case 2: Z = value; break; 
                    case 3: W = value; break; 
                    default: throw new IndexOutOfRangeException(); 
                } 
            }
        }

        public static Vector4 operator +(Vector4 a, Vector4 b) => new Vector4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        public static Vector4 operator -(Vector4 a, Vector4 b) => new Vector4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        public static Vector4 operator -(Vector4 a) => new Vector4(-a.X, -a.Y, -a.Z, -a.W);
        public static Vector4 operator *(Vector4 a, float scalar) => new Vector4(a.X * scalar, a.Y * scalar, a.Z * scalar, a.W * scalar);
        public static Vector4 operator *(float scalar, Vector4 a) => a * scalar;
        public static Vector4 operator /(Vector4 a, float scalar)
        {
            if (MathF.Abs(scalar) < Values.Epsilon) throw new DivideByZeroException();
            float inv = 1.0f / scalar;
            return new Vector4(a.X * inv, a.Y * inv, a.Z * inv, a.W * inv);
        }

        public override string ToString() => $"({X:F3}, {Y:F3}, {Z:F3}, {W:F3})";
    }
}
