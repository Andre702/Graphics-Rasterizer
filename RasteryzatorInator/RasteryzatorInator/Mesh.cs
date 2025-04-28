using RasteryzatorInator.MathLibrary;
using System.Diagnostics;
using System.Drawing;

namespace RasteryzatorInator
{
    internal class Mesh
    {
        public List<VertexData> Vertices { get; private set; }
        public List<int> Indices { get; private set; }

        public Mesh(List<VertexData> vertices, List<int> indices)
        {
            if (vertices == null) throw new ArgumentNullException(nameof(vertices));
            if (indices == null) throw new ArgumentNullException(nameof(indices));
            if (indices.Count % 3 != 0) throw new ArgumentException();

            Vertices = new List<VertexData>(vertices);
            Indices = new List<int>(indices);

            CalculateNormals();
        }

        private Mesh(List<VertexData> vertices, List<int> indices, bool skipNormalCalculation)
        {
            Vertices = vertices;
            Indices = indices;
        }

        public void CalculateNormals()
        {

            Vector3[] normalAccumulator = new Vector3[Vertices.Count];

            for (int i = 0; i < Indices.Count; i += 3)
            {
                int index1 = Indices[i];
                int index2 = Indices[i + 1];
                int index3 = Indices[i + 2];

                Vector3 p1 = Vertices[index1].Position;
                Vector3 p2 = Vertices[index2].Position;
                Vector3 p3 = Vertices[index3].Position;

                Vector3 edge1 = p2 - p1;
                Vector3 edge2 = p3 - p1;

                Vector3 faceNormal = Vector3.Cross(edge1, edge2);

                if (faceNormal.LengthSquared() < 0.000001f)
                {
                    continue;
                }

                normalAccumulator[index1] += faceNormal;
                normalAccumulator[index2] += faceNormal;
                normalAccumulator[index3] += faceNormal;
            }

            List<VertexData> updatedVertices = new List<VertexData>(Vertices.Count);
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vector3 accumulatedNormal = normalAccumulator[i];
                Vector3 finalNormal;

                if (accumulatedNormal.LengthSquared() < 0.000001f)
                {
                    finalNormal = Vector3.UnitY;
                    Console.WriteLine($"Wierzchołek {i} ma zerową normalną po akumulacji.");
                }
                else
                {
                    finalNormal = accumulatedNormal.Normalized();
                }

                updatedVertices.Add(new VertexData(Vertices[i].Position, finalNormal, Vertices[i].Color));
            }

            Vertices = updatedVertices;
        }

        public static Mesh Cone(int verticalSegments, float height, RawColor color1, RawColor color2, RawColor color3)
        {
            if (verticalSegments < 3)
                throw new ArgumentOutOfRangeException();

            var vertices = new List<VertexData>();
            var indices = new List<int>();

            // Wierzchołek
            Vector3 apexPosition = new Vector3(0, height, 0);
            vertices.Add(new VertexData(apexPosition, color1));
            int apexIndex = 0;

            // Środek podstawy
            Vector3 baseCenterPosition = Vector3.Zero;
            vertices.Add(new VertexData(baseCenterPosition, color1));
            int baseCenterStartIndex = 1;

            // Podstawa
            int baseRimStartIndex = vertices.Count;

            RawColor[] rimColor = { color2, color3 };

            for (int i = 0; i < verticalSegments; i++)
            {
                float angle = i * 2.0f * MathF.PI / verticalSegments;
                float x = MathF.Cos(angle);
                float z = MathF.Sin(angle);
                Vector3 baseVertexPos = new Vector3(x, 0, z);
                vertices.Add(new VertexData(baseVertexPos, rimColor[i % 2]));
            }

            // Trójkąty:
            for (int i = 0; i < verticalSegments; i++)
            {
                int currentBaseIndex = baseRimStartIndex + i;
                int nextBaseIndex = baseRimStartIndex + (i + 1) % verticalSegments;

                // podstawa
                indices.Add(baseCenterStartIndex);
                indices.Add(currentBaseIndex);
                indices.Add(nextBaseIndex);

                // bok
                indices.Add(apexIndex);
                indices.Add(nextBaseIndex);
                indices.Add(currentBaseIndex);
            }

            return new Mesh(vertices, indices);
        }

