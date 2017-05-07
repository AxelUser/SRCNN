using System;
using System.Collections.Generic;
using System.Text;

namespace ImageSuperResolution.Common.Messages.QueueEvents
{
    public class TaskProgress: ITaskMessage
    {
        public Guid TaskId { get; set; }

        public UpscallingStatuses Status { get; set; }

        public int BlockNumber { get; set; }

        public int BlocksCount { get; set; }

        public double ProgressRatio { get; set; }

        public string Message { get; set; }

    }
}
