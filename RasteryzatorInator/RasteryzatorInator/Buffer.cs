using System;
using System.IO;

namespace RasteryzatorInator
{
    public struct RawColor
    {
        public readonly byte R, G, B;

        public RawColor(byte r, byte g, byte b)
        {
            (R, G, B) = (r, g, b);
        }

        public static RawColor Random(Random rand)
        {
            byte r = (byte)rand.Next(256);
            byte g = (byte)rand.Next(256);
            byte b = (byte)rand.Next(256);
            return new RawColor(r, g, b);
        }

        public static RawColor Gray(byte value)
        {
            return new RawColor(value, value, value);
        }
    }

    internal class Buffer
    {
        public int Width, Height;
        public RawColor[] ColorBuffer;
        public float[] DepthBuffer;
        public RawColor ClearColorValue;
        public float ClearDepthValue;

        public void SetSize(int w, int h)
        {
            Width = w;
            Height = h;
            ColorBuffer = new RawColor[w * h];
            DepthBuffer = new float[w * h];
        }

        public void ClearColor(byte r, byte g, byte b)
        {
            ClearColorValue = new RawColor(r, g, b);
            for (int i = 0; i < ColorBuffer.Length; i++)
            {
                ColorBuffer[i] = ClearColorValue;
            }
        }

        public void ClearDepth(float depth)
        {
            ClearDepthValue = depth;
            for (int i = 0; i < DepthBuffer.Length; i++)
            {
                DepthBuffer[i] = depth;
            }
        }

        public void SaveTGA(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write((byte)0); // ID length
                bw.Write((byte)0); // Color map type
                bw.Write((byte)2); // Image type (uncompressed true-color)
                bw.Write(new byte[5]); // Color map specification
                bw.Write((short)0); // X origin
                bw.Write((short)0); // Y origin
                bw.Write((short)Width);
                bw.Write((short)Height);
                bw.Write((byte)24); // Bits per pixel
                bw.Write((byte)0); // Image descriptor

                for (int i = 0; i < ColorBuffer.Length; i++)
                {
                    bw.Write(ColorBuffer[i].B);
                    bw.Write(ColorBuffer[i].G);
                    bw.Write(ColorBuffer[i].R);
                }
            }
        }
    }
}
