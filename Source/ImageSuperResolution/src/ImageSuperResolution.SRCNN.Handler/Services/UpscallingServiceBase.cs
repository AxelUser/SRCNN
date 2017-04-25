using System.IO;
using System.Linq;
using System.Reflection;
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
            var assembly = typeof(UpscallingServiceBase).GetTypeInfo().Assembly;
            string[] names = assembly.GetManifestResourceNames();
            Stream modelResource = assembly.GetManifestResourceStream(names.First(name => name.Contains("model.json")));
            string jsonModel;
            using (var sr = new StreamReader(modelResource))
            {
                jsonModel = sr.ReadToEnd();
            }
            return SRCNNModelLayer.ReadModel(jsonModel);
        }
    }
}
