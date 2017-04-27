using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageSuperResolution.Common
{
    public static class MqUtils
    {
        public const string ImageForUpscallingQueue = "image_input";

        public const string UpscallingProgressQueue = "image_output";
    }
}
