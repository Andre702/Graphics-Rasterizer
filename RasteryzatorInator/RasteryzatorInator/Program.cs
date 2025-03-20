// See https://aka.ms/new-console-template for more information
namespace RasteryzatorInator
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Hello, World!");
            Buffer buffer = new Buffer();
            buffer.SetSize(256, 256);
            buffer.ClearColor(0, 255, 200);
            buffer.ClearDepth(1.0f);
            buffer.SaveTGA("output.tga");
        }
    }
}
