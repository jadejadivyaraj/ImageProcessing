using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageProcessing.Web.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace ImageProcessing.Web
{
    public class SearchModel : PageModel
    {
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions;
        private readonly string _targetFilePath;
        private readonly IConfiguration _config;

        public SearchModel(IConfiguration config)
        {
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");

            _targetFilePath = config.GetValue<string>("StoredFilesPath");

            _permittedExtensions = config.GetValue<string>("PermittedExtensions")?.Split(',');
            _config = config;
        }

        [BindProperty]
        public BufferedSingleFileUploadDb FileUpload { get; set; }

        public string Result { get; private set; }

        public string ResultPath { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            // Perform an initial check to catch FileUpload class
            // attribute violations.
            if (!ModelState.IsValid)
            {
                Result = "Please correct the form.";

                return Page();
            }

            var formFileContent =
                await FileHelpers.ProcessFormFile<BufferedSingleFileUploadDb>(
                    FileUpload.FormFile, ModelState, _permittedExtensions,
                    _fileSizeLimit);

            // Perform a second check to catch ProcessFormFile method
            // violations. If any validation check fails, return to the
            // page.
            if (!ModelState.IsValid)
            {
                Result = "Please correct the form.";

                return Page();
            }

            var trustedFileNameForFileStorage = $"{Guid.NewGuid()}_{Path.GetFileName(FileUpload.FormFile.FileName)}";
            var personName = "PersonName";
            var folderPath = Path.Combine(_targetFilePath, FileUpload.Stadium, personName);
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

            ProcessHelper.SearchFile(_config,
                Path.Combine(Path.GetDirectoryName(folderPath), FileUpload.DateTime.ToString("yyyy-MM-dd-HH-mm")),
                Path.GetDirectoryName(filePath)
                ,personName, FileUpload.ResultCount);

            //wait till algo runs
            ProcessHelper.WaitUntillAlgoComplete(_config);

            ResultPath = Path.Combine(Path.GetDirectoryName(filePath), personName);
            return await OnPostUploadedAsync();
        }

        public async Task<IActionResult> OnPostUploadedAsync()
        {
            return Page();
        }
    }
    public class BufferedSingleFileUploadDb
    {
        [Required]
        [Display(Name = "File")]
        public IFormFile FormFile { get; set; }

        [Required]
        [Display(Name = "Stadium")]
        [StringLength(100, MinimumLength = 0)]
        public string Stadium { get; set; }

        [Required]
        [Display(Name = "Result Count")]
        [Range(typeof(int),"1","1000")]
        public int ResultCount { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime DateTime { get; set; }
    }
}