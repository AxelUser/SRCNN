using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImageSuperResolution.SRCNN.Handler.Services
{
    public class ImageWatcherService
    {

        public string DirectoryWithImages { get; private set; }

        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<string> OnNewFileFound;

        public ImageWatcherService(string directoryWithImages)
        {
            DirectoryWithImages = directoryWithImages;
        }

        public void StartWatching()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Watching(_cancellationTokenSource.Token);
        }

        public void StopWatching()
        {
            _cancellationTokenSource.Cancel();
        }


        private void Watching(CancellationToken token)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    string[] imagePaths = Directory.GetFiles(DirectoryWithImages);                        
                    if (imagePaths.Any())
                    {
                        string imagePath = imagePaths.First(s => s.EndsWith(".bmp") || s.EndsWith(".png") || s.EndsWith(".jpg"));
                        OnNewFileFound?.Invoke(this, imagePath);
                    }                    
                }
            }, token);
        }
    }
}
