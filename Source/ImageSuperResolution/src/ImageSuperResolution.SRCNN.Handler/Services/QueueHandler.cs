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
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ImageSuperResolution.SRCNN.Handler.Services
{
    public class QueueHandler: UpscallingServiceBase
    {
        private readonly string _queueInput;
        private readonly string _queueOutput;

        private string _imageConsumerTag;
        private IConnection _connectionToMq;
        private IModel _inputChannel;
        private IModel _outputChannel;

        private readonly List<Task> _imagePrecessingTasks;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public QueueHandler() : base()
        {
            _queueInput = MqUtils.ImageForUpscallingQueue;
            _queueOutput = MqUtils.UpscallingProgressQueue;
            _imagePrecessingTasks = new List<Task>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public override void Start()
        {
            ConnectToMq();
        }

        public override void Stop()
        {
            _inputChannel.BasicCancel(_imageConsumerTag);
            _connectionToMq.Close();
        }

        private void ConnectToMq()
        {
            
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };

            _connectionToMq = factory.CreateConnection();
            _inputChannel = _connectionToMq.CreateModel();
            _outputChannel = _connectionToMq.CreateModel();

            InitConsumer(_inputChannel);
        }        

        private void InitConsumer(IModel inputChannel)
        {
            inputChannel.QueueDeclare(
                queue: _queueInput,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            inputChannel.BasicQos(0, 1, false);

            var imageConsumer = new EventingBasicConsumer(inputChannel);

            imageConsumer.Received += MessageReceived;

            _imageConsumerTag = inputChannel.BasicConsume(_queueInput, false, imageConsumer);
        }

        private void PublishMessage(IModel outputChannel, Guid taskId, MqMessage message)
        {
            var props = outputChannel.CreateBasicProperties();
            props.CorrelationId = taskId.ToString();

            message.TaskId = taskId;

            var converter = new BinaryConverter();
            byte[] messageBody = converter.Serialize(message);

            outputChannel.BasicPublish(exchange: "", routingKey: _queueOutput, 
                basicProperties: props, body: messageBody);
        }

        private void MessageReceived(object sender, BasicDeliverEventArgs e)
        {
            byte[] imageData = e.Body;
            Guid taskId;
            bool hasTaskId = Guid.TryParse(e.BasicProperties.CorrelationId, out taskId);

            bool isValid = imageData != null && hasTaskId;

            if (isValid && !_cancellationTokenSource.IsCancellationRequested)
            {
                using (var originalImage = new Bitmap(ImageUtils.GetImageFromByteArray(imageData)))
                {
                    byte[] rgba = ImageUtils.GetRgbaFromBitmap(originalImage);
                    SRCNNHandler srcnn = new SRCNNHandler()
                    {
                        Scale = 2,
                        ScaleModel = Model
                    };

                    int width = originalImage.Width;
                    int height = originalImage.Height;



                    Task upscallingTask = Task.Run(
                        () => srcnn.UpscaleImageAsync(taskId, rgba, width, height, ResultHandling,
                            ProgressLogging), _cancellationTokenSource.Token);
                    _imagePrecessingTasks.RemoveAll(t => t.IsCompleted);
                    _imagePrecessingTasks.Add(upscallingTask);
                }
            }

        }

        private void ResultHandling(ResultMessage result)
        {
            var newImage = ImageUtils.GetBitmapFromRgba(result.ImageWidth, result.ImageHeight, result.ImageRgba);

            MqMessage message = new MqMessage()
            {
                Message = result.ToString(),
                Content = ImageUtils.SaveImageToBytesPng(newImage)
            };

            PublishMessage(_outputChannel, result.TaskId, message);

        }

        private void ProgressLogging(ProgressMessage progressMessage)
        {
            MqMessage message = new MqMessage()
            {
                Message = progressMessage.ToString()
            };

            PublishMessage(_outputChannel, progressMessage.TaskId, message);
        }
    }
}
