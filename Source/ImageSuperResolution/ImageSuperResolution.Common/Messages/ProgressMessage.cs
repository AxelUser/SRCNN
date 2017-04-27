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

        public ProgressMessage(Guid taskId, UpscallingStatuses phase)
        {
            TaskId = taskId;
            Phase = phase;
        }

        public ProgressMessage(Guid taskId, UpscallingStatuses phase, string message)
        {
            TaskId = taskId;
            Phase = phase;
            Message = message;
        }

        public override string ToString()
        {
            string message = $"[{TaskId}] {Phase.Description()}";
            if (Percent != null)
            {
                message += $" ({Percent}%)";
            }
            if (!string.IsNullOrEmpty(Message))
            {
                message += $": {Message}";
            }
            return message;
        }
    }
}
