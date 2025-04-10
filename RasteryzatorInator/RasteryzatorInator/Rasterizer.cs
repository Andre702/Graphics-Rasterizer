namespace RasteryzatorInator;

internal class Rasterizer
{
    private Buffer buffer;
    private VertexProcessor vertexProcessor;

    public Rasterizer(Buffer _buffer, VertexProcessor _vertexProcessor)
    {
        buffer = _buffer;
        vertexProcessor = _vertexProcessor;
    }

    private struct ProjectedVertex
    {
        public Vector3 ScreenPos; // Screen X, Screen Y, Mapped NDC Z [0, 1]
        public RawColor Color;
        public float InvW; // 1 / clip.W
    }

    public void ProcessTriangle(VertexData v0, VertexData v1, VertexData v2)
    {
        Vector4 clip0 = vertexProcessor.TransformPositionToClipSpace(v0.Position);
        Vector4 clip1 = vertexProcessor.TransformPositionToClipSpace(v1.Position);
        Vector4 clip2 = vertexProcessor.TransformPositionToClipSpace(v2.Position);

        float clipEpsilon = 0.0001f;
        if (clip0.W <= clipEpsilon && clip1.W <= clipEpsilon && clip2.W <= clipEpsilon)
        {
            return; // Cały trójkąt za lub na płaszczyźnie obserwatora
        }

        float invW0 = (Math.Abs(clip0.W) > clipEpsilon) ? 1.0f / clip0.W : 0;
        float invW1 = (Math.Abs(clip1.W) > clipEpsilon) ? 1.0f / clip1.W : 0;
        float invW2 = (Math.Abs(clip2.W) > clipEpsilon) ? 1.0f / clip2.W : 0;

        Vector3 ndc0 = new Vector3(clip0.X * invW0, clip0.Y * invW0, clip0.Z * invW0);
        Vector3 ndc1 = new Vector3(clip1.X * invW1, clip1.Y * invW1, clip1.Z * invW1);
        Vector3 ndc2 = new Vector3(clip2.X * invW2, clip2.Y * invW2, clip2.Z * invW2);

        bool ndc0_ok = ndc0.X >= -1 && ndc0.X <= 1 && ndc0.Y >= -1 && ndc0.Y <= 1 && ndc0.Z >= -1 && ndc0.Z <= 1;
        bool ndc1_ok = ndc1.X >= -1 && ndc1.X <= 1 && ndc1.Y >= -1 && ndc1.Y <= 1 && ndc1.Z >= -1 && ndc1.Z <= 1;
        bool ndc2_ok = ndc2.X >= -1 && ndc2.X <= 1 && ndc2.Y >= -1 && ndc2.Y <= 1 && ndc2.Z >= -1 && ndc2.Z <= 1;

        ProjectedVertex pv0 = MapToScreen(ndc0, invW0, v0.Color);
        ProjectedVertex pv1 = MapToScreen(ndc1, invW1, v1.Color);
        ProjectedVertex pv2 = MapToScreen(ndc2, invW2, v2.Color);

        bool scr0_ok = pv0.ScreenPos.X >= 0 && pv0.ScreenPos.X < buffer.Width && pv0.ScreenPos.Y >= 0 && pv0.ScreenPos.Y < buffer.Height;
        bool scr1_ok = pv1.ScreenPos.X >= 0 && pv1.ScreenPos.X < buffer.Width && pv1.ScreenPos.Y >= 0 && pv1.ScreenPos.Y < buffer.Height;
        bool scr2_ok = pv2.ScreenPos.X >= 0 && pv2.ScreenPos.X < buffer.Width && pv2.ScreenPos.Y >= 0 && pv2.ScreenPos.Y < buffer.Height;


        float crossProductScreen = (pv1.ScreenPos.X - pv0.ScreenPos.X) * (pv2.ScreenPos.Y - pv0.ScreenPos.Y) -
                                   (pv1.ScreenPos.Y - pv0.ScreenPos.Y) * (pv2.ScreenPos.X - pv0.ScreenPos.X);

        //if (crossProductScreen < 0)
        //{
        //    return;
        //}

        Triangle(pv0, pv1, pv2);
    }

