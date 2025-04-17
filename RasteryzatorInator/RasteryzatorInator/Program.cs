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
        buffer.ClearColor(0, 255, 200);
        buffer.ClearDepth(1.0f);

        VertexProcessor vertexProcessor = new VertexProcessor();
        Rasterizer rasterizer = new Rasterizer(buffer, vertexProcessor);

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
        vertexProcessor.Rotate(Vector3.UnitX, 10);
        vertexProcessor.Translate(new Vector3(-3f, -1, 0));
        rasterizer.DrawCone(8, 2, new RawColor(250, 0, 30), new RawColor(0, 0, 30), new RawColor(255, 255, 255));

        vertexProcessor.ResetObjectTransform();
        vertexProcessor.Rotate(Vector3.UnitX, -20);
        vertexProcessor.Translate(new Vector3(3f, -1, 0));
        rasterizer.DrawCylinder(12, 3, 2, new RawColor(250, 0, 30), new RawColor(0, 0, 30), new RawColor(255, 255, 255));

        vertexProcessor.ResetObjectTransform();
        vertexProcessor.Rotate(Vector3.UnitX, -45);
        vertexProcessor.Scale(new Vector3(0.5f, 0.5f, 0.5f));

        rasterizer.DrawTorus(2, 1, 12, 8, new RawColor(250, 0, 30), new RawColor(0, 0, 30), new RawColor(255, 255, 255));

        buffer.SaveTGA("output_rasterized.tga");
    }
}
