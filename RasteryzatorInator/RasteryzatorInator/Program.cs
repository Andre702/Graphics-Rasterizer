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

        float fovYDegrees = 60.0f;
        float aspectRatio = (float)width / height;
        float nearPlane = 0.1f;
        float farPlane = 100.0f;
        vertexProcessor.SetPerspective(fovYDegrees, aspectRatio, nearPlane, farPlane);

        //VertexData vA;
        //VertexData vB;
        //VertexData vC;


        //vA = new VertexData(new Vector3(0, 1, 0.0f), new RawColor(250, 0, 30));
        //vB = new VertexData(new Vector3(0.7f, 0, 0.7f), new RawColor(0, 0, 30));
        //vC = new VertexData(new Vector3(0.92f, 0.0f, 0.38f), new RawColor(255, 255, 255));

        //vertexProcessor.ResetObjectTransform();

        //rasterizer.DrawTriangle(vA, vB, vC);

        //vertexProcessor.ResetObjectTransform();
        //vertexProcessor.Translate(new Vector3(-1.3f, 0, 0));
        //vertexProcessor.Rotate(Vector3.UnitZ, 102);

        //rasterizer.DrawTriangle(vA, vB, vC);


        //vertexProcessor.ResetObjectTransform();
        //vertexProcessor.Scale(new Vector3(0.6f, 0.6f, 0.6f));
        //// 2. Wywołaj metodę rysującą Rasterizera:
        //rasterizer.DrawCone(cSegments, 2, new RawColor(250, 0, 30), new RawColor(0, 0, 30), new RawColor(255, 255, 255));


        vertexProcessor.ResetObjectTransform();
        vertexProcessor.Rotate(Vector3.UnitX, -45);
        vertexProcessor.Scale(new Vector3(0.5f, 0.5f, 0.5f));


        //rasterizer.DrawCylinder(16, 3, 2, new RawColor(250, 0, 30), new RawColor(0, 0, 30), new RawColor(255, 255, 255));

        rasterizer.DrawTorus(2, 1, 12, 12, new RawColor(0, 0, 30), new RawColor(255, 255, 255));







        buffer.SaveTGA("output_rasterized.tga");
    }
}
