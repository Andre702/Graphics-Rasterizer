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

        public static Matrix4 LookAt(Vector3 eye, Vector3 center, Vector3 up)
        {
            // Oblicz osie układu współrzędnych kamery
            Vector3 zaxis = (eye - center).Normalized();  // Oś Z kamery (patrzy na obserwatora, +Z wychodzi z ekranu)
            Vector3 xaxis = Vector3.Cross(up, zaxis).Normalized(); // Oś X kamery (w prawo)
            Vector3 yaxis = Vector3.Cross(zaxis, xaxis); // Oś Y kamery (w górę - nie trzeba normalizować)

            Matrix4 view = new Matrix4(
                new Vector4(xaxis.X, xaxis.Y, xaxis.Z, 0),          // Kolumna 0
                new Vector4(yaxis.X, yaxis.Y, yaxis.Z, 0),          // Kolumna 1
                new Vector4(zaxis.X, zaxis.Y, zaxis.Z, 0),          // Kolumna 2
                new Vector4(0, 0, 0, 1)                             // Kolumna 3 (translacja będzie dodana)
            );
            // view jest teraz R^T. Potrzebujemy pomnożyć przez T(-eye)
            Matrix4 translation = Matrix4.Identity();
            translation[3] = new Vector4(-eye.X, -eye.Y, -eye.Z, 1); // Kolumna translacji

            return view * translation;

            // Alternatywnie, wypełnijmy macierz od razu:
            // return new Matrix4(
            //     new Vector4(xaxis.X, yaxis.X, zaxis.X, 0), // Kolumna 0 - BŁĄD! To są komponenty X osi
            //     new Vector4(xaxis.Y, yaxis.Y, zaxis.Y, 0), // Kolumna 1 - BŁĄD! To są komponenty Y osi
            //     new Vector4(xaxis.Z, yaxis.Z, zaxis.Z, 0), // Kolumna 2 - BŁĄD! To są komponenty Z osi
            //     new Vector4(-Vector3.Dot(xaxis, eye), -Vector3.Dot(yaxis, eye), -Vector3.Dot(zaxis, eye), 1) // Kolumna 3 - Poprawna translacja
            // );
            // Ta powyższa jest Transponowana, jeśli ma działać z M*v.

            // OSTATECZNA POPRAWNA WERSJA dla Column-Major i M*v:
            // return new Matrix4(
            //     new Vector4(xaxis.X, yaxis.X, zaxis.X, -Vector3.Dot(xaxis, eye)), // Kolumna 0
            //     new Vector4(xaxis.Y, yaxis.Y, zaxis.Y, -Vector3.Dot(yaxis, eye)), // Kolumna 1
            //     new Vector4(xaxis.Z, yaxis.Z, zaxis.Z, -Vector3.Dot(zaxis, eye)), // Kolumna 2
            //     new Vector4(0,       0,       0,        1)                        // Kolumna 3
            // ); // Ta forma nie jest standardowa. Standardowa to ta wyliczona R^T * T(-eye).

            // Użyjemy R^T * T(-eye)
            Matrix4 rotT = new Matrix4(
                new Vector4(xaxis.X, xaxis.Y, xaxis.Z, 0),
                new Vector4(yaxis.X, yaxis.Y, yaxis.Z, 0),
                new Vector4(zaxis.X, zaxis.Y, zaxis.Z, 0),
                new Vector4(0, 0, 0, 1)
            );
            Matrix4 transInv = Matrix4.Identity();
            transInv[3] = new Vector4(-eye.X, -eye.Y, -eye.Z, 1);

            return rotT * transInv;

        }


        // --- Poprawiona Macierz Perspective ---
        public static Matrix4 CreatePerspective(float fovyDegrees, float aspectRatio, float nearPlane, float farPlane)
        {
            if (nearPlane <= 0) throw new ArgumentOutOfRangeException();
            if (farPlane <= nearPlane) throw new ArgumentOutOfRangeException();
            if (aspectRatio <= 0) throw new ArgumentOutOfRangeException();
            if (fovyDegrees <= 0 || fovyDegrees >= 180) throw new ArgumentOutOfRangeException();

            float fovyRadians = fovyDegrees * (MathF.PI / 180.0f);
            float f = 1.0f / MathF.Tan(fovyRadians / 2.0f); // cot(fovy/2)
            float rangeInv = 1.0f / (nearPlane - farPlane); // 1 / (n - f)

            // Standard OpenGL Perspective Matrix (Column-Major)
            return new Matrix4(
                new Vector4(f / aspectRatio, 0, 0, 0),                 // Kolumna 0
                new Vector4(0, f, 0, 0),                                // Kolumna 1
                new Vector4(0, 0, (farPlane + nearPlane) * rangeInv, -1.0f), // Kolumna 2 (Element [3,2] to -1) - POPRAWIONO
                new Vector4(0, 0, (2.0f * farPlane * nearPlane) * rangeInv, 0)  // Kolumna 3 (Element [2,3] to 2fn/(n-f))
            );
        }
    }
}