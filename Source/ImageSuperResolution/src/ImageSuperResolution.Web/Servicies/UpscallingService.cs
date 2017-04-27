using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageSuperResolution.Common.Messages;

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
            throw new NotImplementedException();
        }
    }
}
