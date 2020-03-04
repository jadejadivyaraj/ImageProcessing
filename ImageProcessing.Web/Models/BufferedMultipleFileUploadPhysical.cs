using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProcessing.Web.Models
{
    
        public class BufferedMultipleFileUploadPhysical
        {
            [Required]
            [Display(Name = "Images")]
            public List<IFormFile> FormFiles { get; set; }

            [Required]
            [Display(Name = "Stadium")]
            [StringLength(100, MinimumLength = 0)]
            public string Stadium { get; set; }

            [Display(Name = "Date")]
            [DataType(DataType.Date)]
            public DateTime DateTime { get; set; }
        }
    
}
