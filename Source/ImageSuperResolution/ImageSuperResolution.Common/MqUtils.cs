using ImageSuperResolution.Common.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSuperResolution.Common
{
    public static class MqUtils
    {
        public const string ImageForUpscallingQueue = "image_input";

        public const string UpscallingPregressQueue = "image_progress";

        public const string UpscallingResultQueue = "image_output";

        public static byte[] SerializeMessage(ITaskMessage message)
        {
            string json = null;
            if (message is ProgressMessage)
            {
                var progressMessage = message as ProgressMessage;
                json = JsonConvert.SerializeObject(progressMessage);
            } else if(message is BlockUpscalling)
            {
                var upscallingMessage = message as BlockUpscalling;
                json = JsonConvert.SerializeObject(upscallingMessage);
            }
            else if (message is ResultMessage)
            {
                var resultMessage = message as ResultMessage;
                json = JsonConvert.SerializeObject(resultMessage);
            }

            return Encoding.UTF8.GetBytes(json);
        }

        //public static ITaskMessage DeserializeMessage(byte[] body)
        //{
        //    string json = Encoding.UTF8.GetString(body);

        //    if (message is ProgressMessage)
        //    {
        //        var progressMessage = message as ProgressMessage;
        //        json = JsonConvert.SerializeObject(progressMessage);
        //    }
        //    else if (message is BlockUpscalling)
        //    {
        //        var upscallingMessage = message as BlockUpscalling;
        //        json = JsonConvert.SerializeObject(upscallingMessage);
        //    }
        //    else if (message is ResultMessage)
        //    {
        //        var resultMessage = message as ResultMessage;
        //        json = JsonConvert.SerializeObject(resultMessage);
        //    }

        //    return Encoding.UTF8.GetBytes(json);
        //}


    }
}
