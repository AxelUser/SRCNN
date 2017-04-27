using System.ComponentModel;

namespace ImageSuperResolution.Common.Messages
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
