using RasteryzatorInator.MathLibrary;
using System.Diagnostics;

namespace RasteryzatorInator;

public enum ShadingMode
{
    Flat,    // jedna normalna na trójkąt, oświetlenie per piksel
    Gouraud, // po wierzchołkach, interpolacja koloru
    Phong    // interpolacja normalnych, oświetlenie per piksel
}

internal class Rasterizer
{
    private readonly Buffer _buffer;
    private readonly VertexProcessor _vertexProcessor;
    private readonly Vector3 _cameraPos;
    private Texture? _currentTexture;
    private bool _applyLight = true;
    private float Epsilon = 0.00001f;

    public List<Light> Lights { get; set; } = new List<Light>();


    public Rasterizer(Buffer buffer, VertexProcessor vertexProcessor, Vector3 cameraPos)
    {
        _buffer = buffer;
        _vertexProcessor = vertexProcessor;
        _cameraPos = cameraPos;
    }

    private struct ProjectedVertex
    {
        public Vector3 ScreenPos;
        public RawColor Color;
        public float InvW;

        public Vector3 WorldPos;
        public Vector3 WorldNormal;
        public Vector2 TexCoord;

        public float X => ScreenPos.X;
        public float Y => ScreenPos.Y;
        public float Z => ScreenPos.Z;
    }

    public void DrawMesh(Mesh mesh, ShadingMode shadingMode)
    {
        for (int i = 0; i < mesh.Indices.Count; i += 3)
        {
            int index1 = mesh.Indices[i];
            int index2 = mesh.Indices[i + 1];
            int index3 = mesh.Indices[i + 2];

            VertexData v1 = mesh.Vertices[index1];
            VertexData v2 = mesh.Vertices[index2];
            VertexData v3 = mesh.Vertices[index3];

            DrawTriangle(v1, v2, v3, shadingMode);
        }
    }

    private void DrawPrimitive(Mesh mesh, ShadingMode shadingMode, bool applyLight, Texture? texture)
    {
        _applyLight = applyLight;
        if (texture != null)
        {
            _currentTexture = texture;
            mesh.ApplyCylindricalTextureMapping();
        }
        else _currentTexture = null;

        DrawMesh(mesh, shadingMode);
    }

    public void DrawCone(int verticalSegments, float height, ShadingMode shadingMode, RawColor color, bool applyLight, Texture? texture = null)
    {
        Mesh coneMesh = Mesh.Cone(verticalSegments, height, color, color, color);
        DrawPrimitive(coneMesh, shadingMode, applyLight, texture);
    }

    public void DrawCone(int verticalSegments, float height, ShadingMode shadingMode, RawColor color1, RawColor color2, RawColor color3, bool applyLight, Texture? texture = null)
    {

        Mesh coneMesh = Mesh.Cone(verticalSegments, height, color1, color2, color3);
        DrawPrimitive(coneMesh, shadingMode, applyLight, texture);
    }

    public void DrawCylinder(int verticalSegments, int heightSegments, float height, ShadingMode shadingMode, RawColor color, bool applyLight, Texture? texture = null)
    {
        Mesh cylinderMesh = Mesh.Cylinder(verticalSegments, heightSegments, height, color, color, color);
        DrawPrimitive(cylinderMesh, shadingMode, applyLight, texture);
    }

    public void DrawCylinder(int verticalSegments, int heightSegments, float height, ShadingMode shadingMode, RawColor color1, RawColor color2, RawColor color3, bool applyLight, Texture? texture = null)
    {
        Mesh cylinderMesh = Mesh.Cylinder(verticalSegments, heightSegments, height, color1, color2, color3);
        DrawPrimitive(cylinderMesh, shadingMode, applyLight, texture);
    }

    public void DrawTorus(float R, float r, int outerSegments, int innerSegments, ShadingMode shadingMode, RawColor color, bool applyLight, Texture? texture = null)
    {
        Mesh torusMesh = Mesh.Torus(R, r, outerSegments, innerSegments, color, color, color);
        DrawPrimitive(torusMesh, shadingMode, applyLight, texture);
    }

    public void DrawTorus(float R, float r, int outerSegments, int innerSegments, ShadingMode shadingMode, RawColor color1, RawColor color2, RawColor color3, bool applyLight, Texture? texture = null)
    {
        Mesh torusMesh = Mesh.Torus(R, r, outerSegments, innerSegments, color1, color2, color3);
        DrawPrimitive(torusMesh, shadingMode, applyLight, texture);
    }

    private static RawColor AddColors(RawColor c1, RawColor c2)
    {
        int r = c1.R + c2.R; int g = c1.G + c2.G; int b = c1.B + c2.B;
        return new RawColor((byte)Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(b, 0, 255));
    }

    private RawColor CalculateLight(Vector3 worldPos, Vector3 worldNormal, Vector3 viewDir, RawColor baseColor)
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

    public void DrawTriangle(VertexData v1, VertexData v2, VertexData v3, ShadingMode shadingMode)
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
        RawColor colorForPv1 = v1.Color;

