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
    public class UpscallingService : IUpscallingService, IDisposable
    {
        private IBus _mqBus;
        private IDisposable progressReceiver;
        private IDisposable resultReceiver;


        public UpscallingService()
        {
            _mqBus = RabbitHutch.CreateBus("host=localhost");

            _mqBus.Receive<TaskProgress>(MqUtils.UpscallingProgressQueue, OnProgress);

            _mqBus.Receive<TaskFinished>(MqUtils.UpscallingProgressQueue, OnResult);
        }

        private Task OnProgress(TaskProgress progressMessage)
        {
            return Task.Run(() =>
            {

            });
        }

        private Task OnResult(TaskFinished resultMessage)
        {
            return Task.Run(() =>
            {

            });
        }

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
            var taskId = Guid.NewGuid();
            await _mqBus.SendAsync(MqUtils.ImageForUpscallingQueue, new SendImage()
            {
                TaskId = taskId,
                Image = image
            });
            return taskId;
        }

        public void Dispose()
        {
            progressReceiver?.Dispose();
            resultReceiver?.Dispose();
        }
    }
}
