using RasteryzatorInator.MathLibrary;

namespace RasteryzatorInator;

class Program
{
    static void Main()
    {
        Console.WriteLine("Hello There");

        const int width = 750;
        const int height = 480;

        Buffer buffer = new Buffer();
        buffer.SetSize(width, height);
        buffer.ClearColor(25, 25, 25);
        buffer.ClearDepth(1.0f);

        VertexProcessor vertexProcessor = new VertexProcessor();
        Rasterizer rasterizer = new Rasterizer(buffer, vertexProcessor);


        var directionalLight = new LightDirectional(
                direction: new Vector3(0.5f, -1.0f, -0.7f).Normalized(),
                ambient: new RawColor(20, 20, 20),
                diffuse: new RawColor(100, 100, 100),
                specular: new RawColor(100, 100, 100),
                shininess: 32f
            );

        var pointLight = new LightPoint(
            position: new Vector3(0f, 1f, 3f),
            ambient: new RawColor(10, 10, 10),
            diffuse: new RawColor(255, 230, 180),
            specular: new RawColor(255, 255, 255),
            shininess: 32f
        )
        {
            ConstantAttenuation = 1.0f,
            LinearAttenuation = 0.00f,
            QuadraticAttenuation = 0.00f
        };

        pointLight.SetSpotlight(new Vector3(0, 0, -1), 30);


        rasterizer.Lights.Add(directionalLight);
        rasterizer.Lights.Add(pointLight);

        // kamera
        Vector3 eyePosition = new Vector3(0, 0, 5);
        Vector3 focusPoint = Vector3.Zero;
        Vector3 upVector = Vector3.Up;
        vertexProcessor.SetLookAt(eyePosition, focusPoint, upVector);

        float fovYDegrees = 60.0f;
        float aspectRatio = (float)width / height;
        float nearPlane = 0.1f;
        float farPlane = 100.0f;
        vertexProcessor.SetPerspective(fovYDegrees, aspectRatio, nearPlane, farPlane);

        vertexProcessor.ResetObjectTransform();
        vertexProcessor.Rotate(Vector3.UnitX, -30);
        vertexProcessor.Translate(new Vector3(-2.6f, -1, 0));
        rasterizer.DrawCone(8, 2, new RawColor(255, 255, 255));

        vertexProcessor.ResetObjectTransform();
        vertexProcessor.Rotate(Vector3.UnitX, -40);
        vertexProcessor.Translate(new Vector3(3f, -1, 0));
        rasterizer.DrawCylinder(32, 3, 2, new RawColor(255, 255, 0));

        vertexProcessor.ResetObjectTransform();
        vertexProcessor.Rotate(Vector3.UnitX, -45);
        vertexProcessor.Scale(new Vector3(0.5f, 0.5f, 0.5f));

        rasterizer.DrawTorus(2, 1, 6, 6, new RawColor(0, 255, 255));

        buffer.SaveTGA("output_rasterized.tga");
    }
}
