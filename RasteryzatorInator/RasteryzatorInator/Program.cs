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
        buffer.ClearDepth(100f);

        Rasterizer rasterizer = new Rasterizer(buffer);

        // new triangle
        rasterizer.Triangle(
            new Point3D(1.2f, -0.8f, 0f, new RawColor(250, 0, 30)), 
            new Point3D(-0.4f, -0.5f, 0f, new RawColor(0, 0, 30)), 
            new Point3D(0f, 0.4f, 0f, new RawColor(255, 255, 255)));

        // new triangle
        rasterizer.Triangle(
            new Point3D(-0.4f, -0.5f, 0f, new RawColor(250, 0, 30)),
            new Point3D(-0.8f, 0f, 0f, new RawColor(0, 0, 30)),
            new Point3D(0f, 0.4f, 0f, new RawColor(255, 255, 255)));

        // new triangle
        rasterizer.Triangle(
            new Point3D(0f, -0.8f, 4f, new RawColor(250, 0, 30)),
            new Point3D(-0.8f, -0.5f, 4f, new RawColor(0, 0, 30)),
            new Point3D(0.5f, 0.7f, -6f, new RawColor(255, 255, 255)));

        buffer.SaveTGA("output.tga");
    }
}
