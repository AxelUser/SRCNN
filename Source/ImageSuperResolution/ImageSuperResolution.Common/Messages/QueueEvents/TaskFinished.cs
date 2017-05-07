using System;
using System.Collections.Generic;
using System.Text;

namespace ImageSuperResolution.Common.Messages.QueueEvents
{
    public class TaskFinished: ITaskMessage
    {
        public Guid TaskId { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public TimeSpan ElapsedTime { get; set; }

        public byte[] Image { get; set; }
    }
}
