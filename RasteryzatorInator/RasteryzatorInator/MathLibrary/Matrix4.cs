namespace RasteryzatorInator.MathLibrary;

public static class Values
{
    public const float Epsilon = 0.00001f;
} 

public struct Matrix4
{
    public Vector4 Col0, Col1, Col2, Col3;

    public Matrix4(bool identity)
    {
        if (identity)
        {
            Col0 = new Vector4(1, 0, 0, 0);
            Col1 = new Vector4(0, 1, 0, 0);
            Col2 = new Vector4(0, 0, 1, 0);
            Col3 = new Vector4(0, 0, 0, 1);
        }
        else
        {
            Col0 = Col1 = Col2 = Col3 = Vector4.Zero;
        }
    }

    public Matrix4(Vector4 c0, Vector4 c1, Vector4 c2, Vector4 c3)
    {
        Col0 = c0; Col1 = c1; Col2 = c2; Col3 = c3;
    }

    public static Matrix4 Identity => new Matrix4(true);

    public float this[int row, int col]
    {
        get => col switch
        {
            0 => Col0[row],
            1 => Col1[row],
            2 => Col2[row],
            3 => Col3[row],
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (col)
            {
                case 0: Col0[row] = value; break;
                case 1: Col1[row] = value; break;
                case 2: Col2[row] = value; break;
                case 3: Col3[row] = value; break;
                default: throw new IndexOutOfRangeException();
            }
        }
    }

    // (M1 * M2)
    public static Matrix4 operator *(Matrix4 m1, Matrix4 m2)
    {
        Matrix4 result = new Matrix4(false);
        result.Col0 = m1 * m2.Col0;
        result.Col1 = m1 * m2.Col1;
        result.Col2 = m1 * m2.Col2;
        result.Col3 = m1 * m2.Col3;
        return result;
    }

    // (M * v)
    // Macierz * Wektor (kolumnowy)
    public static Vector4 operator *(Matrix4 m, Vector4 v)
    {
        return new Vector4(
            m[0, 0] * v.X + m[0, 1] * v.Y + m[0, 2] * v.Z + m[0, 3] * v.W, // Dot(Row0, v)
            m[1, 0] * v.X + m[1, 1] * v.Y + m[1, 2] * v.Z + m[1, 3] * v.W, // Dot(Row1, v)
            m[2, 0] * v.X + m[2, 1] * v.Y + m[2, 2] * v.Z + m[2, 3] * v.W, // Dot(Row2, v)
            m[3, 0] * v.X + m[3, 1] * v.Y + m[3, 2] * v.Z + m[3, 3] * v.W  // Dot(Row3, v)
        );
    }


    // Macierze Transformacje
    public static Matrix4 CreateTranslation(Vector3 v) => new Matrix4(
        new Vector4(1, 0, 0, 0),
        new Vector4(0, 1, 0, 0),
        new Vector4(0, 0, 1, 0),
        new Vector4(v.X, v.Y, v.Z, 1)
    );

    public static Matrix4 CreateScale(Vector3 v) => new Matrix4(
        new Vector4(v.X, 0, 0, 0),
        new Vector4(0, v.Y, 0, 0),
        new Vector4(0, 0, v.Z, 0),
        new Vector4(0, 0, 0, 1)
    );

