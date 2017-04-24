using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageSuperResolution.SRCNN.Handler.Upscalling
{
    public class ImageChannel
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public byte[] Buffer { get; set; }

        public ImageChannel(int width, int height, byte[] buffer = null)
        {
            Width = width;
            Height = height;

            if (buffer != null)
            {
                if (buffer.Length != width * height)
                {
                    throw new ArgumentException("Illegal buffer length");
                }
                else
                {
                    Buffer = buffer;
                }
            }
            else
            {
                Buffer = new byte[width * height];
            }
        }

        public static byte[] ChannelCompose(ImageChannels channels)
        {
            int width = channels.Red.Width;
            int height = channels.Red.Height;
            byte[] image = new byte[width * height * 4];
            for (int i = 0; i < width * height; i++)
            {
                image[i * 4] = channels.Red.Buffer[i];
                image[i * 4 + 1] = channels.Green.Buffer[i];
                image[i * 4 + 2] = channels.Blue.Buffer[i];
                image[i * 4 + 3] = channels.Alpha.Buffer[i];
            }
            return image;
        }

        public static ImageChannels ChannelDecompose(byte[] image, int width, int height)
        {
            ImageChannel imageR = new ImageChannel(width, height);
            ImageChannel imageG = new ImageChannel(width, height);
            ImageChannel imageB = new ImageChannel(width, height);
            ImageChannel imageA = new ImageChannel(width, height);
            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    int index = w + (h * width);
                    imageR.Buffer[index] = image[(w * 4) + (h * width * 4)];
                    imageG.Buffer[index] = image[(w * 4) + (h * width * 4) + 1];
                    imageB.Buffer[index] = image[(w * 4) + (h * width * 4) + 2];
                    imageA.Buffer[index] = image[(w * 4) + (h * width * 4) + 3];
                }
            }

            return new ImageChannels()
            {
                Alpha = imageA,
                Red = imageR,
                Green = imageG,
                Blue = imageB
            };
        }

        public byte[] ChannelDecompose(ImageChannels channels)
        {
            int width = channels.Red.Width;
            int height = channels.Red.Height;
            byte[] image = new byte[width * height * 4];
            for (int i = 0; i < width * height; i++)
            {
                image[i * 4] = channels.Red.Buffer[i];
                image[i * 4 + 1] = channels.Green.Buffer[i];
                image[i * 4 + 2] = channels.Blue.Buffer[i];
                image[i * 4 + 3] = channels.Alpha.Buffer[i];
            }
            return image;
        }

        private int ToIndex(int widthPosition, int heigthPosition, int width)
        {
            return widthPosition + heigthPosition * width;
        }

        public ImageChannel Extrapolation(int px)
        {
            int width = Width;
            int height = Height;
            ImageChannel extrapolatedImage = new ImageChannel(width + (2 * px), height + (2 * px));

            for (int h = 0; h < height + (px * 2); h++)
            {
                for (int w = 0; w < width + (px * 2); w++)
                {
                    int index = w + h * (width + (px * 2));
                    if (w < px)
                    {
                        // Left outer area
                        if (h < px)
                        {
                            // Left upper area
                            extrapolatedImage.Buffer[index] = Buffer[ToIndex(0, 0, width)];
                        }
                        else if (px + height <= h)
                        {
                            // Left lower area
                            extrapolatedImage.Buffer[index] = Buffer[ToIndex(0, height - 1, width)];
                        }
                        else
                        {
                            // Left outer area
                            extrapolatedImage.Buffer[index] = Buffer[ToIndex(0, h - px, width)];
                        }
                    }
                    else if (px + width <= w)
                    {
                        // Right outer area
                        if (h < px)
                        {
                            // Right upper area
                            extrapolatedImage.Buffer[index] = Buffer[ToIndex(width - 1, 0, width)];
                        }
                        else if (px + height <= h)
                        {
                            // Right lower area
                            extrapolatedImage.Buffer[index] = Buffer[ToIndex(width - 1, height - 1, width)];
                        }
                        else
                        {
                            // Right outer area
                            extrapolatedImage.Buffer[index] = Buffer[ToIndex(width - 1, h - px, width)];
                        }
                    }
                    else if (h < px)
                    {
                        // Upper outer area
                        extrapolatedImage.Buffer[index] = Buffer[ToIndex(w - px, 0, width)];
                    }
                    else if (px + height <= h)
                    {
                        // Lower outer area
                        extrapolatedImage.Buffer[index] = Buffer[ToIndex(w - px, height - 1, width)];
                    }
                    else
                    {
                        // Inner area
                        extrapolatedImage.Buffer[index] = Buffer[ToIndex(w - px, h - px, width)];
                    }
                }
            }
            return extrapolatedImage;
        }

        public ImageChannel Resize(double scale)
        {
            int width = Width;
            int height = Height;
            int scaledWidth = (int)Math.Round(width * scale, MidpointRounding.AwayFromZero);
            int scaledHeight = (int)Math.Round(height * scale, MidpointRounding.AwayFromZero);
            ImageChannel scaledImage = new ImageChannel(scaledWidth, scaledHeight);
            for (int w = 0; w < scaledWidth; w++)
            {
                for (int h = 0; h < scaledHeight; h++)
                {
                    int scaledIndex = w + (h * scaledWidth);
                    int originalWidth = (int) Math.Round((w + 1) / scale, MidpointRounding.AwayFromZero) - 1;
                    int originalHeight = (int) Math.Round((h + 1) / scale, MidpointRounding.AwayFromZero) - 1;
                    int originalImageIndex = originalWidth + (originalHeight * width);
                    scaledImage.Buffer[scaledIndex] = Buffer[originalImageIndex];
                }
            }
            return scaledImage;
        }
    }
}