        public static Mesh Cylinder2(int radialSegments, int heightSegments, float height, RawColor color1, RawColor color2, RawColor color3)
        {
            if (radialSegments < 3)
                throw new ArgumentOutOfRangeException();
            if (heightSegments < 1)
                throw new ArgumentOutOfRangeException();

            var vertices = new List<VertexData>();
            var indices = new List<int>();

            // Środek dolnej podstawy
            vertices.Add(new VertexData(Vector3.Zero, color1));
            int bottomCenterIndex = 0;

            // Środek górnej podstawy
            vertices.Add(new VertexData(new Vector3(0, height, 0), color1));
            int topCenterIndex = 1;


            int bottomCapStartIndex = vertices.Count;
            // Podstawa dolna
            for (int i = 0; i < radialSegments; i++)
            {
                float angle = i * 2.0f * MathF.PI / radialSegments;
                float x = MathF.Cos(angle);
                float z = MathF.Sin(angle);
                vertices.Add(new VertexData(new Vector3(x, 0, z), color2));
            }

            int topCapStartIndex = vertices.Count;
            // Podstawa górna
            for (int i = 0; i < radialSegments; i++)
            {
                float angle = i * 2.0f * MathF.PI / radialSegments;
                float x = MathF.Cos(angle);
                float z = MathF.Sin(angle);
                vertices.Add(new VertexData(new Vector3(x, height, z), color2));
            }

            int firstSideVertexIndex = vertices.Count;
            RawColor[] rimColor = { color1, color2, color3};

            // Boki
            for (int h = 0; h <= heightSegments; h++)
            {
                float y = (float)h / heightSegments * height;
                for (int i = 0; i < radialSegments; i++)
                {
                    float angle = i * 2.0f * MathF.PI / radialSegments;
                    float x = MathF.Cos(angle);
                    float z = MathF.Sin(angle);
                    vertices.Add(new VertexData(new Vector3(x, y, z), rimColor[i%2 + h%2]));
                }
            }

            // Trójkąty:
            for (int i = 0; i < radialSegments; i++)
            {
                // dolna podstawa
                int current = bottomCapStartIndex + i;
                int next = bottomCapStartIndex + (i + 1) % radialSegments;
                indices.Add(bottomCenterIndex);
                indices.Add(current);
                indices.Add(next); 
            }

            for (int i = 0; i < radialSegments; i++)
            {
                // górna podstawa
                int current = topCapStartIndex + i;
                int next = topCapStartIndex + (i + 1) % radialSegments;
                indices.Add(current);
                indices.Add(topCenterIndex);
                indices.Add(next);
            }

            for (int h = 0; h < heightSegments; h++)
            {
                for (int i = 0; i < radialSegments; i++)
                {
                    // boki
                    int idx(int heightIdx, int radialIdx) =>
                        firstSideVertexIndex + heightIdx * radialSegments + radialIdx % radialSegments;

                    int v0 = idx(h, i);
                    int v1 = idx(h + 1, i);
                    int v2 = idx(h + 1, i + 1);
                    int v3 = idx(h, i + 1);

                    indices.Add(v0);
                    indices.Add(v1);
                    indices.Add(v2);

                    indices.Add(v0);
                    indices.Add(v2);
                    indices.Add(v3);
                }
            }

            return new Mesh(vertices, indices);
        }

