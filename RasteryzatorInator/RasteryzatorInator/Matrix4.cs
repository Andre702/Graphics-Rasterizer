using Microsoft.VisualBasic;
using System;
using System.Runtime.CompilerServices; // Dla AggressiveInlining

namespace RasteryzatorInator
{
    internal class Matrix4
    {
        private readonly Vector4[] columns = new Vector4[4];

        public Vector4 this[int colIndex]
        {
            get
            {
                if (colIndex < 0 || colIndex > 3) throw new IndexOutOfRangeException();
                return columns[colIndex];
            }
            set
            {
                if (colIndex < 0 || colIndex > 3) throw new IndexOutOfRangeException();
                columns[colIndex] = value;
            }
        }

        public float this[int rowIndex, int colIndex]
        {
            get
            {
                return this[colIndex][rowIndex];
            }
            set
            {
                this[colIndex][rowIndex] = value;
            }
        }

        public Matrix4() : this(
            new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
            new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            new Vector4(0.0f, 0.0f, 1.0f, 0.0f),
            new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
        )
        { }


        public Matrix4(Vector4 col0, Vector4 col1, Vector4 col2, Vector4 col3)
        {
            columns[0] = col0;
            columns[1] = col1;
            columns[2] = col2;
            columns[3] = col3;
        }

        public static Matrix4 Identity()
        {
            return new Matrix4();
        }

        public static Matrix4 operator *(Matrix4 m1, Matrix4 m2)
        {
            Matrix4 result = new Matrix4();
            for (int j = 0; j < 4; ++j) // Iteruj przez kolumny wyniku (i kolumny m2)
            {
                // Kolumna j wyniku jest liniową kombinacją kolumn m1, ważoną przez kolumnę j z m2
                result[j] = m1[0] * m2[0, j] +  // Col0(m1) * m2[0,j] (element z 1. wiersza, j-tej kolumny m2)
                            m1[1] * m2[1, j] +  // Col1(m1) * m2[1,j]
                            m1[2] * m2[2, j] +  // Col2(m1) * m2[2,j]
                            m1[3] * m2[3, j];   // Col3(m1) * m2[3,j]
            }
            return result;
        }

        public static Matrix4 Translate(Matrix4 m, Vector3 v)
        {
            Matrix4 translate = new Matrix4(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(v.X, v.Y, v.Z, 1));

            return m * translate;
        }

        public static Matrix4 Scale(Matrix4 m, Vector3 v)
        {
            return new Matrix4(
                m[0] * v.X,
                m[1] * v.Y,
                m[2] * v.Z,
                m[3]
            );
        }

        public static Matrix4 Rotate(Matrix4 m, Vector3 axis, float angleRadians)
        {
            try { axis.Normalize(); } 
            catch (DivideByZeroException) { return Identity(); }

            float x = axis.X, y = axis.Y, z = axis.Z;
            float c = MathF.Cos(angleRadians);
            float s = MathF.Sin(angleRadians);
            float t = 1.0f - c;

            Matrix4 rotation = new Matrix4(
                new Vector4(x * x * t + c, x * y * t - s * z, t * x * z + s * y, 0.0f),
                new Vector4(t * x * y + s * z, t * y * y + c, t * y * z - s * x, 0.0f),
                new Vector4(t * x * z - s * y, t * y * z + s * x, t * z * z + c, 0.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
            );

            return m * rotation;
        }

        public static Matrix4 LookAt(Vector3 origin, Vector3 focusAt) => LookAt(origin, focusAt, Vector3.DefaultUp);

        public static Matrix4 LookAt(Vector3 origin, Vector3 focusAt, Vector3 up)
        {
            Vector3 forward = (origin - focusAt).Normalized();

            Vector3 side = Vector3.Cross(forward, up).Normalized();

            Vector3 upward = Vector3.Cross(side, forward);

            Matrix4 result = new Matrix4(
                new Vector4(side.X, upward.X, -forward.X, 0), // Kolumna 0 (oś s)
                new Vector4(side.Y, upward.Y, -forward.Y, 0), // Kolumna 1 (oś u)
                new Vector4(side.Z, upward.Z, -forward.Z, 0), // Kolumna 2 (oś -f)
                new Vector4(0, 0, 0, 1) // Kolumna 3 (na razie jednostkowa translacja)
            );

            Matrix4 inverseOrigin = new Matrix4();
            inverseOrigin[3] = new Vector4(-origin.X, -origin.Y, -origin.Z, 1);

            return result * inverseOrigin;
        }

        public static Matrix4 CreatePerspective(float fovyDegrees, float aspectRatio, float nearPlane, float farPlane)
        {
            if (nearPlane <= 0) throw new ArgumentOutOfRangeException();
            if (farPlane <= nearPlane) throw new ArgumentOutOfRangeException();
            if (aspectRatio <= 0) throw new ArgumentOutOfRangeException();
            if (fovyDegrees <= 0 || fovyDegrees >= 180) throw new ArgumentOutOfRangeException();

            float fovyRadians = fovyDegrees * (MathF.PI / 180.0f);
            float f = MathF.Cos(fovyRadians) / MathF.Sin(fovyRadians);
            float rangeInv = 1.0f / (nearPlane - farPlane);

            return new Matrix4(
                new Vector4(f / aspectRatio, 0, 0, 0),
                new Vector4(0, f, 0, 0),
                new Vector4(0, 0, (farPlane + nearPlane) * rangeInv, -1),
                new Vector4(0, 0, (2 * farPlane * nearPlane) * rangeInv, 0)
            );
        }
    }
}