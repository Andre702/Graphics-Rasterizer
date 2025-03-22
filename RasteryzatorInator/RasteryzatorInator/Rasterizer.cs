using System;
using System.Collections.Generic;
using System.Drawing;
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

    public void Triangle(Point3D p1, Point3D p2, Point3D p3, RawColor? defaultColor = null)
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
        
        // Triangle boundaries - screen restriction
        int minX = Math.Max(0, Math.Min(screenX1, Math.Min(screenX2, screenX3)));
        int minY = Math.Max(0, Math.Min(screenY1, Math.Min(screenY2, screenY3)));
        int maxX = Math.Min(width - 1, Math.Max(screenX1, Math.Max(screenX2, screenX3)));
        int maxY = Math.Min(height - 1, Math.Max(screenY1, Math.Max(screenY2, screenY3)));

        //for (int y = minY; y <= maxY; y++)
        //{
        //    for (int x = minX; x <= maxX; x++)
        //    {
        //        if (InsideTriangle(x, y, screenX1, screenY1, screenX2, screenY2, screenX3, screenY3))
        //        {
        //            buffer.ColorBuffer[y * width + x] = color;
        //        }
        //    }
        //}

        bool fullColorMode = false;
        if (defaultColor != null) fullColorMode = true;

        if (!fullColorMode)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (InsideTriangle(x, y, screenX1, screenY1, screenX2, screenY2, screenX3, screenY3))
                    {
                        var bary = BarycentricCoordinates(x, y, screenX1, screenY1, screenX2, screenY2, screenX3, screenY3);

                        RawColor interpolatedColor = InterpolateColor(bary, p1.Color, p2.Color, p3.Color);
                        buffer.ColorBuffer[y * width + x] = interpolatedColor;
                    }
                }
            }
        }
        else
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (InsideTriangle(x, y, screenX1, screenY1, screenX2, screenY2, screenX3, screenY3))
                    {
                        buffer.ColorBuffer[y * width + x] = defaultColor.Value;
                    }
                }
            }
        }
    }

    private bool InsideTriangle(int x, int y, int werticeX1, int werticeY1, int werticeX2, int werticeY2, int werticeX3, int werticeY3)
    {
        int a = (werticeX1 - werticeX2) * (y - werticeY1) - (werticeY1 - werticeY2) * (x - werticeX1);
        int b = (werticeX2 - werticeX3) * (y - werticeY2) - (werticeY2 - werticeY3) * (x - werticeX2);
        int c = (werticeX3 - werticeX1) * (y - werticeY3) - (werticeY3 - werticeY1) * (x - werticeX3);

        return (a > 0 && b > 0 && c > 0) || (a < 0 && b < 0 && c < 0);
    }

    private (float, float, float) BarycentricCoordinates(int x, int y, int x1, int y1, int x2, int y2, int x3, int y3)
    {
        float denominator = (y2 - y3) * (x1 - x3) + (x3 - x2) * (y1 - y3);
        float lambda1 = ((y2 - y3) * (x - x3) + (x3 - x2) * (y - y3)) / denominator;
        float lambda2 = ((y3 - y1) * (x - x3) + (x1 - x3) * (y - y3)) / denominator;
        float lambda3 = 1 - lambda1 - lambda2;

        return (lambda1, lambda2, lambda3);
    }

    private RawColor InterpolateColor((float, float, float) bary, RawColor color1, RawColor color2, RawColor color3)
    {
        float lambda1 = bary.Item1;
        float lambda2 = bary.Item2;
        float lambda3 = bary.Item3;

        byte r = (byte)(lambda1 * color1.R + lambda2 * color2.R + lambda3 * color3.R);
        byte g = (byte)(lambda1 * color1.G + lambda2 * color2.G + lambda3 * color3.G);
        byte b = (byte)(lambda1 * color1.B + lambda2 * color2.B + lambda3 * color3.B);

        return new RawColor(r, g, b);
    }
}
