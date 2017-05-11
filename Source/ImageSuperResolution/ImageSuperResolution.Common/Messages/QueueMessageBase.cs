using System;
using System.Collections.Generic;
using System.Text;

namespace ImageSuperResolution.Common.Messages
{
    public abstract class QueueMessageBase : ITaskMessage
    {
        public Guid MessageId { get; set; }

        public Guid TaskId { get; set; }

        public QueueMessageBase()
        {
            MessageId = Guid.NewGuid();
        }

    }
}
