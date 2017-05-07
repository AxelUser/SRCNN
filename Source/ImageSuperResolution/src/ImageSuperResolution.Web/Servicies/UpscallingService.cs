using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageSuperResolution.Common.Messages.QueueEvents;
using Microsoft.VisualBasic.CompilerServices;
using EasyNetQ;
using ImageSuperResolution.Common.Messages.QueueCommands;
using ImageSuperResolution.Common;

namespace ImageSuperResolution.Web.Servicies
{
    public class UpscallingService : IUpscallingService
    {
        public TaskProgress GetProgress(Guid ticket)
        {

            throw new NotImplementedException();
        }

        public TaskProgress GetResult(Guid ticket)
        {
            throw new NotImplementedException();
        }

        public async Task<Guid> SendFile(byte[] image)
        {
            using (var bus = RabbitHutch.CreateBus("host=localhost"))
            {
                var taskId = Guid.NewGuid();
                await bus.SendAsync(MqUtils.ImageForUpscallingQueue, new SendImage()
                {
                    TaskId = taskId,
                    Image = image
                });
                return taskId;
            }
            //var factory = new ConnectionFactory() { HostName = "localhost" };
            //using (var mqConnection = factory.CreateConnection())
            //{
            //    using (var mqChannel = mqConnection.CreateModel())
            //    {
            //        Guid taskId = new Guid();
            //        mqChannel.QueueDeclare(queue: MqUtils.ImageForUpscallingQueue,
            //            durable: false,
            //            exclusive: false,
            //            autoDelete: false,
            //            arguments: null);

            //        var props = mqChannel.CreateBasicProperties();
            //        props.CorrelationId = taskId.ToString();

            //        mqChannel.BasicPublish(exchange: "",
            //            routingKey: MqUtils.ImageForUpscallingQueue,
            //            basicProperties: props,
            //            body: image);
            //        return taskId;
            //    }
            //}
        }
    }
}
