using System;
using ImageSuperResolution.Common.Messages.QueueEvents;
using System.Threading.Tasks;
using System.Collections.Generic;
using ImageSuperResolution.Common.Messages.ViewModels;

namespace ImageSuperResolution.Web.Servicies
{
    public interface IUpscallingService
    {
        Task<Guid> SendFile(byte[] image);

        IEnumerable<TaskProgress> GetProgress(Guid ticket);

        ResultInfo GetResultInfo(Guid ticket);

        bool ClearEvents(Guid ticket);
    }
}