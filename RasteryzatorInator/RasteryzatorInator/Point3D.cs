using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasteryzatorInator
{
    internal struct Point3D
    {
        public float X, Y, Z;
        public RawColor Color;

        public Point3D(float x, float y, float z, RawColor? color = null)
        {
            X = x;
            Y = y;
            Z = z;

            if (color != null)
            {
                Color = color.Value;
            }
            else
            {
                Color = new RawColor(0, 0, 0);
            }
        }
    }
}
