using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ImageSuperResolution.Common
{
    public static class ImageUtils
    {
        public static Bitmap GetBitmapFromRgba(int w, int h, byte[] rgba)
        {
            Bitmap pic = new Bitmap(w, h);
            var colors = new List<Color>();
            for (int i = 0; i < rgba.Length; i = i + 4)
            {
                byte Red = rgba[i];
                byte Green = rgba[i + 1];
                byte Blue = rgba[i + 2];
                byte Alpha = rgba[i + 3];
                Color c = Color.FromArgb(Alpha, Red, Green, Blue);
                colors.Add(c);
            }
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    pic.SetPixel(j, i, colors[i * w + j]);
                }
            }

            return pic;
        }

        public static byte[] GetRgbaFromBitmap(Bitmap image)
        {
            List<byte> rgba = new List<byte>();
            List<Color> colors = new List<Color>();
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    colors.Add(image.GetPixel(j, i));
                }
            }
            foreach (var color in colors)
            {
                rgba.Add(color.R);
                rgba.Add(color.G);
                rgba.Add(color.B);
                rgba.Add(color.A);
            }
            return rgba.ToArray();
        }

        public static Image DeserializeImage(byte[] file)
        {
            using (MemoryStream mStream = new MemoryStream(file))
            {
                return Image.FromStream(mStream);
            }
        }

        public static byte[] SerializeImage(Bitmap image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        public static Image GetPngImage(byte[] imageBytes)
        {
            using (MemoryStream mStream = new MemoryStream(imageBytes))
            {
                return Image.FromStream(mStream);
            }
        }

        public static byte[] ReadToEnd(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }

}
