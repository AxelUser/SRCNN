using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageSuperResolution.SRCNN.Handler;

namespace ImageSuperResolution.SRCNN.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            QueueHandler handler = new QueueHandler();
            handler.Start(true);
        }
    }
}
