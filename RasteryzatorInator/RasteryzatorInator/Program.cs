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
        //vertexProcessor.ApplyTranslation(new Vector3(-2f, 4f, 0));
        vertexProcessor.ApplyRotation(30, new Vector3(0, 0, 1));
        vertexProcessor.ApplyScale(new Vector3(2, 2, 2));

        VertexData vA = new VertexData(new Vector3(-0.6f, -0.6f, 1), new RawColor(255, 0, 0));
        VertexData vB = new VertexData(new Vector3(0.6f, -0.6f, 1), new RawColor(0, 255, 0));
        VertexData vC = new VertexData(new Vector3(0, 0.6f, 1), new RawColor(0, 0, 255));

        Console.WriteLine("--- Initial Data ---");
        Console.WriteLine($"Eye Position: {eyePosition.X}, {eyePosition.Y}, {eyePosition.Z}");
        Console.WriteLine($"LookAt Point: {lookAtPoint.X}, {lookAtPoint.Y}, {lookAtPoint.Z}");
        Console.WriteLine($"Up Vector:    {upVector.X}, {upVector.Y}, {upVector.Z}");
        Console.WriteLine($"Near Plane: {0.1f}, Far Plane: {100.0f}");
        Console.WriteLine($"ObjectToWorld (before Draw): \n{vertexProcessor.ObjectToWorld}");
        Console.WriteLine($"WorldToView: \n{vertexProcessor.WorldToView}");
        Console.WriteLine($"ViewToProjection: \n{vertexProcessor.ViewToProjection}");
        Console.WriteLine($"Vertex A (Object): {vA.Position.X}, {vA.Position.Y}, {vA.Position.Z}");
        Console.WriteLine($"Vertex B (Object): {vB.Position.X}, {vB.Position.Y}, {vB.Position.Z}");
        Console.WriteLine($"Vertex C (Object): {vC.Position.X}, {vC.Position.Y}, {vC.Position.Z}");

        rasterizer.ProcessTriangle(vA, vB, vC);


        buffer.SaveTGA("output2.tga");
    }
}
