using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wimm.Model.Control;

namespace Wimm.Logging
{
    internal class CrashReport
    {
        public static void Log(string message,Exception e)
        {
            var logDir = LogDirectory.GetLogDirectory()?.CreateSubdirectory("crash-report");
            if (logDir is null) return;
            var date = DateTime.Now;
            var file = new FileInfo(logDir.FullName + $"/crash-report_{date.Year}-{date.Month}-{date.Day}_{date.Hour}-{date.Minute}-{date.Second}.log");
            using (var stream = new StreamWriter(file.OpenWrite(),Encoding.UTF8))
            {
                stream.Write("[Wimm Crash Report]\n\n");
                stream.Write(message);
                stream.Write("\n\n[Exception And Stack Trace]\n\n");
                stream.Write($"{e.GetType().FullName} : {e.Message}\n");
                stream.Write(e.StackTrace);
                stream.Write("\n\n");
                Exception? innerException = e;
                while ((innerException = innerException?.InnerException) is not null)
                {
                    stream.Write("-- Caused by --\n");
                    stream.Write($"{e.GetType().FullName}\n : {e.Message}\n");
                    stream.Write(e.StackTrace);
                    stream.Write("\n\n");
                }
                stream.WriteLine("[Environment]");
                stream.WriteLine($"OS : {Environment.OSVersion.VersionString}");
                stream.WriteLine($"Runtime : {RuntimeInformation.FrameworkDescription}");
                stream.WriteLine($"Process Architecture : {RuntimeInformation.ProcessArchitecture}");
                stream.WriteLine($"Date : {DateTime.Now}");

                stream.WriteLine("\n-** I wish it run correctly in next time **-");
            }
        }
    }
}
