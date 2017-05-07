using System;
using System.Collections.Generic;
using System.Text;

namespace ImageSuperResolution.Common.Messages.QueueCommands
{
    public class SendImage: ITaskMessage
    {
        public Guid TaskId { get; set; }

        public byte[] Image { get; set; }
    }
}
