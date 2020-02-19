using ImageProcessing.Web.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace ImageProcessing.Web
{
    public class FileUploadModel : PageModel
    {

        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions;
        private readonly string _targetFilePath;

        public FileUploadModel(IConfiguration config)
        {
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");

            _targetFilePath = config.GetValue<string>("StoredFilesPath");

            _permittedExtensions = config.GetValue<string>("PermittedExtensions")?.Split(',');
        }

        [BindProperty]
        public BufferedMultipleFileUploadPhysical FileUpload { get; set; }

        public string Result { get; private set; }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (!ModelState.IsValid)
            {
                Result = "Please correct the form.";

                return Page();
            }

            foreach (var formFile in FileUpload.FormFiles)
            {
                var formFileContent =
                    await FileHelpers
                        .ProcessFormFile<BufferedMultipleFileUploadPhysical>(
                            formFile, ModelState, _permittedExtensions,
                            _fileSizeLimit);

                if (!ModelState.IsValid)
                {
                    Result = "Please correct the form.";

                    return Page();
                }

                // For the file name of the uploaded file stored
                // server-side, use Path.GetRandomFileName to generate a safe
                // random file name.
                //var trustedFileNameForFileStorage = Path.GetRandomFileName();
                var trustedFileNameForFileStorage = $"{Guid.NewGuid()}_{Path.GetFileName(formFile.FileName)}";
                var folderPath = Path.Combine(_targetFilePath,FileUpload.Stadium,FileUpload.DateTime.ToString("yyyy-MM-dd-HH-mm"));
                var filePath = Path.Combine(folderPath, trustedFileNameForFileStorage);
                // **WARNING!**
                // In the following example, the file is saved without
                // scanning the file's contents. In most production
                // scenarios, an anti-virus/anti-malware scanner API
                // is used on the file before making the file available
                // for download or for use by other systems. 
                // For more information, see the topic that accompanies 
                // this sample.

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                using (var fileStream = System.IO.File.Create(filePath))
                {
                    await fileStream.WriteAsync(formFileContent);

                    // To work directly with the FormFiles, use the following
                    // instead:
                    //await formFile.CopyToAsync(fileStream);
                }
            }

            Result = "Uploaded Successfully";
            //return RedirectToPage();
            return RedirectToPage("./Search");
        }

    }

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