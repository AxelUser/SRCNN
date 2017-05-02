using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageSuperResolution.Common;
using ImageSuperResolution.Common.Messages;
using Microsoft.VisualBasic.CompilerServices;
using RabbitMQ.Client;

namespace ImageSuperResolution.Web.Servicies
{
    public class UpscallingService : IUpscallingService
    {
        public MqMessage GetProgress(Guid ticket)
        {

            throw new NotImplementedException();
        }

        public Guid SendFile(byte[] image)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var mqConnection = factory.CreateConnection())
            {
                using (var mqChannel = mqConnection.CreateModel())
                {
                    Guid taskId = new Guid();
                    mqChannel.QueueDeclare(queue: MqUtils.ImageForUpscallingQueue,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    var props = mqChannel.CreateBasicProperties();
                    props.CorrelationId = taskId.ToString();
                    
                    mqChannel.BasicPublish(exchange: "",
                        routingKey: MqUtils.ImageForUpscallingQueue,
                        basicProperties: props,
                        body: image);
                    return taskId;
                }
            }
        }
    }
}
