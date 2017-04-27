using System.ComponentModel;

namespace ImageSuperResolution.Common.Messages
{
    public enum UpscallingStatuses
    {
        [Description("Image received")]
        Received,

        [Description("Decomposing")]
        Decompose,

        [Description("Upscalling blocks")]
        UpscallingBlock,

        [Description("Composing")]
        Compose,

        [Description("Upscaled image was sent")]
        SentResult
    }
}
