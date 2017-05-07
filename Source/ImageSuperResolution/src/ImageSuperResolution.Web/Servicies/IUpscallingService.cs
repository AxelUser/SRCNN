using System;
using ImageSuperResolution.Common.Messages;
using ImageSuperResolution.Common.Messages.QueueEvents;
using System.Threading.Tasks;

namespace ImageSuperResolution.Web.Servicies
{
    public interface IUpscallingService
    {
        Task<Guid> SendFile(byte[] image);

        TaskProgress GetProgress(Guid ticket);

        TaskProgress GetResult(Guid ticket);
    }
}