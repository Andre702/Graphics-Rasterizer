using RasteryzatorInator.MathLibrary;
using System.Drawing;

namespace RasteryzatorInator
{
    internal class Mesh
    {
        public List<VertexData> Vertices { get; private set; }
        public List<int> Indices { get; private set; } // Grupy po 3 indeksy tworzą trójkąt

        public Mesh(List<VertexData> vertices, List<int> indices)
        {
            Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
            Indices = indices ?? throw new ArgumentNullException(nameof(indices));

            if (Indices.Count % 3 != 0)
            {
                throw new ArgumentException("Liczba indeksów musi być podzielna przez 3.", nameof(indices));
            }
        }

        public static Mesh Cone(int verticalSegments, float height, RawColor basecolor)
        {
            return Cone(verticalSegments, height, basecolor, basecolor, basecolor);
        }

        public static Mesh Cone(int verticalSegments, float height, RawColor color1, RawColor color2, RawColor color3)
        {
            if (verticalSegments < 3)
            {
                throw new ArgumentOutOfRangeException();
            }

            var vertices = new List<VertexData>();
            var indices = new List<int>();

            // 1. Wierzchołek (Apex)
            Vector3 apexPosition = new Vector3(0, height, 0);
            vertices.Add(new VertexData(apexPosition, new RawColor(250, 0, 30)));
            int apexIndex = 0;

            // 2. Środek podstawy (opcjonalny, jeśli chcemy zamknąć podstawę)
            Vector3 baseCenterPosition = Vector3.Zero; // (0,0,0)
            vertices.Add(new VertexData(baseCenterPosition, new RawColor(250, 0, 30)));
            int baseCenterIndex = 1;

            // 3. Wierzchołki na okręgu podstawy
            int firstBaseVertexIndex = vertices.Count;

            RawColor[] rimColor = { new RawColor(255, 255, 255), new RawColor(0, 0, 30) };

            for (int i = 0; i < verticalSegments; i++)
            {
                float angle = i * 2.0f * MathF.PI / verticalSegments;
                float x = MathF.Cos(angle);
                float z = MathF.Sin(angle);
                Vector3 baseVertexPos = new Vector3(x, 0, z);
                vertices.Add(new VertexData(baseVertexPos, rimColor[i % 2]));
            }

            // 4. Tworzenie indeksów (trójkątów)
            for (int i = 0; i < verticalSegments; i++)
            {
                int currentBaseIndex = firstBaseVertexIndex + i;
                // Modulo zapewnia "zawinięcie" do pierwszego wierzchołka podstawy dla ostatniego trójkąta
                int nextBaseIndex = firstBaseVertexIndex + (i + 1) % verticalSegments;

                // Trójkąt podstawy (CCW patrząc od dołu - upewnij się, że twój culling to obsłuży)
                // Jeśli rasterizer usuwa trójkąty skierowane "tyłem" (backface culling),
                // a kamera patrzy od góry, ta kolejność (baseCenter, current, next) będzie OK.
                indices.Add(baseCenterIndex);
                indices.Add(currentBaseIndex);
                indices.Add(nextBaseIndex);


                // Trójkąt boczny (CCW patrząc z zewnątrz)
                indices.Add(apexIndex);
                indices.Add(nextBaseIndex); // Następny wierzchołek podstawy
                indices.Add(currentBaseIndex); // Obecny wierzchołek podstawy
            }

            return new Mesh(vertices, indices);
        }


        public static Mesh Cylinder(int radialSegments, int heightSegments, float height, RawColor baseColor)
        {
            return Cylinder(radialSegments, heightSegments, height, baseColor, baseColor, baseColor);
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
            vertices.Add(new VertexData(Vector3.Zero, color1)); // Indeks 0
            int bottomCenterIndex = 0;

            // Środek górnej podstawy
            vertices.Add(new VertexData(new Vector3(0, height, 0), color1)); // Indeks 1
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
            RawColor[] rimColor = { color2, color3, new RawColor(0, 255, 0)};

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

            // --- Indeksy ---

            // 1. Dolna podstawa (Cap)
            for (int i = 0; i < radialSegments; i++)
            {
                int current = bottomCapStartIndex + i;
                int next = bottomCapStartIndex + (i + 1) % radialSegments;
                indices.Add(bottomCenterIndex);
                indices.Add(current);
                indices.Add(next); 
            }

            // 2. Górna podstawa (Cap)
            for (int i = 0; i < radialSegments; i++)
            {
                int current = topCapStartIndex + i;
                int next = topCapStartIndex + (i + 1) % radialSegments;
                indices.Add(current);
                indices.Add(topCenterIndex);
                indices.Add(next);
            }

            // 3. Boki
            for (int h = 0; h < heightSegments; h++) // Iteruj po "pasach" wysokości
            {
                for (int i = 0; i < radialSegments; i++)
                {
                    // Indeksy wierzchołków w sekcji 'sideVertices'
                    // Indeks wierzchołka = firstSideVertexIndex + nr_pierscienia * radialSegments + nr_segmentu_radialnego
                    int idx(int heightIdx, int radialIdx) =>
                        firstSideVertexIndex + heightIdx * radialSegments + radialIdx % radialSegments;

                    int v0 = idx(h, i);             // Dolny-lewy (bieżący)
                    int v1 = idx(h + 1, i);         // Górny-lewy
                    int v2 = idx(h + 1, i + 1);     // Górny-prawy
                    int v3 = idx(h, i + 1);         // Dolny-prawy

                    // Trójkąt 1 (dolny-lewy, górny-lewy, górny-prawy)
                    indices.Add(v0);
                    indices.Add(v1);
                    indices.Add(v2);

                    // Trójkąt 2 (dolny-lewy, górny-prawy, dolny-prawy)
                    indices.Add(v0);
                    indices.Add(v2);
                    indices.Add(v3);
                }
            }

            return new Mesh(vertices, indices);
        }

