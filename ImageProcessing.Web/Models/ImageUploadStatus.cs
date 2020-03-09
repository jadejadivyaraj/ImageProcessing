using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProcessing.Web.Models
{
    public class ImageUploadStatus
    {
        public int SerialNumber { get; set; }
        public string ImageName { get; set; }
        public string Status { get; set; }
    }
}
