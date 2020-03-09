using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ImageProcessing.Web.Models
{
    public class FileUploadPostData
    {

        public IFormFile File { get; set; }

        public string PermittedExtensions { get; set; }


        public string TargetPath { get; set; }
        public string Stadium { get; set; }

        //public string[] PermittedExtensions { get; set; }
        public string FileSizeLimit { get; set; }

        public string DateTime { get; set; }

    }
}
