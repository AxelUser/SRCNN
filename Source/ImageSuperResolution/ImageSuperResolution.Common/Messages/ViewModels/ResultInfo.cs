using System;
using System.Collections.Generic;
using System.Text;

namespace ImageSuperResolution.Common.Messages.ViewModels
{
    public class ResultInfo
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public TimeSpan ElapsedTime { get; set; }

        public string FilePath { get; set; }
    }
}
