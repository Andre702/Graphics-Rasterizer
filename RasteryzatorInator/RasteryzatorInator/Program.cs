namespace RasteryzatorInator;

class Program
{
    static void Main()
    {
        Console.WriteLine("Hello There");

        const int width = 800;
        const int height = 600;

        Buffer buffer = new Buffer();
        buffer.SetSize(width, height);
        buffer.ClearColor(0, 0, 0);
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



        VertexData vA = new VertexData(new Vector3(-0.5f, -0.5f, 0.0f), new RawColor (255,0,0));
        VertexData vB = new VertexData(new Vector3(0.5f, -0.5f, 0.0f), new RawColor(0, 255, 0));
        VertexData vC = new VertexData(new Vector3(0.0f, 0.5f, 0.0f), new RawColor(0, 0, 255));

        vertexProcessor.ResetObjectTransform();

        rasterizer.DrawTriangle(vA, vB, vC);

        VertexData vD = new VertexData(new Vector3(-0.8f, -0.8f, -0.5f), RawColor.Gray(255));
        VertexData vE = new VertexData(new Vector3(-0.2f, -0.8f, -0.5f), RawColor.Gray(180));
        VertexData vF = new VertexData(new Vector3(-0.5f, -0.2f, -0.5f), RawColor.Gray(100));

        vertexProcessor.ResetObjectTransform();
        vertexProcessor.Translate(new Vector3(-0.5f, 0, 0));
        vertexProcessor.Rotate(Vector3.UnitZ, 15f);

        rasterizer.DrawTriangle(vD, vE, vF);


        buffer.SaveTGA("output_rasterized.tga");
    }
}
