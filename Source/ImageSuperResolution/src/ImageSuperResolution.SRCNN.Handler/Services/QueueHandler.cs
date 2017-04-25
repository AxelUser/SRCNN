using System;
using System.Drawing;
using System.IO;
using ImageSuperResolution.SRCNN.Handler.Messages;
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
        private IModel _channerInput;

        public QueueHandler() : base()
        {
            _queueInput = "image_input";
            _queueOutput = "image_output";
        }

        public override void Start()
        {
            ConnectToMq();
        }

        public override void Stop()
        {
            _channerInput.BasicCancel(_imageConsumerTag);
        }

        private void ConnectToMq()
        {
            
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };

            using (var connection = factory.CreateConnection())
            {
                using (_channerInput = connection.CreateModel())
                {
                    _channerInput.QueueDeclare(
                        queue: _queueInput,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
                    _channerInput.BasicQos(0, 1, false);

                    var imageConsumer = new EventingBasicConsumer(_channerInput);

                    imageConsumer.Received += MessageReceived;

                    _imageConsumerTag = _channerInput.BasicConsume(_queueInput, false, imageConsumer);
                }
            }

        }

        private void MessageReceived(object sender, BasicDeliverEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
