using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ImageSuperResolution.SRCNN.Handler.Messages
{
    public class BlockUpscalling: ProgressMessage
    {
        public int TotalBlocks { get; set; }

        public int BlockNumber { get; set; }

        public BlockUpscalling(int blockNumber, int totalBlocks): base(UpscallingStatuses.UpscallingBlock, "scalling block")
        {
            BlockNumber = blockNumber;
            TotalBlocks = totalBlocks;
        }

        public override string ToString()
        {
            return $"{base.ToString()} (block {BlockNumber} of {TotalBlocks})";
        }
    }
}
