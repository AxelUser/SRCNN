using System;

namespace ImageSuperResolution.Common.Messages
{
    public class MqMessage
    {
        public Guid TaskId { get; set; }

        public string Message { get; set; }

        public byte[] Content { get; set; }        
    }
}
