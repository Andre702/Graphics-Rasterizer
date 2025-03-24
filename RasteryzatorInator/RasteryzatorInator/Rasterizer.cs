using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
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
        float cross = (p2.cX - p1.cX) * (p3.cY - p1.cY) - (p2.cY - p1.cY) * (p3.cX - p1.cX);
        if (cross > 0) return; // points are described in the wrong convention

        int width = buffer.Width;
        int height = buffer.Height;

        p1.CalculatePointCoordinates(width, height);
        p2.CalculatePointCoordinates(width, height);
        p3.CalculatePointCoordinates(width, height);

        float dx12 = p1.vX - p2.vX;
        float dx23 = p2.vX - p3.vX;
        float dx31 = p3.vX - p1.vX;
                                
        float dy12 = p1.vY - p2.vY;
        float dy23 = p2.vY - p3.vY;
        float dy31 = p3.vY - p1.vY;

        bool topLeftEdge1 = dy12 < 0 || (dy12 == 0 && dx12 < 0);
        bool topLeftEdge2 = dy23 < 0 || (dy23 == 0 && dx23 < 0);
        bool topLeftEdge3 = dy31 < 0 || (dy31 == 0 && dx31 < 0);

        float lambdaDenominator = dy23 * -dx31 + dx23 * dy31;
              
        // Triangle boundaries - screen restriction
        int minX = Math.Max(0, Math.Min(p1.vX, Math.Min(p2.vX, p3.vX)));
        int minY = Math.Max(0, Math.Min(p1.vY, Math.Min(p2.vY, p3.vY)));
        int maxX = Math.Min(width - 1, Math.Max(p1.vX, Math.Max(p2.vX, p3.vX)));
        int maxY = Math.Min(height - 1, Math.Max(p1.vY, Math.Max(p2.vY, p3.vY)));

        bool fullColorMode = false;
        if (defaultColor != null) fullColorMode = true;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                // Inside triangle equation (one sided and with TopLeft priority)
                if (dx12 * (y - p1.vY) - dy12 * (x - p1.vX) < (topLeftEdge1 ? 0 : 1)) continue;
                if (dx23 * (y - p2.vY) - dy23 * (x - p2.vX) < (topLeftEdge2 ? 0 : 1)) continue;
                if (dx31 * (y - p3.vY) - dy31 * (x - p3.vX) < (topLeftEdge3 ? 0 : 1)) continue;

                if (fullColorMode)
                {
                    buffer.ColorBuffer[y * width + x] = defaultColor.Value;
                    continue;
                }

                float lambda1 = ((p2.vY - p3.vY) * (x - p3.vX) + (p3.vX - p2.vX) * (y - p3.vY)) / lambdaDenominator;
                float lambda2 = ((p3.vY - p1.vY) * (x - p3.vX) + (p1.vX - p3.vX) * (y - p3.vY)) / lambdaDenominator;
                float lambda3 = 1 - lambda1 - lambda2;

                byte r = (byte)(lambda1 * p1.Color.R + lambda2 * p2.Color.R + lambda3 * p3.Color.R);
                byte g = (byte)(lambda1 * p1.Color.G + lambda2 * p2.Color.G + lambda3 * p3.Color.G);
                byte b = (byte)(lambda1 * p1.Color.B + lambda2 * p2.Color.B + lambda3 * p3.Color.B);

                buffer.ColorBuffer[y * width + x] = new RawColor(r, g, b);
            }
        }

    }

    private bool GetConstants(Point3D p1, Point3D p2, Point3D p3, 
        out float dx12, out float dx23, out float dx31,
        out float dy12, out float dy23, out float dy31,
        out float lambdaDenominator)
    {
        dx12 = p1.cX - p2.cX;
        dx23 = p2.cX - p3.cX;
        dx31 = p3.cX - p1.cX;

        dy12 = p1.cY - p2.cY;
        dy23 = p2.cY - p3.cY;
        dy31 = p3.cY - p1.cY;

        //lambdaDenominator = dx23 * -dx31 + dx23 * dy31;
        lambdaDenominator = (p2.cY - p3.cY) * (p1.cX - p3.cX) + (p3.cX - p2.cX) * (p1.cY - p3.cY);

        float cross = (p2.cX - p1.cX) * (p3.cY - p1.cY) - (p2.cY - p1.cY) * (p3.cX - p1.cX);
        if (cross > 0) return false; // points are described in the wrong convention

        return true;

    }

    private bool InsideTriangle(int x, int y, int werticeX1, int werticeY1, int werticeX2, int werticeY2, int werticeX3, int werticeY3)
    {
        int a = (werticeX1 - werticeX2) * (y - werticeY1) - (werticeY1 - werticeY2) * (x - werticeX1);
        int b = (werticeX2 - werticeX3) * (y - werticeY2) - (werticeY2 - werticeY3) * (x - werticeX2);
        int c = (werticeX3 - werticeX1) * (y - werticeY3) - (werticeY3 - werticeY1) * (x - werticeX3);

        return (a > 0 && b > 0 && c > 0);
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

    private RawColor InterpolateColor(int x, int y, Point3D p1, Point3D p2, Point3D p3)
    {
        float denominator = (p2.cY - p3.cY) * (p1.cX - p3.cX) + (p3.cX - p2.cX) * (p1.cY - p3.cY);
        float lambda1 = ((p2.cY - p3.cY) * (x - p3.cX) + (p3.cX - p2.cX) * (y - p3.cY)) / denominator;
        float lambda2 = ((p3.cY - p1.cY) * (x - p3.cX) + (p1.cX - p3.cX) * (y - p3.cY)) / denominator;
        float lambda3 = 1 - lambda1 - lambda2;

        byte r = (byte)(lambda1 * p1.Color.R + lambda2 * p2.Color.R + lambda3 * p3.Color.R);
        byte g = (byte)(lambda1 * p1.Color.G + lambda2 * p2.Color.G + lambda3 * p3.Color.G);
        byte b = (byte)(lambda1 * p1.Color.B + lambda2 * p2.Color.B + lambda3 * p3.Color.B);

        return new RawColor(r, g, b);
    }
}
