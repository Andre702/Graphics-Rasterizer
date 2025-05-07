using System.Drawing;
using System.IO;

namespace RasteryzatorInator
{
    public class Texture
    {
        private RawColor[] _pixels;
        private int _width;
        private int _height;

        public int Width => _width;
        public int Height => _height;

        public Texture(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Texture file not found.", filePath);

            try
            {
                // załadowanie obrazu
                using (var bmp = new Bitmap(filePath))
                {
                    _width = bmp.Width;
                    _height = bmp.Height;
                    _pixels = new RawColor[_width * _height];

                    for (int y = 0; y < _height; y++)
                    {
                        for (int x = 0; x < _width; x++)
                        {
                            Color systemColor = bmp.GetPixel(x, y);
                            _pixels[y * _width + x] = new RawColor(systemColor.R, systemColor.G, systemColor.B);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load texture from {filePath}. Error: {ex.Message}", ex);
            }
        }

        public RawColor Sample(float u, float v)
        {
            if (_pixels == null || _width == 0 || _height == 0)
            {
                return new RawColor(255, 0, 255);
            }

            u = u - MathF.Floor(u);
            v = v - MathF.Floor(v);

            // UV [0,1] na współrzędne pikseli [0, Width-1] / [0, Height-1]
            int texX = (int)(u * _width);
            int texY = (int)(v * _height);

            texX = Math.Clamp(texX, 0, _width - 1);
            texY = Math.Clamp(texY, 0, _height - 1);

            return _pixels[texY * _width + texX];
        }
    }
}
