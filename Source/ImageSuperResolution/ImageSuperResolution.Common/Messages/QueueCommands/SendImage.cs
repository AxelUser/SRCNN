using System;
using System.Collections.Generic;
using System.Text;

namespace ImageSuperResolution.Common.Messages.QueueCommands
{
    public class SendImage: QueueMessageBase
    {
        public byte[] Image { get; set; }
    }
}
