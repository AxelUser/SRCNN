using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ImageSuperResolution.Common;
using ImageSuperResolution.Common.Messages;
using ImageSuperResolution.SRCNN.Handler.Upscalling;

namespace ImageSuperResolution.SRCNN.Handler.Services
{
    public class FileStubHandler: UpscallingServiceBase
    {
        private string _directoryInput;
        private string _directoryOutput;
        private readonly bool _imageLoadingParallel;

        private ImageWatcherService _watcher;

        private readonly List<Task> _imagePrecessingTasks;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public FileStubHandler(bool imageLoadingParallel = true) : base()
        {
            InitFolders();
            _imagePrecessingTasks = new List<Task>();
            _cancellationTokenSource = new CancellationTokenSource();
            _imageLoadingParallel = imageLoadingParallel;
        }

        public override void Start()
        {
            _watcher = new ImageWatcherService(_directoryInput);
            _watcher.OnNewFileFound += ProceedImage;
            _watcher.StartWatching();
            Console.WriteLine("File stub service started");
            Console.ReadKey();
            Stop();
        }

        public override void Stop()
        {
            Console.WriteLine("Cancel requested.");
            _watcher.StopWatching();
            _cancellationTokenSource.Cancel();
            Task.WaitAll(_imagePrecessingTasks.ToArray());
            Console.WriteLine("File stub service stoped");
            Console.ReadKey();
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

                    var taskId = Guid.NewGuid();

                    ProgressLogging(new ProgressMessage(taskId, UpscallingStatuses.Received));

                    Task upscallingTask = Task.Run(
                        async () =>
                        {
                            await srcnn.UpscaleImageAsync(taskId, rgba, width, height, ResultHandling,
                                ProgressLogging);
                        }, _cancellationTokenSource.Token);
                    if (_imageLoadingParallel)
                    {
                        _imagePrecessingTasks.RemoveAll(t => t.IsCompleted);
                        _imagePrecessingTasks.Add(upscallingTask);
                    }
                    else
                    {
                        upscallingTask.Wait();
                    }
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
            string filename = $"{result.TaskId:N}_{(int)Math.Round(result.ElapsedTime.TotalSeconds)}sec.png";
            newImage.Save(Path.Combine(_directoryOutput, filename), ImageFormat.Png);
            Console.WriteLine(result);
            ProgressLogging(new ProgressMessage(result.TaskId, UpscallingStatuses.SentResult));
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
