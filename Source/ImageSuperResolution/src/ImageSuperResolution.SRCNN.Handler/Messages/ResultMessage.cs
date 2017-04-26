using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageSuperResolution.SRCNN.Handler.Messages
{
    public class ResultMessage
    {
        public Guid TaskId { get; set; }

        public byte[] ImageRgba { get; set; }

        public TimeSpan ElapsedTime { get; set; }

        public int ImageWidth { get; set; }

        public int ImageHeight { get; set; }

        public ResultMessage(Guid taskId, byte[] image, int width, int height, TimeSpan time)
        {
            TaskId = taskId;
            ImageRgba = image;
            ImageWidth = width;
            ImageHeight = height;
            ElapsedTime = time;
        }

        public override string ToString()
        {
            return $"Image ({ImageRgba.Length} bytes) was upscaled to width {ImageWidth}px and height {ImageHeight}px for {ElapsedTime:g}";
        }
    }
}
