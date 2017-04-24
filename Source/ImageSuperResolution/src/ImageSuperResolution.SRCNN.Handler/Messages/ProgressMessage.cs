using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageSuperResolution.SRCNN.Handler.Messages.Helpers;

namespace ImageSuperResolution.SRCNN.Handler.Messages
{
    public class ProgressMessage
    {
        public int? Percent { get; set; } = null;

        public string Message { get; set; }

        public UpscallingStatuses Phase { get; set; }

        public ProgressMessage(UpscallingStatuses phase, string message)
        {
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
            else
            {
                return $"{Phase.Description()}: {Message}";
            }            
        }
    }
}
