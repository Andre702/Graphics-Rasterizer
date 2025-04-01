using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasteryzatorInator;

internal class Rasterizer
{
    private Buffer buffer;

    public Rasterizer(Buffer _buffer)
    {
        buffer = _buffer;
    }

    public void Triangle(Point3D p1, Point3D p2, Point3D p3, RawColor color)
    {
        int width = buffer.Width;
        int height = buffer.Height;

        // Canonical view to Screen coordinates
        int screenX1 = (int)((p1.X + 1) * 0.5f * width);
        int screenY1 = (int)((p1.Y + 1) * 0.5f * height);

        int screenX2 = (int)((p2.X + 1) * 0.5f * width);
        int screenY2 = (int)((p2.Y + 1) * 0.5f * height);

        int screenX3 = (int)((p3.X + 1) * 0.5f * width);
        int screenY3 = (int)((p3.Y + 1) * 0.5f * height);
        
        // Triangle boundaries
        int minX = Math.Min(screenX1, Math.Min(screenX2, screenX3));
        int minY = Math.Min(screenY1, Math.Min(screenY2, screenY3));
        int maxX = Math.Max(screenX1, Math.Max(screenX2, screenX3));
        int maxY = Math.Max(screenY1, Math.Max(screenY2, screenY3));


        for (int x = 0; x <= maxX; x++)
        {
            for (int y = minX; y <= maxY; y++)
            {
                if (InsideTriangle(x, y, screenX1, screenY1, screenX2, screenY2, screenX3, screenY3))
                {
                    buffer.ColorBuffer[y * width + x] = color;
                }
            }
        }
    }

    private bool InsideTriangle(int x, int y, int werticeX1, int werticeY1, int werticeX2, int werticeY2, int werticeX3, int werticeY3)
    {
        int a = (x - werticeX2) * (werticeY1 - werticeY2) - (y - werticeY2) * (werticeX1 - werticeX2);
        int b = (x - werticeX3) * (werticeY2 - werticeY3) - (y - werticeY3) * (werticeX2 - werticeX3);
        int c = (x - werticeX1) * (werticeY3 - werticeY1) - (y - werticeY1) * (werticeX3 - werticeX1);

        return (a > 0 && b > 0 && c > 0);
    }
}