        Vector4 worldPos4_2 = objectToWorld * new Vector4(v2.Position, 1.0f);
        Vector3 worldPos2 = worldPos4_2.Xyz;
        RawColor colorForPv2 = v1.Color;

        Vector4 worldPos4_3 = objectToWorld * new Vector4(v3.Position, 1.0f);
        Vector3 worldPos3 = worldPos4_3.Xyz;
        RawColor colorForPv3 = v1.Color;

        Vector3 worldNormal1 = (objectToWorld * new Vector4(v1.Normal, 0.0f)).Xyz.Normalized();
        Vector3 worldNormal2 = (objectToWorld * new Vector4(v2.Normal, 0.0f)).Xyz.Normalized();
        Vector3 worldNormal3 = (objectToWorld * new Vector4(v3.Normal, 0.0f)).Xyz.Normalized();

        if (shadingMode == ShadingMode.Gouraud) // osobne kolory dla wierzchołków przy cieniowaniu per Vertex:
        {
            // Ignoruje tekstury! 
            Vector3 viewDir1 = (cameraWorldPosition - worldPos1).Normalized();
            colorForPv1 = CalculateLight(worldPos1, worldNormal1, viewDir1, v1.Color);

            Vector3 viewDir2 = (cameraWorldPosition - worldPos2).Normalized();
            colorForPv2 = CalculateLight(worldPos2, worldNormal2, viewDir2, v2.Color);

            Vector3 viewDir3 = (cameraWorldPosition - worldPos3).Normalized();
            colorForPv3 = CalculateLight(worldPos3, worldNormal3, viewDir3, v3.Color);
        }

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
        ProjectedVertex pv1 = MapNdcToScreen(ndc1, invW1, colorForPv1, worldNormal1, worldPos1, v1.TexCoord);
        ProjectedVertex pv2 = MapNdcToScreen(ndc2, invW2, colorForPv2, worldNormal2, worldPos2, v2.TexCoord);
        ProjectedVertex pv3 = MapNdcToScreen(ndc3, invW3, colorForPv3, worldNormal3, worldPos3, v3.TexCoord);


        float screenArea = (pv2.X - pv1.X) * (pv3.Y - pv1.Y) -
                           (pv2.Y - pv1.Y) * (pv3.X - pv1.X);

        if (screenArea < 0)
        {
            // trójkąt opisany w złej orientacji
            return;
        }

