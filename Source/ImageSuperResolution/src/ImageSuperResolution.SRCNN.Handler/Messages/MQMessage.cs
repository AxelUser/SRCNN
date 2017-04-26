using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageSuperResolution.SRCNN.Handler.Messages
{
    public class MqMessage
    {
        public Guid TaskId { get; set; }

        public string Message { get; set; }

        public byte[] Content { get; set; }        
    }
}
