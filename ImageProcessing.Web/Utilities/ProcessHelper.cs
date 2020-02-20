using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProcessing.Web.Utilities
{
    public class ProcessHelper
    {
        public static void SearchFile(IConfiguration config, string imagePath)
        {
            var start = new ProcessStartInfo();
            start.FileName = config.GetValue<string>("PythonExePath");
            start.Arguments = string.Format("{0} {1}", config.GetValue<string>("PythonScriptPath"), imagePath);
            start.UseShellExecute = false;
            start.WorkingDirectory = config.GetValue<string>("WorkingDir");
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }
        }
    }
}
