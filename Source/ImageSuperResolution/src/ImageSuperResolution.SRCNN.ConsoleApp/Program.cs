using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageSuperResolution.SRCNN.Handler;
using ImageSuperResolution.SRCNN.Handler.Services;

namespace ImageSuperResolution.SRCNN.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //FileStubHandler handler = new FileStubHandler(false);
            QueueHandler handler = new QueueHandler();
            handler.Start();
            Console.ReadKey();
            handler.Stop();
        }
    }
}
