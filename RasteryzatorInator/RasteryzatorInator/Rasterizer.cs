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
        public Vector3 ScreenPos;
        public RawColor Color;
        public float InvW;
    }                            

    public void DrawTriangle(VertexData v0, VertexData v1, VertexData v2)
    {
        Vector4 clip0 = vertexProcessor.TransformPositionToClipSpace(v0.Position);
        Vector4 clip1 = vertexProcessor.TransformPositionToClipSpace(v1.Position);
        Vector4 clip2 = vertexProcessor.TransformPositionToClipSpace(v2.Position);

        float epsilon = 0.0001f;
        if (clip0.W >= -epsilon || clip1.W >= -epsilon || clip2.W >= -epsilon)
        {
            return;
        }

        float invW0 = 1.0f / clip0.W;
        float invW1 = 1.0f / clip1.W;
        float invW2 = 1.0f / clip2.W;

        Vector3 ndc0 = new Vector3(clip0.X * invW0, clip0.Y * invW0, clip0.Z * invW0);
        Vector3 ndc1 = new Vector3(clip1.X * invW1, clip1.Y * invW1, clip1.Z * invW1);
        Vector3 ndc2 = new Vector3(clip2.X * invW2, clip2.Y * invW2, clip2.Z * invW2);

        ProjectedVertex pv0 = MapToScreen(ndc0, invW0, v0.Color);
        ProjectedVertex pv1 = MapToScreen(ndc1, invW1, v1.Color);
        ProjectedVertex pv2 = MapToScreen(ndc2, invW2, v2.Color);

        float crossProductScreen = (pv1.ScreenPos.X - pv0.ScreenPos.X) * (pv2.ScreenPos.Y - pv0.ScreenPos.Y) -
                                       (pv1.ScreenPos.Y - pv0.ScreenPos.Y) * (pv2.ScreenPos.X - pv0.ScreenPos.X);
        if (crossProductScreen < 0)
        {
            return;
        }

        RasterizeScreenTriangle(pv0, pv1, pv2);

    }

    private ProjectedVertex MapToScreen(Vector3 ndc, float invW, RawColor color)
    {
        int width = buffer.Width;
        int height = buffer.Height;

        float screenX = (ndc.X + 1.0f) * 0.5f * width;
        float screenY = (1.0f - ndc.Y) * 0.5f * height;

        float depthZ = (ndc.Z + 1.0f) * 0.5f;

        return new ProjectedVertex
        {
            ScreenPos = new Vector3(screenX, screenY, depthZ),
            Color = color,
            InvW = invW
        };
    }

    private void RasterizeScreenTriangle(ProjectedVertex p1, ProjectedVertex p2, ProjectedVertex p3)
    {
        int width = buffer.Width;
        int height = buffer.Height;

        float x1 = p1.ScreenPos.X, y1 = p1.ScreenPos.Y;
        float x2 = p2.ScreenPos.X, y2 = p2.ScreenPos.Y;
        float x3 = p3.ScreenPos.X, y3 = p3.ScreenPos.Y;

        int minX = Math.Max(0, (int)MathF.Ceiling(Math.Min(x1, Math.Min(x2, x3)) - 0.5f));
        int minY = Math.Max(0, (int)MathF.Ceiling(Math.Min(y1, Math.Min(y2, y3)) - 0.5f));
        int maxX = Math.Min(width - 1, (int)MathF.Floor(Math.Max(x1, Math.Max(x2, x3)) + 0.5f));
        int maxY = Math.Min(height - 1, (int)MathF.Floor(Math.Max(y1, Math.Max(y2, y3)) + 0.5f));

        if (minX > maxX || minY > maxY) return;

        float dx12 = x2 - x1; float dy12 = y2 - y1;
        float dx23 = x3 - x2; float dy23 = y3 - y2;
        float dx31 = x1 - x3; float dy31 = y1 - y3;

        float startPx = minX + 0.5f;
        float startPy = minY + 0.5f;
        float edge1Start = (startPx - x1) * dy12 - (startPy - y1) * dx12;
        float edge2Start = (startPx - x2) * dy23 - (startPy - y2) * dx23;
        float edge3Start = (startPx - x3) * dy31 - (startPy - y3) * dx31;

        float baryDenominator = (x3 - x1) * dy12 - (y3 - y1) * dx12;
        if (Math.Abs(baryDenominator) < 0.00001f) return;
        float invBaryDenominator = 1.0f / baryDenominator;


        // Pętla po pikselach w bounding box
        for (int y = minY; y <= maxY; y++)
        {
            float edge1Row = edge1Start;
            float edge2Row = edge2Start;
            float edge3Row = edge3Start;

            for (int x = minX; x <= maxX; x++)
            {
                if (edge1Row >= -1e-5f && edge2Row >= -1e-5f && edge3Row >= -1e-5f)
                {
                    float lambda3 = edge1Row * invBaryDenominator;
                    float lambda1 = edge2Row * invBaryDenominator;
                    float lambda2 = edge3Row * invBaryDenominator;

                    float interpolatedInvW = lambda1 * p1.InvW + lambda2 * p2.InvW + lambda3 * p3.InvW;
                    if (Math.Abs(interpolatedInvW) < float.Epsilon) continue;

                    float interpolatedDepth = lambda1 * p1.ScreenPos.Z + lambda2 * p2.ScreenPos.Z + lambda3 * p3.ScreenPos.Z;

                    int pixelIndex = y * width + x;
                    if (interpolatedDepth >= 0 && interpolatedDepth < buffer.DepthBuffer[pixelIndex])
                    {
                        buffer.DepthBuffer[pixelIndex] = interpolatedDepth;
                        byte r = (byte)Math.Clamp(lambda1 * p1.Color.R + lambda2 * p2.Color.R + lambda3 * p3.Color.R, 0, 255);
                        byte g = (byte)Math.Clamp(lambda1 * p1.Color.G + lambda2 * p2.Color.G + lambda3 * p3.Color.G, 0, 255);
                        byte b = (byte)Math.Clamp(lambda1 * p1.Color.B + lambda2 * p2.Color.B + lambda3 * p3.Color.B, 0, 255);

                        buffer.ColorBuffer[pixelIndex] = new RawColor(r, g, b);
                    }
                }
                edge1Row += dy12;
                edge2Row += dy23;
                edge3Row += dy31;
            }
            edge1Start -= dx12;
            edge2Start -= dx23;
            edge3Start -= dx31;
        }
    }
}
