using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageSuperResolution.SRCNN.Handler.Messages;
using ImageSuperResolution.SRCNN.Handler.Upscalling;

namespace ImageSuperResolution.SRCNN.Handler
{
    public class QueueHandler
    {
        private readonly SRCNNModelLayer[] _model;

        public QueueHandler()
        {
            _model = LoadModel();
        }

        public void Start(bool isTestingPhase = false)
        {
            if (isTestingPhase)
            {
                HardcodeTesting();
            }
        }

        private SRCNNModelLayer[] LoadModel()
        {
            var jsonModel = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "model.json"));
            return SRCNNModelLayer.ReadModel(jsonModel);
        }

        private void HardcodeTesting()
        {
            var originalImage = new Bitmap(Image.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "test.bmp")));
            var rgba = ImageUtils.GetRgbaFromBitmap(originalImage);
            SRCNNHandler srcnn = new SRCNNHandler()
            {
                Scale = 2,
                ScaleModel = _model
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
