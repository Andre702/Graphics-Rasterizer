using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasteryzatorInator
{
    internal struct Point3D
    {
        public float cX, cY, cZ; // cannonical
        public int vX, vY; // screen / volume

        public RawColor Color;

        public Point3D(float x, float y, float z, RawColor? color = null)
        {
            cX = x;
            cY = y;
            cZ = z;

            if (color != null)
            {
                Color = color.Value;
            }
            else
            {
                Color = new RawColor(0, 0, 0);
            }
        }

        public void CalculatePointCoordinates(float width, float height)
        {
            vX = (int)((cX + 1) * 0.5f * width);
            vY = (int)((cY + 1) * 0.5f * height);

        }
    }
}
