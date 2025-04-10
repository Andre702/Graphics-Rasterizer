namespace RasteryzatorInator;

class Program
{
    static void Main()
    {
        Console.WriteLine("Hello There!");

        Buffer buffer = new Buffer();
        buffer.SetSize(512, 512);
        buffer.ClearColor(25, 40, 30);
        buffer.ClearDepth(100f);

        VertexProcessor vertexProcessor = new VertexProcessor();
        Rasterizer rasterizer = new Rasterizer(buffer, vertexProcessor);

        Vector3 eyePosition = new Vector3(0, 1, 3);
        Vector3 lookAtPoint = new Vector3(0, 0, 0);
        Vector3 upVector = Vector3.DefaultUp;

        vertexProcessor.SetPerspective(60.0f, (float)buffer.Width / buffer.Height, 0.1f, 100.0f);
        vertexProcessor.SetLookAt(eyePosition, lookAtPoint, upVector);

        vertexProcessor.ObjectToWorld = Matrix4.Identity();
        vertexProcessor.ApplyTranslation(new Vector3(-0.1f, 0, 0));

        VertexData vA = new VertexData(new Vector3(-0.6f, -0.6f, 0), new RawColor(255, 0, 0));
        VertexData vB = new VertexData(new Vector3(0.6f, -0.6f, 0), new RawColor(0, 255, 0));
        VertexData vC = new VertexData(new Vector3(0, 0.6f, 0), new RawColor(0, 0, 255));

        rasterizer.DrawTriangle(vA, vB, vC);


        buffer.SaveTGA("output.tga");
    }
}
