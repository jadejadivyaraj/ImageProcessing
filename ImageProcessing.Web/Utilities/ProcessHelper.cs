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
        public static void UploadFolder(IConfiguration config, string photographerFolderPath)
        {
            var start = new ProcessStartInfo();
            start.FileName = config.GetValue<string>("PythonExePath");
            start.Arguments = string.Format("{0} {1} {2} {3} {4}"
                , config.GetValue<string>("PythonScriptPath")
                , config.GetValue<string>("ReferenceImagePath")
                , photographerFolderPath
                , "PersonName"
                , config.GetValue<string>("DefaultResultCount"));
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

        public static void SearchFile(IConfiguration config, string photographerFolderPath, string personSearchfolder, string personName, int resultCount)
        {
            var start = new ProcessStartInfo();
            start.FileName = config.GetValue<string>("PythonExePath");
            start.Arguments = string.Format("{0} {1} {2} {3} {4}"
                , config.GetValue<string>("PythonScriptPath")
                , personSearchfolder
                , photographerFolderPath
                , personName
                , resultCount);
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

        public static void WaitUntillAlgoComplete(IConfiguration config)
        {
            var progressFile = Path.Combine(config.GetValue<string>("WorkingDir"), "Progress.txt");

            var retry = 100;
            while (retry>0)
            {
                retry--;
                var status= File.ReadAllText(progressFile);
                if (status == "0100")
                {
                    break;
                }

                Task.Delay(5000);
            }
        }
    }
}
