using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageSuperResolution.SRCNN.Handler.Messages;

namespace ImageSuperResolution.SRCNN.Handler.Upscalling
{
    public class SRCNNHandler
    {
        public SRCNNModelLayer[] ScaleModel { get; set; }
        public int Scale { get; set; } = 2;

        private ImagePlane Normalize(ImageChannel image)
        {
            int width = image.Width;
            int height = image.Height;
            ImagePlane imagePlane = new ImagePlane(width, height);
            if (imagePlane.Buffer.Length != image.Buffer.Length)
            {
                throw new Exception("Assertion error: length");
            }
            for (int i = 0; i < image.Buffer.Length; i++)
            {
                double temp = image.Buffer[i] / 255.0;
                imagePlane.Buffer[i] = temp;
            }
            return imagePlane;
        }

        private ImageChannel Denormalize(ImagePlane imagePlane)
        {
            ImageChannel image = new ImageChannel(imagePlane.Width, imagePlane.Height);

            for (int i = 0; i < imagePlane.Buffer.Length; i++)
            {
                double temp = Math.Round(imagePlane.Buffer[i] * 255, MidpointRounding.AwayFromZero);
                image.Buffer[i] = temp > 255? (byte)255: (byte)temp;
            }
            return image;
        }

        private ImagePlane[] Convolution(ImagePlane[] inputPlanes, double[] W, int nOutputPlane, double[] bias)
        {
            int width = inputPlanes[0].Width;
            int height = inputPlanes[0].Height;
            ImagePlane[] outputPlanes = new ImagePlane[nOutputPlane];
            for (int o = 0; o < nOutputPlane; o++)
            {
                outputPlanes[o] = new ImagePlane(width - 2, height - 2);
            }
            double[] sumValues = new double[nOutputPlane];
            double[] biasValues = new double[nOutputPlane];
            bias.CopyTo(biasValues, 0);
            for (int w = 1; w < width - 1; w++)
            {
                for (int h = 1; h < height - 1; h++)
                {
                    biasValues.CopyTo(sumValues, 0); // leaky ReLU bias
                    for (int i = 0; i < inputPlanes.Length; i++)
                    {
                        double i00 = inputPlanes[i].GetValue(w - 1, h - 1);
                        double i10 = inputPlanes[i].GetValue(w, h - 1);
                        double i20 = inputPlanes[i].GetValue(w + 1, h - 1);
                        double i01 = inputPlanes[i].GetValue(w - 1, h);
                        double i11 = inputPlanes[i].GetValue(w, h);
                        double i21 = inputPlanes[i].GetValue(w + 1, h);
                        double i02 = inputPlanes[i].GetValue(w - 1, h + 1);
                        double i12 = inputPlanes[i].GetValue(w, h + 1);
                        double i22 = inputPlanes[i].GetValue(w + 1, h + 1);

                        for (int o = 0; o < nOutputPlane; o++)
                        {
                            int weightIndex = (o * inputPlanes.Length * 9) + (i * 9);
                            double value = sumValues[o];
                            value += i00 * W[weightIndex++];
                            value += i10 * W[weightIndex++];
                            value += i20 * W[weightIndex++];
                            value += i01 * W[weightIndex++];
                            value += i11 * W[weightIndex++];
                            value += i21 * W[weightIndex++];
                            value += i02 * W[weightIndex++];
                            value += i12 * W[weightIndex++];
                            value += i22 * W[weightIndex++];
                            sumValues[o] = value;
                        }
                    }
                    for (int o = 0; o < nOutputPlane; o++)
                    {
                        double v = sumValues[o];
                        if (v < 0)
                        {
                            v *= 0.1;
                        }
                        outputPlanes[o].SetValue(w - 1, h - 1, v);
                    }
                }
            }
            return outputPlanes;
        }

        private double[][] GetWeights(SRCNNModelLayer[] model)
        {
            LinkedList<double[]> arrayW = new LinkedList<double[]>();
            foreach (SRCNNModelLayer layer in model)
            {
                arrayW.AddLast(layer.GetAllWeights());
            }
            return arrayW.ToArray();
        }