        public static Mesh Torus(float tubeRadius, float pipeRadius, int tubeSegments, int pipeSegments, RawColor color)
        {
            // Wywołaj wersję gradientową, podając ten sam kolor jako zewnętrzny i wewnętrzny
            return Torus(tubeRadius, pipeRadius, tubeSegments, pipeSegments, color, color);
        }

        public static Mesh Torus(float tubeRadius, float pipeRadius, int tubeSegments, int pipeSegments,
                                 RawColor colorOuter, RawColor colorInner)
        {
            if (tubeSegments < 3) throw new ArgumentOutOfRangeException();
            if (pipeSegments < 3) throw new ArgumentOutOfRangeException();
            if (tubeRadius <= 0) throw new ArgumentOutOfRangeException();
            if (pipeRadius <= 0) throw new ArgumentOutOfRangeException();

            var vertices = new List<VertexData>();
            var indices = new List<int>();
            RawColor[] rimColor = { colorOuter, colorInner, new RawColor(0, 255, 0) };

            // --- Wierzchołki ---
            // Iterujemy po segmentach tuby (kąt theta) i segmentach rury (kąt phi)
            for (int i = 0; i < tubeSegments; i++) // Segmenty tuby (główny pierścień)
            {
                float theta = i * 2.0f * MathF.PI / tubeSegments;
                float cosTheta = MathF.Cos(theta);
                float sinTheta = MathF.Sin(theta);

                for (int j = 0; j < pipeSegments; j++) // Segmenty rury (przekrój)
                {
                    float phi = j * 2.0f * MathF.PI / pipeSegments;
                    float cosPhi = MathF.Cos(phi);
                    float sinPhi = MathF.Sin(phi);

                    // Pozycja wierzchołka (parametryzacja torusa)
                    float x = (tubeRadius + pipeRadius * cosPhi) * cosTheta;
                    float y = (tubeRadius + pipeRadius * cosPhi) * sinTheta;
                    float z = pipeRadius * sinPhi;

                    // Kolor wierzchołka (interpolacja dla gradientu)
                    // Używamy (1 + cosPhi) / 2 jako czynnika interpolacji t:
                    // t = 1 dla phi=0 (zewnętrzna krawędź)
                    // t = 0 dla phi=pi (wewnętrzna krawędź)
                    float colorFactor = (1.0f + cosPhi) * 0.5f;
                    RawColor vertexColor = rimColor[i%2 + j%2];

                    vertices.Add(new VertexData(new Vector3(x, y, z), vertexColor));
                }
            }

            // --- Indeksy ---
            // Tworzymy trójkąty łączące wierzchołki w siatkę
            for (int i = 0; i < tubeSegments; i++) // Indeks segmentu tuby
            {
                for (int j = 0; j < pipeSegments; j++) // Indeks segmentu rury
                {
                    // Obliczanie indeksów czterech wierzchołków tworzących quad
                    // Używamy modulo (%) aby zapewnić "zawijanie" na końcach
                    int next_i = (i + 1) % tubeSegments;
                    int next_j = (j + 1) % pipeSegments;

                    // Indeks wierzchołka = nr_segmentu_tuby * liczba_segmentów_rury + nr_segmentu_rury
                    int idx00 = i * pipeSegments + j;          // obecny i, obecny j
                    int idx10 = next_i * pipeSegments + j;     // następny i, obecny j
                    int idx01 = i * pipeSegments + next_j;     // obecny i, następny j
                    int idx11 = next_i * pipeSegments + next_j;// następny i, następny j

                    // Tworzymy dwa trójkąty z quada (CCW patrząc z zewnątrz)
                    // Trójkąt 1: (i,j), (i+1,j), (i+1,j+1)
                    indices.Add(idx00);
                    indices.Add(idx10);
                    indices.Add(idx11);

                    // Trójkąt 2: (i,j), (i+1,j+1), (i,j+1)
                    indices.Add(idx00);
                    indices.Add(idx11);
                    indices.Add(idx01);
                }
            }

            return new Mesh(vertices, indices);
        }

    }
}