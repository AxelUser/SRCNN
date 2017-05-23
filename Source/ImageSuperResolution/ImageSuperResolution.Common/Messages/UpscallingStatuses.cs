using System.ComponentModel;

namespace ImageSuperResolution.Common.Messages
{
    public enum UpscallingStatuses
    {
        [Description("Image received")]
        Received = 0,

        [Description("Decomposing")]
        Decompose = 1,

        [Description("Upscalling blocks")]
        UpscallingBlock = 2,

        [Description("Composing")]
        Compose = 3,

        [Description("Upscaled image was sent")]
        SentResult = 4
    }
}
