using System;
using System.Drawing;
using System.IO;
using ImageSuperResolution.SRCNN.Handler.Messages;
using ImageSuperResolution.SRCNN.Handler.Upscalling;

namespace ImageSuperResolution.SRCNN.Handler.Services
{
    public class QueueHandler: UpscallingServiceBase
    {
        public QueueHandler(): base() { }

        public override void Start()
        {
            HardcodeTesting();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        private void HardcodeTesting()
        {
            var originalImage = new Bitmap(Image.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "test.bmp")));
            var rgba = ImageUtils.GetRgbaFromBitmap(originalImage);
            SRCNNHandler srcnn = new SRCNNHandler()
            {
                Scale = 2,
                ScaleModel = Model
            };
            Action<ProgressMessage> progressCallback = Console.WriteLine;
            Action<ResultMessage> doneCallback = (result) =>
            {                
                var newImage = ImageUtils.GetBitmapFromRgba(result.ImageWidth, result.ImageHeight, result.ImageRgba);
                newImage.Save(Path.Combine(Directory.GetCurrentDirectory(), "test_upscaled.bmp"));
                Console.WriteLine(result);
            };
            srcnn.UpscaleImageAsync(rgba, originalImage.Width, originalImage.Height, doneCallback, progressCallback);
            Console.ReadKey();
        }

    }
}
