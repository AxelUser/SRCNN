using System;
using ImageSuperResolution.Common.Messages;

namespace ImageSuperResolution.Web.Servicies
{
    public interface IUpscallingService
    {
        Guid SendFile(byte[] image);

        MqMessage GetProgress(Guid ticket);
    }
}