    private ProjectedVertex MapToScreen(Vector3 ndc, float invW, RawColor color)
    {
        int width = buffer.Width;
        int height = buffer.Height;

        // Normalizowane współrzędne urządzenia (NDC) [-1, 1] -> Współrzędne okna [0, width], [0, height]
        float screenX = (ndc.X + 1.0f) * 0.5f * width;
        // Odwrócenie osi Y: NDC Y rośnie w górę, ekranowe Y rośnie w dół
        float screenY = (1.0f - ndc.Y) * 0.5f * height;

        // Mapowanie NDC Z [-1, 1] na zakres głębokości [0, 1]
        // NDC Z = -1 odpowiada near plane, NDC Z = 1 odpowiada far plane
        float depthZ = (ndc.Z + 1.0f) * 0.5f;

        return new ProjectedVertex
        {
            ScreenPos = new Vector3(screenX, screenY, depthZ), // Z przechowuje zmapowaną głębokość
            Color = color,
            InvW = invW
        };
    }

    private void Triangle(ProjectedVertex p1, ProjectedVertex p2, ProjectedVertex p3, RawColor? defaultColor = null)
    {
        int width = buffer.Width;
        int height = buffer.Height;

        float x1 = p1.ScreenPos.X, y1 = p1.ScreenPos.Y, z1 = p1.ScreenPos.Z;
        float x2 = p2.ScreenPos.X, y2 = p2.ScreenPos.Y, z2 = p2.ScreenPos.Z;
        float x3 = p3.ScreenPos.X, y3 = p3.ScreenPos.Y, z3 = p3.ScreenPos.Z;

        float dx12 = x1 - x2;
        float dx23 = x2 - x3;
        float dx31 = x3 - x1;

        float dy12 = y1 - y2;
        float dy23 = y2 - y3;
        float dy31 = y3 - y1;

        bool topLeftEdge1 = dy12 < 0 || (dy12 == 0 && dx12 < 0);
        bool topLeftEdge2 = dy23 < 0 || (dy23 == 0 && dx23 < 0);
        bool topLeftEdge3 = dy31 < 0 || (dy31 == 0 && dx31 < 0);

        float lambdaDenominator = dy23 * -dx31 + dx23 * dy31;

        // Triangle boundaries - screen restriction
        int minX = (int)Math.Max(0, Math.Min(x1, Math.Min(x2, x3)));
        int minY = (int)Math.Max(0, Math.Min(y1, Math.Min(y2, y3)));
        int maxX = (int)Math.Min(width - 1, Math.Max(x1, Math.Max(x2, x3)));
        int maxY = (int)Math.Min(height - 1, Math.Max(y1, Math.Max(y2, y3)));

        bool fullColorMode = false;
        if (defaultColor != null) fullColorMode = true;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                // Inside triangle equation
                if (dx12 * (y - y1) - dy12 * (x - x1) < (topLeftEdge1 ? 0 : 1)) continue;
                if (dx23 * (y - y2) - dy23 * (x - x2) < (topLeftEdge2 ? 0 : 1)) continue;
                if (dx31 * (y - y3) - dy31 * (x - x3) < (topLeftEdge3 ? 0 : 1)) continue;

                if (fullColorMode)
                {
                    buffer.ColorBuffer[y * width + x] = defaultColor.Value;
                    continue;
                }

                float lambda1 = ((y2 - y3) * (x - x3) + (x3 - x2) * (y - y3)) / lambdaDenominator;
                float lambda2 = ((y3 - y1) * (x - x3) + (x1 - x3) * (y - y3)) / lambdaDenominator;
                float lambda3 = 1 - lambda1 - lambda2;

                //float depth = (lambda1 * p1.cZ + lambda2 * p2.cZ + lambda3 * p3.cZ);

                //float interpolatedInvW = lambda1 * p1.InvW + lambda2 * p2.InvW + lambda3 * p3.InvW;
                //if (Math.Abs(interpolatedInvW) < float.Epsilon) continue;
                //float w = 1.0f / interpolatedInvW;

                //float depthCorrect = (lambda1 * z1 * p1.InvW +
                //                      lambda2 * z2 * p2.InvW +
                //                      lambda3 * z3 * p3.InvW) * w;

                //if (depth < buffer.DepthBuffer[y * width + x])
                //{
                    
                //}

                //buffer.DepthBuffer[y * width + x] = depth;
                byte r = (byte)(lambda1 * p1.Color.R + lambda2 * p2.Color.R + lambda3 * p3.Color.R);
                byte g = (byte)(lambda1 * p1.Color.G + lambda2 * p2.Color.G + lambda3 * p3.Color.G);
                byte b = (byte)(lambda1 * p1.Color.B + lambda2 * p2.Color.B + lambda3 * p3.Color.B);
                buffer.ColorBuffer[y * width + x] = new RawColor(r, g, b);
            }
        }
    }

    private void RasterizeScreenTriangle(ProjectedVertex p1, ProjectedVertex p2, ProjectedVertex p3)
    {
        int width = buffer.Width;
        int height = buffer.Height;

        float x1 = p1.ScreenPos.X, y1 = p1.ScreenPos.Y;
        float x2 = p2.ScreenPos.X, y2 = p2.ScreenPos.Y;
        float x3 = p3.ScreenPos.X, y3 = p3.ScreenPos.Y;

        Console.WriteLine("\n--- Rasterizer.RasterizeScreenTriangle ---");
        Console.WriteLine($"Screen Vertices: P1({x1:F1},{y1:F1}) P2({x2:F1},{y2:F1}) P3({x3:F1},{y3:F1})");

        int minX = Math.Max(0, (int)MathF.Floor(Math.Min(x1, Math.Min(x2, x3))));
        int minY = Math.Max(0, (int)MathF.Floor(Math.Min(y1, Math.Min(y2, y3))));
        int maxX = Math.Min(width - 1, (int)MathF.Ceiling(Math.Max(x1, Math.Max(x2, x3))));
        int maxY = Math.Min(height - 1, (int)MathF.Ceiling(Math.Max(y1, Math.Max(y2, y3))));

        Console.WriteLine($"Bounding Box: minX={minX}, maxX={maxX}, minY={minY}, maxY={maxY}");

        if (minX > maxX || minY > maxY)
        {
            Console.WriteLine("!!! Rasterization skipped: Empty Bounding Box !!!");
            return;
        }

        // --- POPRAWIONA DEFINICJA WEKTORÓW KRAWĘDZI (V_end - V_start) ---
        float dx12 = x2 - x1; float dy12 = y2 - y1; // Wektor P1 -> P2
        float dx23 = x3 - x2; float dy23 = y3 - y2; // Wektor P2 -> P3
        float dx31 = x1 - x3; float dy31 = y1 - y3; // Wektor P3 -> P1

        float startPx = minX + 0.5f;
        float startPy = minY + 0.5f;

        // --- Oblicz wartości funkcji krawędzi (Standardowa definicja) ---
        // E(x, y) = (x - Vx_start) * dy - (y - Vy_start) * dx
        float edge1Start = (startPx - x1) * dy12 - (startPy - y1) * dx12; // Krawędź 1-2
        float edge2Start = (startPx - x2) * dy23 - (startPy - y2) * dx23; // Krawędź 2-3
        float edge3Start = (startPx - x3) * dy31 - (startPy - y3) * dx31; // Krawędź 3-1

        // Mianownik (już poprawiony wcześniej)
        float baryDenominator = (x2 - x1) * (y3 - y1) - (y2 - y1) * (x3 - x1);
        // float baryDenominator = dx12 * (y3 - y1) - dy12 * (x3 - x1); // Równoważnie
        Console.WriteLine($"Barycentric Denominator: {baryDenominator:F3}");

        if (Math.Abs(baryDenominator) < 1e-6f)
        {
            Console.WriteLine("!!! Rasterization skipped: Degenerate Triangle !!!");
            return;
        }
        float invBaryDenominator = 1.0f / baryDenominator;

        bool pixelDrawn = false;
        float pixelEpsilon = -1e-5f; // Tolerancja dla krawędzi

        for (int y = minY; y <= maxY; y++)
        {
            float edge1Row = edge1Start;
            float edge2Row = edge2Start;
            float edge3Row = edge3Start;

            for (int x = minX; x <= maxX; x++)
            {
                // Test funkcji krawędziowych (powinien być >= 0 dla CCW)
                if (edge1Row >= pixelEpsilon && edge2Row >= pixelEpsilon && edge3Row >= pixelEpsilon)
                {
                    // Oblicz lambdy (bez zmian)
                    float lambda3 = edge1Row * invBaryDenominator; // Waga p3 (przeciw krawędzi 1-2)
                    float lambda1 = edge2Row * invBaryDenominator; // Waga p1 (przeciw krawędzi 2-3)
                    float lambda2 = edge3Row * invBaryDenominator; // Waga p2 (przeciw krawędzi 3-1)
                                                                   // Można dodać sprawdzenie: if (lambda1 < 0 || lambda1 > 1 || lambda2 < 0 || lambda2 > 1 || lambda3 < 0 || lambda3 > 1) continue;

                    float interpolatedInvW = lambda1 * p1.InvW + lambda2 * p2.InvW + lambda3 * p3.InvW;
                    if (Math.Abs(interpolatedInvW) < float.Epsilon) continue;
                    float w = 1.0f / interpolatedInvW;

                    float depthCorrect = (lambda1 * p1.ScreenPos.Z * p1.InvW +
                                          lambda2 * p2.ScreenPos.Z * p2.InvW +
                                          lambda3 * p3.ScreenPos.Z * p3.InvW) * w;

                    int pixelIndex = y * width + x;

                    // Test głębokości (powinien teraz przechodzić, bo buffer ma 100.0f)
                    if (depthCorrect >= 0 && depthCorrect < buffer.DepthBuffer[pixelIndex])
                    {
                        if (!pixelDrawn)
                        {
                            Console.WriteLine($"  DRAWING Pixel ({x},{y})! Depth: {depthCorrect:F5} < {buffer.DepthBuffer[pixelIndex]:F5}. Lambdas: l1={lambda1:F2}, l2={lambda2:F2}, l3={lambda3:F2}");
                            pixelDrawn = true;
                        }
                        buffer.DepthBuffer[pixelIndex] = depthCorrect;
                        // ... interpolacja i zapis koloru ...
                        float r_correct = (lambda1 * p1.Color.R * p1.InvW + lambda2 * p2.Color.R * p2.InvW + lambda3 * p3.Color.R * p3.InvW) * w;
                        float g_correct = (lambda1 * p1.Color.G * p1.InvW + lambda2 * p2.Color.G * p2.InvW + lambda3 * p3.Color.G * p3.InvW) * w;
                        float b_correct = (lambda1 * p1.Color.B * p1.InvW + lambda2 * p2.Color.B * p2.InvW + lambda3 * p3.Color.B * p3.InvW) * w;

                        byte r = (byte)Math.Clamp(r_correct, 0, 255);
                        byte g = (byte)Math.Clamp(g_correct, 0, 255);
                        byte b = (byte)Math.Clamp(b_correct, 0, 255);

                        buffer.ColorBuffer[pixelIndex] = new RawColor(r, g, b);
                    }
                }
                // --- POPRAWIONE KROKI AKTUALIZACJI (X rośnie o 1 -> dodaj dy) ---
                edge1Row += dy12;
                edge2Row += dy23;
                edge3Row += dy31;
            }
            // --- POPRAWIONE KROKI AKTUALIZACJI (Y rośnie o 1 -> odejmij dx) ---
            edge1Start -= dx12;
            edge2Start -= dx23;
            edge3Start -= dx31;
        }
        if (!pixelDrawn)
        {
            Console.WriteLine("!!! Rasterization finished: NO pixels drawn (Culled by edge test failure?) !!!");
        }
    }
}