        RasterizeScreenTriangle(pv1, pv2, pv3, screenArea, shadingMode);
    }

    private ProjectedVertex MapNdcToScreen(Vector3 ndc, float invW, RawColor color, Vector3 worldNormal, Vector3 worldPosition, Vector2 texCoord)
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
            InvW = invW,
            WorldPos = worldPosition,
            WorldNormal = worldNormal,
            TexCoord = texCoord
        };
    }

    private void RasterizeScreenTriangle(ProjectedVertex p1, ProjectedVertex p2, ProjectedVertex p3, float triangleArea,
        ShadingMode shadingMode)
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

        Vector3 triangleWorldNormal = Vector3.Zero;

        RawColor flatColor = RawColor.Gray(0);

        if (shadingMode == ShadingMode.Flat)
        {
            Vector3 edge1 = p2.WorldPos - p1.WorldPos;
            Vector3 edge2 = p3.WorldPos - p1.WorldPos;
            triangleWorldNormal = Vector3.Cross(edge1, edge2).Normalized();
        }

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
                if (MathF.Abs(interpInvW) < Epsilon) continue;
                float w = 1 / interpInvW;

                // Głębokość
                float depth = (lambda1 * p1.Z * p1.InvW + lambda2 * p2.Z * p2.InvW + lambda3 * p3.Z * p3.InvW) * w;

                Vector2 interpTexCoord = Vector2.Zero;
                if (_currentTexture != null)
                {
                    interpTexCoord = InterpolateVector2(p1.TexCoord, p2.TexCoord, p3.TexCoord,
                                          lambda1, lambda2, lambda3,
                                          p1.InvW, p2.InvW, p3.InvW, w);
                }

                int pixelIndex = y * width + x;
                if (depth >= 0 && depth < _buffer.DepthBuffer[pixelIndex])
                {
                    _buffer.DepthBuffer[pixelIndex] = depth;

                    if (shadingMode == ShadingMode.Flat)
                    {
                        Vector3 interpWorldPos = InterpolateVector3(p1.WorldPos, p2.WorldPos, p3.WorldPos, lambda1, lambda2, lambda3, p1.InvW, p2.InvW, p3.InvW, w);
                        Vector3 viewDir = (_cameraPos - interpWorldPos).Normalized();

                        RawColor baseColor = p1.Color;
                        if (_currentTexture != null) baseColor = _currentTexture.Sample(interpTexCoord.X, interpTexCoord.Y);

                        if (_applyLight) _buffer.ColorBuffer[pixelIndex] = CalculateLight(interpWorldPos, triangleWorldNormal, viewDir, baseColor);
                        else _buffer.ColorBuffer[pixelIndex] = baseColor;
                    }

                    else if (shadingMode == ShadingMode.Phong)
                    {
                        Vector3 interpWorldPos = InterpolateVector3(p1.WorldPos, p2.WorldPos, p3.WorldPos, lambda1, lambda2, lambda3, p1.InvW, p2.InvW, p3.InvW, w);
                        Vector3 viewDir = (_cameraPos - interpWorldPos).Normalized();

                        Vector3 pixelNormal = InterpolateNormal(p1.WorldNormal, p2.WorldNormal, p3.WorldNormal, lambda1, lambda2, lambda3, p1.InvW, p2.InvW, p3.InvW, w);
                        if (pixelNormal.LengthSquared() < Epsilon)
                        {
                            pixelNormal = triangleWorldNormal;
                            if (pixelNormal.LengthSquared() < Epsilon) pixelNormal = Vector3.UnitY;
                        }

                        RawColor baseMaterialColor;
                        if (_currentTexture != null) baseMaterialColor = _currentTexture.Sample(interpTexCoord.X, interpTexCoord.Y);
                        else baseMaterialColor = InterpolateColor(p1.Color, p2.Color, p3.Color, lambda1, lambda2, lambda3, p1.InvW, p2.InvW, p3.InvW, w);

                        if (_applyLight) _buffer.ColorBuffer[pixelIndex] = CalculateLight(interpWorldPos, pixelNormal, viewDir, baseMaterialColor);
                        else _buffer.ColorBuffer[pixelIndex] = baseMaterialColor;
                    }

                    else
                    {
                        float r = (lambda1 * p1.Color.R * p1.InvW + lambda2 * p2.Color.R * p2.InvW + lambda3 * p3.Color.R * p3.InvW) * w;
                        float g = (lambda1 * p1.Color.G * p1.InvW + lambda2 * p2.Color.G * p2.InvW + lambda3 * p3.Color.G * p3.InvW) * w;
                        float b = (lambda1 * p1.Color.B * p1.InvW + lambda2 * p2.Color.B * p2.InvW + lambda3 * p3.Color.B * p3.InvW) * w;

                        _buffer.ColorBuffer[pixelIndex] = new RawColor((byte)r, (byte)g, (byte)b);
                    }
                }
            }
        }
    }

    private Vector2 InterpolateVector2(Vector2 v1, Vector2 v2, Vector2 v3,
                                   float l1, float l2, float l3,
                                   float invW1, float invW2, float invW3, float w)
    {
        // Atrybut_i / w_i  ->  (A_i/w_i) * lambda_i ... sum -> result * w_persp
        float x = (l1 * v1.X * invW1 + l2 * v2.X * invW2 + l3 * v3.X * invW3) * w;
        float y = (l1 * v1.Y * invW1 + l2 * v2.Y * invW2 + l3 * v3.Y * invW3) * w;
        return new Vector2(x, y);
    }

    private Vector3 InterpolateVector3(Vector3 v1, Vector3 v2, Vector3 v3, float l1, float l2, float l3, float invW1, float invW2, float invW3, float w)
    {
        float x = (l1 * v1.X * invW1 + l2 * v2.X * invW2 + l3 * v3.X * invW3) * w;
        float y = (l1 * v1.Y * invW1 + l2 * v2.Y * invW2 + l3 * v3.Y * invW3) * w;
        float z = (l1 * v1.Z * invW1 + l2 * v2.Z * invW2 + l3 * v3.Z * invW3) * w;
        return new Vector3(x, y, z);
    }

    private Vector3 InterpolateNormal(Vector3 n1, Vector3 n2, Vector3 n3,
                                   float l1, float l2, float l3,
                                   float invW1, float invW2, float invW3, float w)
    {
        float nx_w = l1 * n1.X * invW1 + l2 * n2.X * invW2 + l3 * n3.X * invW3;
        float ny_w = l1 * n1.Y * invW1 + l2 * n2.Y * invW2 + l3 * n3.Y * invW3;
        float nz_w = l1 * n1.Z * invW1 + l2 * n2.Z * invW2 + l3 * n3.Z * invW3;

        Vector3 interpolatedNormal = new Vector3(nx_w * w, ny_w * w, nz_w * w);

        return interpolatedNormal.Normalized();
    }

    private RawColor InterpolateColor(RawColor c1, RawColor c2, RawColor c3, float l1, float l2, float l3, float invW1, float invW2, float invW3, float w)
    {
        float r = (l1 * c1.R * invW1 + l2 * c2.R * invW2 + l3 * c3.R * invW3) * w;
        float g = (l1 * c1.G * invW1 + l2 * c2.G * invW2 + l3 * c3.G * invW3) * w;
        float b = (l1 * c1.B * invW1 + l2 * c2.B * invW2 + l3 * c3.B * invW3) * w;
        return new RawColor(
            (byte)Math.Clamp(r, 0f, 255f),
            (byte)Math.Clamp(g, 0f, 255f),
            (byte)Math.Clamp(b, 0f, 255f)
        );
    }
}