    public static Matrix4 CreateRotationX(float angleRad)
    {
        float c = MathF.Cos(angleRad);
        float s = MathF.Sin(angleRad);
        return new Matrix4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, c, s, 0),
            new Vector4(0, -s, c, 0),
            new Vector4(0, 0, 0, 1)
        );
    }
    public static Matrix4 CreateRotationY(float angleRad)
    {
        float c = MathF.Cos(angleRad);
        float s = MathF.Sin(angleRad);
        return new Matrix4(
            new Vector4(c, 0, -s, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(s, 0, c, 0),
            new Vector4(0, 0, 0, 1)
        );
    }
    public static Matrix4 CreateRotationZ(float angleRad)
    {
        float c = MathF.Cos(angleRad);
        float s = MathF.Sin(angleRad);
        return new Matrix4(
            new Vector4(c, s, 0, 0),
            new Vector4(-s, c, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 0, 0, 1)
        );
    }

    public static Matrix4 CreateFromAxisAngle(Vector3 axis, float angleRad)
    {
        axis.Normalize();
        float x = axis.X, y = axis.Y, z = axis.Z;
        float c = MathF.Cos(angleRad);
        float s = MathF.Sin(angleRad);
        float t = 1.0f - c;

        return new Matrix4(
            new Vector4(t * x * x + c, t * x * y + s * z, t * x * z - s * y, 0),
            new Vector4(t * x * y - s * z, t * y * y + c, t * y * z + s * x, 0),
            new Vector4(t * x * z + s * y, t * y * z - s * x, t * z * z + c, 0),
            new Vector4(0, 0, 0, 1)
        );
        //return new Matrix4(
        //    new Vector4(t * x * x + c, t * x * y - s * z, t * x * z + s * y, 0), // Column 0
        //    new Vector4(t * x * y + s * z, t * y * y + c, t * y * z - s * x, 0), // Column 1
        //    new Vector4(t * x * z - s * y, t * y * z + s * x, t * z * z + c, 0), // Column 2
        //    new Vector4(0, 0, 0, 1)  // Column 3
        //);
    }

    // Widok i Projekcja

    // (World -> View)
    public static Matrix4 LookAt(Vector3 eye, Vector3 focusPoint, Vector3 up)
    {
        Vector3 zaxis = (eye - focusPoint).Normalized();
        Vector3 xaxis = Vector3.Cross(up, zaxis).Normalized();
        Vector3 yaxis = Vector3.Cross(zaxis, xaxis);

        Matrix4 rotationT = new Matrix4(
            new Vector4(xaxis.X, xaxis.Y, xaxis.Z, 0),
            new Vector4(yaxis.X, yaxis.Y, yaxis.Z, 0),
            new Vector4(zaxis.X, zaxis.Y, zaxis.Z, 0),
            new Vector4(0, 0, 0, 1)
        );

        Matrix4 translation = CreateTranslation(-eye);

        return rotationT * translation;
    }

    // (View -> Clip)
    public static Matrix4 CreatePerspectiveFieldOfView(float fovYRadians, float aspectRatio, float nearPlane, float farPlane)
    {
        if (fovYRadians <= 0 || fovYRadians >= MathF.PI) throw new ArgumentOutOfRangeException(nameof(fovYRadians));
        if (aspectRatio <= 0) throw new ArgumentOutOfRangeException(nameof(aspectRatio));
        if (nearPlane <= 0) throw new ArgumentOutOfRangeException(nameof(nearPlane));
        if (farPlane <= nearPlane) throw new ArgumentOutOfRangeException(nameof(farPlane));

        float f = 1.0f / MathF.Tan(fovYRadians / 2.0f);
        float rangeInv = 1.0f / (nearPlane - farPlane);

        return new Matrix4(
            new Vector4(f / aspectRatio, 0, 0, 0),
            new Vector4(0, f, 0, 0),
            new Vector4(0, 0, (farPlane + nearPlane) * rangeInv, -1.0f),
            new Vector4(0, 0, 2.0f * farPlane * nearPlane * rangeInv, 0)
        );
    }

    public override string ToString()
    {
        return $"[{this[0, 0]:F3} {this[0, 1]:F3} {this[0, 2]:F3} {this[0, 3]:F3}]\n" +
               $"[{this[1, 0]:F3} {this[1, 1]:F3} {this[1, 2]:F3} {this[1, 3]:F3}]\n" +
               $"[{this[2, 0]:F3} {this[2, 1]:F3} {this[2, 2]:F3} {this[2, 3]:F3}]\n" +
               $"[{this[3, 0]:F3} {this[3, 1]:F3} {this[3, 2]:F3} {this[3, 3]:F3}]";
    }
}