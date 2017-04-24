using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageSuperResolution.SRCNN.Handler.Upscalling;

namespace ImageSuperResolution.SRCNN.Handler
{
    public class QueueHandler
    {
        public QueueHandler()
        {
           
        }

        public void Start(bool isTestingPhase)
        {
            HardcodeTesting();
        }



        private void HardcodeTesting()
        {
            var originalImage = new Bitmap(Image.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "test.bmp")));
            var jsonModel = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "model.json"));
            var rgba = ImageUtils.GetRgbaFromBitmap(originalImage);
            SRCNNHandler srcnn = new SRCNNHandler()
            {
                Scale = 2,
                ScaleModel = SRCNNModelLayer.ReadModel(jsonModel)
            };
            Action<int, string> progressCallback = (progressIndex, message) => Console.WriteLine($"{progressIndex}: {message}");
            Action<byte[], int, int> doneCallback = (image, width, height) =>
            {                
                var newImage = ImageUtils.GetBitmapFromRgba(width, height, image);
                newImage.Save(Path.Combine(Directory.GetCurrentDirectory(), "test_upscaled.bmp"));
                Console.WriteLine($"Image upscaled to {srcnn.Scale}: Width = {width}, Height = {height}");                
            };
            srcnn.UpscaleImageAsync(rgba, originalImage.Width, originalImage.Height, doneCallback, progressCallback);
            Console.ReadKey();
        }

    }
}
