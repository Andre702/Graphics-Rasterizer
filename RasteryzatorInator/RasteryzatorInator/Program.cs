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
        rasterizer.Triangle(
            new Point3D(0.5f, -0.5f, 0f, new RawColor(250, 0, 30)), 
            new Point3D(-0.5f, -0.5f, 0f, new RawColor(0, 0, 30)), 
            new Point3D(0f, 0.5f, 0f, new RawColor(255, 255, 255)));

        // new triangle
        rasterizer.Triangle(
            new Point3D(-0.5f, -0.5f, 0f, new RawColor(250, 0, 30)),
            new Point3D(-0.6f, 0f, 0f, new RawColor(0, 0, 30)),
            new Point3D(0f, 0.5f, 0f, new RawColor(255, 255, 255)));

        buffer.SaveTGA("output.tga");
    }
}
