using RasteryzatorInator.MathLibrary;

namespace RasteryzatorInator;

class Program
{
    static void Main()
    {
        Console.WriteLine("Hello There");

        const int width = 512;
        const int height = 512;

        Buffer buffer = new Buffer();
        buffer.SetSize(width, height);
        buffer.ClearColor(0, 255, 200);
        buffer.ClearDepth(1.0f);

        VertexProcessor vertexProcessor = new VertexProcessor();
        Rasterizer rasterizer = new Rasterizer(buffer, vertexProcessor);

        // kamera
        Vector3 eyePosition = new Vector3(0, 0, 3);
        Vector3 focusPoint = Vector3.Zero;
        Vector3 upVector = Vector3.Up;
        vertexProcessor.SetLookAt(eyePosition, focusPoint, upVector);

        float fovYDegrees = 30.0f;
        float aspectRatio = (float)width / height;
        //float aspectRatio = 1;
        float nearPlane = 0.1f;
        float farPlane = 100.0f;
        vertexProcessor.SetPerspective(fovYDegrees, aspectRatio, nearPlane, farPlane);

        VertexData vA;
        VertexData vB;
        VertexData vC;


        vA = new VertexData(new Vector3(-0.5f, -0.5f, 0.0f), new RawColor(250, 0, 30));
        vB = new VertexData(new Vector3(0.5f, -0.5f, 0.0f), new RawColor(0, 0, 30));
        vC = new VertexData(new Vector3(0.0f, 0.5f, 0.0f), new RawColor(255, 255, 255));

        rasterizer.DrawTriangle(vA, vB, vC);

        buffer.SaveTGA("output_rasterized.tga");
    }
}
