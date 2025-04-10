namespace RasteryzatorInator
{
    internal class Vector4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public Vector4(float x, float y, float z, float w) { X = x; Y = y; Z = z; W = w; }
        public Vector4(Vector3 v, float w = 1.0f) { X = v.X; Y = v.Y; Z = v.Z; W = w; }

        public static Vector4 Zero => new Vector4(0, 0, 0, 0);

        public float this[int index]
        {
            get => index switch { 0 => X, 1 => Y, 2 => Z, 3 => W, _ => throw new IndexOutOfRangeException() };
            set { switch (index) { case 0: X = value; break; case 1: Y = value; break; case 2: Z = value; break; case 3: W = value; break; default: throw new IndexOutOfRangeException(); } }
        }

        public static Vector4 operator *(Matrix4 m, Vector4 v)
        {
            float x = m[0, 0] * v.X + m[1, 0] * v.Y + m[2, 0] * v.Z + m[3, 0] * v.W; // Row 0 dot v
            float y = m[0, 1] * v.X + m[1, 1] * v.Y + m[2, 1] * v.Z + m[3, 1] * v.W; // Row 1 dot v
            float z = m[0, 2] * v.X + m[1, 2] * v.Y + m[2, 2] * v.Z + m[3, 2] * v.W; // Row 2 dot v
            float w = m[0, 3] * v.X + m[1, 3] * v.Y + m[2, 3] * v.Z + m[3, 3] * v.W; // Row 3 dot v

            return new Vector4(x, y, z, w);
        }


        public static Vector4 operator *(Vector4 v, float scalar) => new Vector4(v.X * scalar, v.Y * scalar, v.Z * scalar, v.W * scalar);
        public static Vector4 operator *(float scalar, Vector4 v) => v * scalar;
        public static float operator *(Vector4 v4, Vector3 v3) => v4.X * v3.X + v4.Y * v3.Y + v4.Z * v3.Z + v4.W;
        public static Vector4 operator +(Vector4 v1, Vector4 v2) => new Vector4(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z, v1.W + v2.W);
    }
}
