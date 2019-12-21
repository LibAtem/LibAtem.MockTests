using LibAtem.Net;
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;

namespace Logger
{
    class Program
    {
        static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            var log = LogManager.GetLogger(typeof(Program));
            log.Info("Starting");

            var client = new AtemClient("10.42.13.99");
            Console.WriteLine("Hello World!");
        }
    }
}
