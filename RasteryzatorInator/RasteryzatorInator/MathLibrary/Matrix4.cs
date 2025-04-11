using System.Linq;

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

    public override string ToString()
    {
        return $"[{this[0, 0]:F3} {this[0, 1]:F3} {this[0, 2]:F3} {this[0, 3]:F3}]\n" +
               $"[{this[1, 0]:F3} {this[1, 1]:F3} {this[1, 2]:F3} {this[1, 3]:F3}]\n" +
               $"[{this[2, 0]:F3} {this[2, 1]:F3} {this[2, 2]:F3} {this[2, 3]:F3}]\n" +
               $"[{this[3, 0]:F3} {this[3, 1]:F3} {this[3, 2]:F3} {this[3, 3]:F3}]";
    }
}