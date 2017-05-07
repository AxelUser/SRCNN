using System;
using System.Collections.Generic;
using System.Text;

namespace ImageSuperResolution.Common.Messages
{
    public interface ITaskMessage
    {
        Guid TaskId { get; set; }
    }
}
