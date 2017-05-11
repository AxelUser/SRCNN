using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BinaryFormatter;
using ImageSuperResolution.Common;
using ImageSuperResolution.Common.Messages;
using ImageSuperResolution.SRCNN.Handler.Upscalling;
using EasyNetQ;
using ImageSuperResolution.Common.Messages.QueueCommands;
using ImageSuperResolution.Common.Messages.QueueEvents;

namespace ImageSuperResolution.SRCNN.Handler.Services
{
    public class QueueHandler: UpscallingServiceBase
    {
        private readonly string _queueInput;
        private readonly string _queueOutput;

        private readonly List<Task> _imagePrecessingTasks;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private IBus _mqBus;
        private IDisposable _mqReceiver;

        public QueueHandler() : base()
        {
            _queueInput = MqUtils.ImageForUpscallingQueue;
            _queueOutput = MqUtils.UpscallingResultQueue;
            _imagePrecessingTasks = new List<Task>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public override void Start()
        {
            Console.WriteLine("Queue service started");
            if (_mqBus == null || !_mqBus.IsConnected)
            {
                _mqReceiver?.Dispose();
                _mqBus = RabbitHutch.CreateBus("host=localhost");
            }

            _mqReceiver = _mqBus.Receive<SendImage>(MqUtils.ImageForUpscallingQueue, ProceedImage);
        }

        public override void Stop()
        {
            Console.WriteLine("Cancel requested.");
            _mqReceiver.Dispose();
            _cancellationTokenSource.Cancel();
            Task.WaitAll(_imagePrecessingTasks.ToArray());
            _mqBus.Dispose();
            Console.WriteLine("Queue service stoped");
            Console.ReadKey();
        }

        private Task ProceedImage(SendImage message)
        {
            return Task.Run(() =>
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    using(var ms = new MemoryStream(message.Image))
                    {
                        using (var originalImage = new Bitmap(ms))
                        {
                            var rgba = ImageUtils.GetRgbaFromBitmap(originalImage);
                            SRCNNHandler srcnn = new SRCNNHandler()
                            {
                                Scale = 2,
                                ScaleModel = Model
                            };

                            int width = originalImage.Width;
                            int height = originalImage.Height;

                            ProgressLogging(new ProgressMessage(message.TaskId, UpscallingStatuses.Received));

                            Task upscallingTask = Task.Run(
                                async () =>
                                {
                                    await srcnn.UpscaleImageAsync(message.TaskId, rgba, width, height, ResultHandling,
                                        ProgressLogging);
                                }, _cancellationTokenSource.Token);

                            _imagePrecessingTasks.RemoveAll(t => t.IsCompleted);
                            _imagePrecessingTasks.Add(upscallingTask);
                        }
                    }
                }
            });
        }

        private void ResultHandling(ResultMessage result)
        {
            Bitmap resultBitmap = ImageUtils.GetBitmapFromRgba(result.ImageWidth, result.ImageHeight, result.ImageRgba);
            var resultMqEvent = new TaskFinished()
            {
                TaskId = result.TaskId,
                Image = ImageUtils.SerializeImage(resultBitmap),
                Height = result.ImageHeight,
                Width = result.ImageWidth,
                ElapsedTime = result.ElapsedTime
            };

            _mqBus.Send(MqUtils.UpscallingResultQueue, resultMqEvent);
            Console.WriteLine(result);
        }

        private void ProgressLogging(ProgressMessage progressMessage)
        {
            var mqProgressEvent = new TaskProgress()
            {
                TaskId = progressMessage.TaskId,
                Status = progressMessage.Phase,
                Message = progressMessage.ToString()
            };

            if(progressMessage is BlockUpscalling)
            {
                var upscallingProgressMessage = progressMessage as BlockUpscalling;

                mqProgressEvent.BlockNumber = upscallingProgressMessage.BlockNumber;
                mqProgressEvent.BlocksCount = upscallingProgressMessage.TotalBlocks;
                mqProgressEvent.ProgressRatio = upscallingProgressMessage.Percent;
            }

            _mqBus.Send(MqUtils.UpscallingProgressQueue, mqProgressEvent);
            Console.WriteLine(progressMessage);
        }
    }
}
