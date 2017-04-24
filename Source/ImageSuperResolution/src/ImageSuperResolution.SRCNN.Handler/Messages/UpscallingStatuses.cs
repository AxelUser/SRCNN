using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ImageSuperResolution.SRCNN.Handler.Messages
{
    public enum UpscallingStatuses
    {
        [Description("Decomposing")]
        Decompose,

        [Description("Upscalling block")]
        UpscallingBlock,

        [Description("Composing")]
        Compose
    }
}
