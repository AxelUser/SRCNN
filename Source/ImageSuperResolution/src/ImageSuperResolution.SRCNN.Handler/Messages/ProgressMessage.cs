using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageSuperResolution.SRCNN.Handler.Messages.Helpers;

namespace ImageSuperResolution.SRCNN.Handler.Messages
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
