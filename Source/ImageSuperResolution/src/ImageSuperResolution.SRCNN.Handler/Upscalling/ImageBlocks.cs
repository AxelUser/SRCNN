using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageSuperResolution.SRCNN.Handler.Upscalling
{
    public class ImageBlocks
    {
        public List<ImagePlane[]> Blocks { get; set; }

        public int BlocksWidth { get; set; }

        public int BlocksHeight { get; set; }

        public ImageBlocks()
        {
            Blocks = new List<ImagePlane[]>();
        }
    }
}
