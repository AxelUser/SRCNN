using System;

namespace ImageSuperResolution.Common.Messages
{
    public class BlockUpscalling: ProgressMessage
    {
        public int TotalBlocks { get; set; }

        public int BlockNumber { get; set; }

        public BlockUpscalling(Guid taskId, int blockNumber, int totalBlocks): base(taskId, UpscallingStatuses.UpscallingBlock, "scalling block")
        {
            BlockNumber = blockNumber;
            TotalBlocks = totalBlocks;
        }

        public override string ToString()
        {
            return $"{base.ToString()} (block group #{BlockNumber} of total {TotalBlocks})";
        }
    }
}
