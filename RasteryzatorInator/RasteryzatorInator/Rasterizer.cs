using RasteryzatorInator.MathLibrary;
using System.Diagnostics;
using System.Drawing;
namespace RasteryzatorInator;

internal class Rasterizer
{
    private readonly Buffer _buffer;
    private readonly VertexProcessor _vertexProcessor;
    private float Epsilon = 0.00001f;

    public List<Light> Lights { get; set; } = new List<Light>();


    public Rasterizer(Buffer buffer, VertexProcessor vertexProcessor)
    {
        _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        _vertexProcessor = vertexProcessor ?? throw new ArgumentNullException(nameof(vertexProcessor));
    }

    private struct ProjectedVertex
    {
        public Vector3 ScreenPos;
        public RawColor Color;
        public float InvW;

        public float X => ScreenPos.X;
        public float Y => ScreenPos.Y;
        public float Z => ScreenPos.Z;
    }

    public void DrawMesh(Mesh mesh)
    {
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));

        for (int i = 0; i < mesh.Indices.Count; i += 3)
        {
            int index1 = mesh.Indices[i];
            int index2 = mesh.Indices[i + 1];
            int index3 = mesh.Indices[i + 2];

            VertexData v1 = mesh.Vertices[index1];
            VertexData v2 = mesh.Vertices[index2];
            VertexData v3 = mesh.Vertices[index3];

            DrawTriangle(v1, v2, v3);
        }
    }

    public void DrawCone(int verticalSegments, float height, RawColor color)
    {
        Mesh coneMesh = Mesh.Cone(verticalSegments, height, color, color, color);
        DrawMesh(coneMesh);
    }

    public void DrawCone(int verticalSegments, float height, RawColor color1, RawColor color2, RawColor color3)
    {
        Mesh coneMesh = Mesh.Cone(verticalSegments, height, color1, color2, color3);
        DrawMesh(coneMesh);
    }

    public void DrawCylinder(int verticalSegments, int heightSegments, float height, RawColor color)
    {
        Mesh cylinderMesh = Mesh.Cylinder(verticalSegments, heightSegments, height, color, color, color);
        DrawMesh(cylinderMesh);
    }

    public void DrawCylinder(int verticalSegments, int heightSegments, float height, RawColor color1, RawColor color2, RawColor color3)
    {
        Mesh cylinderMesh = Mesh.Cylinder(verticalSegments, heightSegments, height, color1, color2, color3);
        DrawMesh(cylinderMesh);
    }

    public void DrawTorus(float R, float r, int outerSegments, int innerSegments, RawColor color)
    {
        Mesh torusMesh = Mesh.Torus(R, r, outerSegments, innerSegments, color, color, color);
        DrawMesh(torusMesh);
    }

    public void DrawTorus(float R, float r, int outerSegments, int innerSegments, RawColor color1, RawColor color2, RawColor color3)
    {
        Mesh torusMesh = Mesh.Torus(R, r, outerSegments, innerSegments, color1, color2, color3);
        DrawMesh(torusMesh);
    }

    private static RawColor AddColors(RawColor c1, RawColor c2)
    {
        int r = c1.R + c2.R; int g = c1.G + c2.G; int b = c1.B + c2.B;
        return new RawColor((byte)Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(b, 0, 255));
    }

    private RawColor CalculateVertexLighting(Vector3 worldPos, Vector3 worldNormal, Vector3 viewDir, RawColor baseColor)
    {
        RawColor finalColor = RawColor.Gray(0);
        foreach (var light in Lights)
        {
            if (light != null && light.IsEnabled)
            {
                finalColor = AddColors(finalColor, light.Calculate(worldPos, worldNormal, viewDir, baseColor));
            }
        }
        return finalColor;
    }

    public void DrawTriangle(VertexData v1, VertexData v2, VertexData v3)
    {
        Matrix4 objectToWorld = _vertexProcessor.ObjectToWorld;
        Matrix4 worldToView = _vertexProcessor.WorldToView;

        Vector3 cameraWorldPosition = Vector3.Zero;

        if (worldToView.TryInvert(out Matrix4 viewToWorld))
        {
            cameraWorldPosition = viewToWorld.Translation;
        }
        else
        {
            Debug.WriteLine("Warning: Could not invert WorldToView matrix. Using origin for camera position.");
        }

        // Przetwarzanie wierzchołka 1
        Vector4 worldPos4_1 = objectToWorld * new Vector4(v1.Position, 1.0f);
        Vector3 worldPos1 = worldPos4_1.Xyz;
        Vector3 worldNormal1 = (objectToWorld * new Vector4(v1.Normal, 0.0f)).Xyz.Normalized();
        Vector3 viewDir1 = (cameraWorldPosition - worldPos1).Normalized();
        RawColor litColor1 = CalculateVertexLighting(worldPos1, worldNormal1, viewDir1, v1.Color);

        // Przetwarzanie wierzchołka 2
        Vector4 worldPos4_2 = objectToWorld * new Vector4(v2.Position, 1.0f);
        Vector3 worldPos2 = worldPos4_2.Xyz;
        Vector3 worldNormal2 = (objectToWorld * new Vector4(v2.Normal, 0.0f)).Xyz.Normalized();
        Vector3 viewDir2 = (cameraWorldPosition - worldPos2).Normalized();
        RawColor litColor2 = CalculateVertexLighting(worldPos2, worldNormal2, viewDir2, v2.Color);

        // Przetwarzanie wierzchołka 3
        Vector4 worldPos4_3 = objectToWorld * new Vector4(v3.Position, 1.0f);
        Vector3 worldPos3 = worldPos4_3.Xyz;
        Vector3 worldNormal3 = (objectToWorld * new Vector4(v3.Normal, 0.0f)).Xyz.Normalized();
        Vector3 viewDir3 = (cameraWorldPosition - worldPos3).Normalized();
        RawColor litColor3 = CalculateVertexLighting(worldPos3, worldNormal3, viewDir3, v3.Color);

        Vector4 clip1 = _vertexProcessor.TransformPositionToClipSpace(v1.Position);
        Vector4 clip2 = _vertexProcessor.TransformPositionToClipSpace(v2.Position);
        Vector4 clip3 = _vertexProcessor.TransformPositionToClipSpace(v3.Position);


        if (clip1.W <= Epsilon && clip2.W <= Epsilon && clip3.W <= Epsilon)
        {
            // trójkąt jest poza widokiem
            return;
        }

        // Clip Space -> Normalzie Coordinates
        float invW1 = (MathF.Abs(clip1.W) > Epsilon) ? 1.0f / clip1.W : 0f;
        float invW2 = (MathF.Abs(clip2.W) > Epsilon) ? 1.0f / clip2.W : 0f;
        float invW3 = (MathF.Abs(clip3.W) > Epsilon) ? 1.0f / clip3.W : 0f;

        Vector3 ndc1 = new Vector3(clip1.X * invW1, clip1.Y * invW1, clip1.Z * invW1);
        Vector3 ndc2 = new Vector3(clip2.X * invW2, clip2.Y * invW2, clip2.Z * invW2);
        Vector3 ndc3 = new Vector3(clip3.X * invW3, clip3.Y * invW3, clip3.Z * invW3);

        // Normalzie Coordinates -> Screen Position
        ProjectedVertex pv1 = MapNdcToScreen(ndc1, invW1, litColor1);
        ProjectedVertex pv2 = MapNdcToScreen(ndc2, invW2, litColor2);
        ProjectedVertex pv3 = MapNdcToScreen(ndc3, invW3, litColor3);

        float screenArea = (pv2.X - pv1.X) * (pv3.Y - pv1.Y) -
                           (pv2.Y - pv1.Y) * (pv3.X - pv1.X);

        if (screenArea < 0)
        {
            // trójkąt opisany w złej orientacji
            return;
        }

        RasterizeScreenTriangle(pv1, pv2, pv3, screenArea);
    }

    private ProjectedVertex MapNdcToScreen(Vector3 ndc, float invW, RawColor color)
    {
        int width = _buffer.Width;
        int height = _buffer.Height;

        // NDC [-1, 1] -> Viewport [0, W], [0, H]
        float screenX = (ndc.X + 1.0f) * 0.5f * width;
        float screenY = (ndc.Y + 1.0f) * 0.5f * height;

        // NDC Z [-1, 1] -> Depth [0, 1]
        float depthZ = (ndc.Z + 1.0f) * 0.5f;

        return new ProjectedVertex
        {
            ScreenPos = new Vector3(screenX, screenY, depthZ),
            Color = color,
            InvW = invW
        };
    }

    private void RasterizeScreenTriangle(ProjectedVertex p1, ProjectedVertex p2, ProjectedVertex p3, float triangleArea)
    {
        int width = _buffer.Width;
        int height = _buffer.Height;

        // Współrzędne ekranowe
        float x1 = p1.X, y1 = p1.Y;
        float x2 = p2.X, y2 = p2.Y;
        float x3 = p3.X, y3 = p3.Y;

        //bounding box
        int minX = Math.Max(0, (int)MathF.Floor(Math.Min(x1, Math.Min(x2, x3))));
        int minY = Math.Max(0, (int)MathF.Floor(Math.Min(y1, Math.Min(y2, y3))));
        int maxX = Math.Min(width - 1, (int)MathF.Ceiling(Math.Max(x1, Math.Max(x2, x3))));
        int maxY = Math.Min(height - 1, (int)MathF.Ceiling(Math.Max(y1, Math.Max(y2, y3))));

        if (minX > maxX || minY > maxY) return; // Bbox jest pusty

        float dx12 = x2 - x1, dy12 = y2 - y1;
        float dx23 = x3 - x2, dy23 = y3 - y2;
        float dx31 = x1 - x3, dy31 = y1 - y3;

        bool topLeftEdge1 = dy12 < 0 || (dy12 == 0 && dx12 < 0);
        bool topLeftEdge2 = dy23 < 0 || (dy23 == 0 && dx23 < 0);
        bool topLeftEdge3 = dy31 < 0 || (dy31 == 0 && dx31 < 0);

        float lambdaDenominator = dy23 * -dx31 + dx23 * dy31;

        float invArea = 1 / triangleArea;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (dx12 * (y - y1) - dy12 * (x - x1) < (topLeftEdge1 ? 0 : 0)) continue;
                if (dx23 * (y - y2) - dy23 * (x - x2) < (topLeftEdge2 ? 0 : 0)) continue;
                if (dx31 * (y - y3) - dy31 * (x - x3) < (topLeftEdge3 ? 0 : 0)) continue;

                float lambda1 = ((y2 - y3) * (x - x3) + (x3 - x2) * (y - y3)) / lambdaDenominator;
                float lambda2 = ((y3 - y1) * (x - x3) + (x1 - x3) * (y - y3)) / lambdaDenominator;
                float lambda3 = 1 - lambda1 - lambda2;

                // Interpolacja perspektywicznie poprawna
                float interpInvW = lambda1 * p1.InvW + lambda2 * p2.InvW + lambda3 * p3.InvW;
                if (MathF.Abs(interpInvW) < Epsilon) continue; // Unikaj dzielenia przez W=nieskończoność
                float w = 1 / interpInvW;

                // Głębokość
                float depth = (lambda1 * p1.Z * p1.InvW + lambda2 * p2.Z * p2.InvW + lambda3 * p3.Z * p3.InvW) * w;

                int pixelIndex = y * width + x;
                if (depth >= 0 && depth < _buffer.DepthBuffer[pixelIndex])
                {
                    _buffer.DepthBuffer[pixelIndex] = depth;

                    // Kolor
                    float r = (lambda1 * p1.Color.R * p1.InvW + lambda2 * p2.Color.R * p2.InvW + lambda3 * p3.Color.R * p3.InvW) * w;
                    float g = (lambda1 * p1.Color.G * p1.InvW + lambda2 * p2.Color.G * p2.InvW + lambda3 * p3.Color.G * p3.InvW) * w;
                    float b = (lambda1 * p1.Color.B * p1.InvW + lambda2 * p2.Color.B * p2.InvW + lambda3 * p3.Color.B * p3.InvW) * w;

                    _buffer.ColorBuffer[pixelIndex] = new RawColor((byte)r, (byte)g, (byte)b);
                }
            }
        }
    }
}