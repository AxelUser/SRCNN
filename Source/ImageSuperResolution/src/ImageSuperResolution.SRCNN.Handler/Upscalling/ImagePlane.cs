using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageSuperResolution.SRCNN.Handler.Upscalling
{
    public class ImagePlane
    {
        //Settings
        private const int BlockSize = 128;

        private const int Overlap = 14;

        //Props
        public int Width { get; set; }

        public int Height { get; set; }

        public double[] Buffer { get; private set; }

        public ImagePlane(int width, int height, double[] buffer = null)
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
                Buffer = new double[width * height];
            }
        }

        public long GetIndex(int widthPosition, int heightPosition)
        {
            return widthPosition + heightPosition * Width;
        }

        public double GetValue(int widthPosition, int heightPosition)
        {
            return Buffer[GetIndex(widthPosition, heightPosition)];
        }

        public void SetValue(int widthPosition, int heightPosition, double value)
        {
            Buffer[GetIndex(widthPosition, heightPosition)] = value;
        }

        public static ImageBlocks Blocking(ImagePlane[] initialPlanes)
        {
            int widthInput = initialPlanes[0].Width;
            int heightInput = initialPlanes[0].Height;

            int blocksWidth = (int)Math.Ceiling((double)(widthInput - Overlap) / (BlockSize - Overlap));
            int blocksHeight = (int)Math.Ceiling((double)(heightInput - Overlap) / (BlockSize - Overlap));
            int blocks = blocksWidth * blocksHeight;

            ImageBlocks inputBlocks = new ImageBlocks() // [ [ block0_R, block0_G, block0_B ], [ block1_R, ...] ... ]
            {
                BlocksWidth = blocksWidth,
                BlocksHeight = blocksHeight
            };

            for (int b = 0; b < blocks; b++)
            {
                int blockWidthIndex = b % blocksWidth;
                int blockHeightIndex = (int)Math.Floor((double)(b / blocksWidth));

                int blockWidth;
                int blockHeight;
                if (blockWidthIndex == blocksWidth - 1)
                {
                    blockWidth = widthInput - ((BlockSize - Overlap) * blockWidthIndex); // right end block
                }
                else
                {
                    blockWidth = BlockSize;
                }
                if (blockHeightIndex == blocksHeight - 1)
                {
                    blockHeight = heightInput - ((BlockSize - Overlap) * blockHeightIndex); // bottom end block
                }
                else
                {
                    blockHeight = BlockSize;
                }

                ImagePlane[] channels = new ImagePlane[initialPlanes.Length];
                for (int i = 0; i < channels.Length; i++)
                {
                    channels[i] = new ImagePlane(blockWidth, blockHeight);
                }

                for (int w = 0; w < blockWidth; w++)
                {
                    for (int h = 0; h < blockHeight; h++)
                    {
                        for (int n = 0; n < initialPlanes.Length; n++)
                        {
                            int targetIndexW = blockWidthIndex * (BlockSize - Overlap) + w;
                            int targetIndexH = blockHeightIndex * (BlockSize - Overlap) + h;
                            ImagePlane channel = initialPlanes[n];
                            double channelValue = channel.GetValue(targetIndexW, targetIndexH);
                            channels[n].SetValue(w, h, channelValue);
                        }
                    }
                }
                inputBlocks.Blocks.Add(channels);
            }
            return inputBlocks;
        }

        public static ImagePlane[] Deblocking(ImagePlane[][] outputBlocks, int blocksWidth, int blocksHeight)
        {
            int blockSize = outputBlocks[0][0].Width;
            int width = 0;
            for (int b = 0; b < blocksWidth; b++)
            {
                width += outputBlocks[b][0].Width;
            }
            int height = 0;
            for (int b = 0; b < blocksWidth * blocksHeight; b += blocksWidth)
            {
                height += outputBlocks[b][0].Height;
            }
            //console.log("result image width:" + width + " height:" + height);
            List<ImagePlane> outputPlanes = new List<ImagePlane>(); // [ planeR, planeG, planeB ]
            for (int b = 0; b < outputBlocks.Length; b++)
            {
                ImagePlane[] block = outputBlocks[b];
                int blockWidthIndex = b % blocksWidth;
                int blockHeightIndex = (int)Math.Floor((double)b / blocksWidth);

                for (int n = 0; n < block.Length; n++)
                {
                    if (outputPlanes.Count < n + 1)
                    {
                        outputPlanes.Add(new ImagePlane(width, height));
                    }
                    else if (outputPlanes[n] == null)
                    {
                        outputPlanes[n] = new ImagePlane(width, height);
                    }
                    ImagePlane channelBlock = block[n];
                    for (int w = 0; w < channelBlock.Width; w++)
                    {
                        for (int h = 0; h < channelBlock.Height; h++)
                        {
                            int targetIndexW = blockWidthIndex * blockSize + w;
                            int targetIndexH = blockHeightIndex * blockSize + h;
                            int targetIndex = targetIndexH * width + targetIndexW;
                            double channelValue = channelBlock.GetValue(w, h);
                            outputPlanes[n].Buffer[targetIndex] = channelValue;
                        }
                    }
                }
            }
            return outputPlanes.ToArray();
        }
    }
}
