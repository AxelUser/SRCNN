using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ImageSuperResolution.SRCNN.Handler.Messages;
using ImageSuperResolution.SRCNN.Handler.Upscalling;

namespace ImageSuperResolution.SRCNN.Handler.Services
{
    public class FileStubHandler: UpscallingServiceBase
    {
        private string _directoryInput;
        private string _directoryOutput;

        private ImageWatcherService _watcher;

        private readonly List<Task> _imagePrecessingTasks;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public FileStubHandler() : base()
        {
            InitFolders();
            _imagePrecessingTasks = new List<Task>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public override void Start()
        {
            _watcher = new ImageWatcherService(_directoryInput);
            _watcher.OnNewFileFound += ProceedImage;
            _watcher.StartWatching();
        }

        public override void Stop()
        {
            Console.WriteLine("Cancel requested.");
            _watcher.StopWatching();
            _cancellationTokenSource.Cancel();
            Task.WaitAll(_imagePrecessingTasks.ToArray());                        
        }

        private void ProceedImage(object sender, string pathToImage)
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                using (var originalImage = new Bitmap(pathToImage))
                {
                    var rgba = ImageUtils.GetRgbaFromBitmap(originalImage);
                    SRCNNHandler srcnn = new SRCNNHandler()
                    {
                        Scale = 2,
                        ScaleModel = Model
                    };

                    int width = originalImage.Width;
                    int height = originalImage.Height;
                    
                    Task upscallingTask = Task.Run(
                        () => srcnn.UpscaleImageAsync(rgba, width, height, ResultHandling,
                            ProgressLogging), _cancellationTokenSource.Token);
                    _imagePrecessingTasks.RemoveAll(t => t.IsCompleted);
                    _imagePrecessingTasks.Add(upscallingTask);
                }               
                File.Delete(pathToImage);

            }
        }

        private void ProgressLogging(ProgressMessage message)
        {
            Console.WriteLine(message);
        }

        private void ResultHandling(ResultMessage result)
        {
            var newImage = ImageUtils.GetBitmapFromRgba(result.ImageWidth, result.ImageHeight, result.ImageRgba);
            string filename = $"{DateTime.Now:yy-MM-dd hh-mm-ss}.bmp";
            newImage.Save(Path.Combine(_directoryOutput, filename));
            Console.WriteLine(result);
        }

        private void InitFolders()
        {
            _directoryInput = Path.Combine(Directory.GetCurrentDirectory(), "ImageInputStub");
            _directoryOutput = Path.Combine(Directory.GetCurrentDirectory(), "ImageOutputStub");
            Directory.CreateDirectory(_directoryInput);
            Directory.CreateDirectory(_directoryOutput);
        }
    }
}
