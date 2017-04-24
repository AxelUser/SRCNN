using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageSuperResolution.SRCNN.Handler.Upscalling
{
    public class ImageChannels
    {
        public ImageChannel Alpha { get; set; }
        public ImageChannel Red { get; set; }
        public ImageChannel Green { get; set; }
        public ImageChannel Blue { get; set; }

        public ImageChannel[] ToArray(bool ignoreAlpha = true)
        {
            if (ignoreAlpha)
            {
                return new ImageChannel[] { Red, Green, Blue };
            }
            else
            {
                return new ImageChannel[] { Red, Green, Blue, Alpha};
            }
                
        }
    }
}