        public static Mesh Cylinder(int radialSegments, int heightSegments, float height, RawColor color1, RawColor color2, RawColor color3)
        {
            if (radialSegments < 3)
                throw new ArgumentOutOfRangeException();
            if (heightSegments < 1)
                throw new ArgumentOutOfRangeException();

            var vertices = new List<VertexData>();
            var indices = new List<int>();

            // Środek dolnej podstawy
            vertices.Add(new VertexData(Vector3.Zero, color1));
            int bottomCenterIndex = 0;

            // Środek górnej podstawy
            vertices.Add(new VertexData(new Vector3(0, height, 0), color1));
            int topCenterIndex = 1;

            // Wierzchołki boków (w tym dolna i górna obręcz)
            int sideVertexStartIndex = vertices.Count;
            RawColor[] rimColors = { color1, color2, color3 };

            for (int h = 0; h <= heightSegments; h++)
            {
                float y = (float)h / heightSegments * height;
                bool isCapRing = (h == 0 || h == heightSegments); // Czy to pierścień należący też do podstawy?

                for (int i = 0; i < radialSegments; i++)
                {
                    float angle = i * 2.0f * MathF.PI / radialSegments;
                    float x = MathF.Cos(angle);
                    float z = MathF.Sin(angle);
                    Vector3 position = new Vector3(x, y, z);

                    RawColor vertexColor = isCapRing ? color2 : rimColors[(i + h) % rimColors.Length];

                    vertices.Add(new VertexData(position, vertexColor));
                }
            }

            // Trójkąty

            int idx(int heightIdx, int radialIdx)
            {
                return sideVertexStartIndex + heightIdx * radialSegments + (radialIdx % radialSegments);
            }


            // Dolna podstawa
            for (int i = 0; i < radialSegments; i++)
            {
                // dolny pierścień (h=0)
                int current = idx(0, i);
                int next = idx(0, i + 1);

                indices.Add(bottomCenterIndex);
                indices.Add(current);
                indices.Add(next);
            }

            // Górna podstawa
            for (int i = 0; i < radialSegments; i++)
            {
                // górny pierścień (h=heightSegments)
                int current = idx(heightSegments, i);
                int next = idx(heightSegments, i + 1);

                indices.Add(current);
                indices.Add(topCenterIndex);
                indices.Add(next);
            }

            // Boki
            for (int h = 0; h < heightSegments; h++)
            {
                for (int i = 0; i < radialSegments; i++)
                {
                    int v00 = idx(h, i);
                    int v10 = idx(h + 1, i);
                    int v11 = idx(h + 1, i + 1);
                    int v01 = idx(h, i + 1);

                    indices.Add(v00);
                    indices.Add(v10);
                    indices.Add(v11);

                    indices.Add(v00);
                    indices.Add(v11);
                    indices.Add(v01);
                }
            }

            return new Mesh(vertices, indices);
        }

        public static Mesh Torus(float R, float r, int outerSegments, int innerSegments,
                                 RawColor color1, RawColor color2, RawColor color3)
        {
            if (outerSegments < 3) throw new ArgumentOutOfRangeException();
            if (innerSegments < 3) throw new ArgumentOutOfRangeException();
            if (R <= 0) throw new ArgumentOutOfRangeException();
            if (r <= 0) throw new ArgumentOutOfRangeException();

            var vertices = new List<VertexData>();
            var indices = new List<int>();
            RawColor[] rimColor = { color1, color2, color3 };

            for (int i = 0; i < outerSegments; i++) // tuba (góra / dół)
            {
                float theta = i * 2.0f * MathF.PI / outerSegments;
                float cosTheta = MathF.Cos(theta);
                float sinTheta = MathF.Sin(theta);

                for (int j = 0; j < innerSegments; j++) // przekrój
                {
                    float phi = j * 2.0f * MathF.PI / innerSegments;
                    float cosPhi = MathF.Cos(phi);
                    float sinPhi = MathF.Sin(phi);

                    float x = (R + r * cosPhi) * cosTheta;
                    float y = (R + r * cosPhi) * sinTheta;
                    float z = r * sinPhi;

                    float colorFactor = (1.0f + cosPhi) * 0.5f;
                    RawColor vertexColor = rimColor[i%2 + j%2];

                    vertices.Add(new VertexData(new Vector3(x, y, z), vertexColor));
                }
            }

            // Trójkąty:
            for (int i = 0; i < outerSegments; i++)
            {
                for (int j = 0; j < innerSegments; j++)
                {
                    int next_i = (i + 1) % outerSegments;
                    int next_j = (j + 1) % innerSegments;

                    int idx00 = i * innerSegments + j;
                    int idx10 = next_i * innerSegments + j;
                    int idx01 = i * innerSegments + next_j;
                    int idx11 = next_i * innerSegments + next_j;

                    indices.Add(idx00);
                    indices.Add(idx10);
                    indices.Add(idx11);

                    indices.Add(idx00);
                    indices.Add(idx11);
                    indices.Add(idx01);
                }
            }

            return new Mesh(vertices, indices);
        }

    }
}