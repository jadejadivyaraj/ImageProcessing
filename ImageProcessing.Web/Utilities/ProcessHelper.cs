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
        public static void SearchFile(IConfiguration config, string folderPath, string imagePath)
        {
            var start = new ProcessStartInfo();
            start.FileName = config.GetValue<string>("PythonExePath");
            start.Arguments = string.Format("{0} {1} {2} {3} {4}"
                , config.GetValue<string>("PythonScriptPath")
                , folderPath
                , imagePath
                , "PersonName"
                , 3);
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
