using System.IO;
using ImageSuperResolution.SRCNN.Handler.Upscalling;

namespace ImageSuperResolution.SRCNN.Handler.Services
{
    public abstract class UpscallingServiceBase
    {
        protected readonly SRCNNModelLayer[] Model;

        public UpscallingServiceBase()
        {
            Model = LoadModel();
        }

        public abstract void Start();
        public abstract void Stop();

        private SRCNNModelLayer[] LoadModel()
        {
            var jsonModel = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "model.json"));
            return SRCNNModelLayer.ReadModel(jsonModel);
        }
    }
}
