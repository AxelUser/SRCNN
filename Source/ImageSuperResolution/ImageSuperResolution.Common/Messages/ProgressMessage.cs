using System;
using ImageSuperResolution.Common.Messages.Helpers;

namespace ImageSuperResolution.Common.Messages
{
    public class ProgressMessage
    {
        public Guid TaskId { get; set; }

        public int? Percent { get; set; } = null;

        public string Message { get; set; }

        public UpscallingStatuses Phase { get; set; }

        public ProgressMessage(Guid taskId, UpscallingStatuses phase, string message)
        {
            TaskId = taskId;
            Phase = phase;
            Message = message;
        }

        public ProgressMessage()
        {

        }

        public override string ToString()
        {
            if (Percent != null)
            {
                return $"{Phase.Description()} ({Percent}%): {Message}";
            }
            return $"{Phase.Description()}: {Message}";
        }
    }
}
