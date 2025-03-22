// See https://aka.ms/new-console-template for more information
namespace RasteryzatorInator;

class Program
{
    static void Main()
    {
        Console.WriteLine("Hello There!");

        Buffer buffer = new Buffer();
        buffer.SetSize(512, 512);
        buffer.ClearColor(0, 255, 200);
        buffer.ClearDepth(1.0f);

        Rasterizer rasterizer = new Rasterizer(buffer);

        // new triangle
        Point3D v1 = new Point3D(-0.5f, -0.5f, 0f);
        Point3D v2 = new Point3D(0.5f, -0.5f, 0f);
        Point3D v3 = new Point3D(0f, 0.5f, 0f);
        RawColor triangleColor = new RawColor(200, 120, 40);

        rasterizer.Triangle(v1, v2, v3, triangleColor);

        buffer.SaveTGA("output.tga");
    }
}
