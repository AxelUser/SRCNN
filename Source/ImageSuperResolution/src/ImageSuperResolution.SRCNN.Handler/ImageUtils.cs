﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ImageSuperResolution.SRCNN.Handler
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
    }

}