        private ImagePlane[][] UpscaleBlocks(SRCNNModelLayer[] model, double[][] weights, Action<ProgressMessage> progressCallback, int blockGroup, int totalBlockGroups, params  ImagePlane[][] blocks)
        {
            List<ImagePlane[]> outputBlocks = new List<ImagePlane[]>();
            for (int b = 0; b < blocks.Length; b++)
            {
                ImagePlane[] inputBlock = blocks[b];
                ImagePlane[] outputBlock = null;
                for (int l = 0; l < model.Length; l++)
                {
                    int nOutputPlane = model[l].OutputPlanesCount;

                    // convolution
                    outputBlock = Convolution(inputBlock, weights[l], nOutputPlane, model[l].Bias);
                    inputBlock = outputBlock; // propagate output plane to next layer input
                    //imageBlocks.Blocks[b] = null;
                }
                outputBlocks.Add(outputBlock);
                int doneRatio = (int)Math.Round((double)(100 * (b + 1)) / blocks.Length, MidpointRounding.AwayFromZero);
                progressCallback(new BlockUpscalling(blockGroup, totalBlockGroups)
                {
                    Percent = doneRatio
                });
            }
            //imageBlocks.Blocks = null;
            return outputBlocks.ToArray();
        }

        private ImageChannels UpscaleRgbAsync(ImageChannels channels, SRCNNModelLayer[] model, int scale, Action<ProgressMessage> progressCallback, string phase)
        {
            ImagePlane[] inputPlanes = channels.ToArray().Select((image) =>
            {
                ImageChannel imgResized = scale == 1 ? image : image.Resize(scale);

                // extrapolation for layer count (each convolution removes outer 1 pixel border)
                ImageChannel imgExtra = imgResized.Extrapolation(model.Length);

                return Normalize(imgExtra);
            }).ToArray();

            // blocking
            ImageBlocks imageBlocks = ImagePlane.Blocking(inputPlanes);
            inputPlanes = null;

            // init W
            double[][] weights = GetWeights(model);

            var outputBlocks = imageBlocks.Blocks.AsParallel()
                .SelectMany((block, index) => UpscaleBlocks(model, weights, progressCallback,
                    index + 1, imageBlocks.Blocks.Count,
                    block))
                .ToList();

            
            // de-blocking
            ImagePlane[] outputPlanes = ImagePlane.Deblocking(outputBlocks.ToArray(), imageBlocks.BlocksWidth, imageBlocks.BlocksHeight);
            if (outputPlanes.Length != 3)
            {
                throw new Exception("Output planes must be 3: color channel R, G, B.");
            }

            ImageChannels outputChannels = new ImageChannels()
            {
                Red = Denormalize(outputPlanes[0]),
                Green = Denormalize(outputPlanes[1]),
                Blue = Denormalize(outputPlanes[2]),
                Alpha = channels.Alpha
            };

            return outputChannels;
        }
        public async Task UpscaleImage(byte[] image, int width, int height, Action<ResultMessage> done, Action<ProgressMessage> progress)
        {
            TimeSpan timeSpanStart = TimeSpan.FromTicks(DateTime.Now.Ticks);
            // decompose
            progress(new ProgressMessage(UpscallingStatuses.Decompose, "in process"));
            ImageChannels channels = ImageChannel.ChannelDecompose(image, width, height);
            progress(new ProgressMessage(UpscallingStatuses.Decompose, "ready"));

            // calculate
            //Scaling all blocks
            ImageChannels upscaledChannels = await Task.Run(() => UpscaleRgbAsync(channels, ScaleModel, Scale, progress, "scale"));
            //Scaled all blocks

            // resize alpha channel
            ImageChannel imageA = Scale == 1 ? upscaledChannels.Alpha : upscaledChannels.Alpha.Resize(Scale);

            if (imageA.Buffer.Length != upscaledChannels.Red.Buffer.Length)
            {
                throw new Exception("A channel image size must be same with R channel image size");
            }

            upscaledChannels.Alpha = imageA;

            // recompose
            progress(new ProgressMessage(UpscallingStatuses.Compose, "in process"));
            byte[] upscaledImage = ImageChannel.ChannelCompose(upscaledChannels);
            progress(new ProgressMessage(UpscallingStatuses.Compose, "ready"));

            TimeSpan elapsedTime = TimeSpan.FromTicks(DateTime.Now.Ticks) - timeSpanStart;

            done(new ResultMessage(upscaledImage, upscaledChannels.Red.Width, upscaledChannels.Red.Height, elapsedTime));
        }

        public async void UpscaleImageAsync(byte[] image, int width, int height, Action<ResultMessage> done, Action<ProgressMessage> progress)
        {
            await UpscaleImage(image, width, height, done, progress);
        }
    }
}
