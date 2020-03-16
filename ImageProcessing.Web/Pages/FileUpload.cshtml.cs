using ImageProcessing.Web.Models;
using ImageProcessing.Web.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ImageProcessing.Web
{
    [DisableRequestSizeLimit]
    public class FileUploadModel : PageModel
    {
        private readonly IConfiguration _config;

        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions;
        private readonly string _targetFilePath;
        private readonly string _uploadFileURL;

        public FileUploadModel(IConfiguration config)
        {
            _config = config;
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");

            _targetFilePath = config.GetValue<string>("StoredFilesPath");

            _permittedExtensions = config.GetValue<string>("PermittedExtensions")?.Split(',');

            _uploadFileURL = config.GetValue<string>("ApiPath");
        }

        [BindProperty]
        public BufferedMultipleFileUploadPhysical FileUpload { get; set; }

        public string Result { get; private set; }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostUploadAsync()
        {

            int SerialNumber = 0;

            if (!ModelState.IsValid)
            {
                Result = "Please correct the form.";

                return Page();
            }

            List<ImageUploadStatus> UploadedImages = new List<ImageUploadStatus>();
           
           foreach (var formFile in FileUpload.FormFiles)
            {

                HttpClient client = new HttpClient();
                byte[] data;
                using (var br = new BinaryReader(formFile.OpenReadStream()))
                {
                    data = br.ReadBytes((int)formFile.OpenReadStream().Length);
                }

                ByteArrayContent bytes = new ByteArrayContent(data);

                MultipartFormDataContent multiContent = new MultipartFormDataContent();

                multiContent.Add(bytes,"files",formFile.FileName);

                multiContent.Add(new StringContent(_targetFilePath), "TargetPath");

                multiContent.Add(new StringContent(FileUpload.Stadium), "Stadium");

                multiContent.Add(new StringContent(JsonConvert.SerializeObject(_permittedExtensions)), "PermittedExtensions");

                multiContent.Add(new StringContent(FileUpload.DateTime.ToString("yyyy-MM-dd-HH-mm")), "DateTime");
                
                multiContent.Add(new StringContent(_fileSizeLimit.ToString()), "FileSizeLimit");
                
                HttpResponseMessage httpResponse = await client.PostAsync(_uploadFileURL, multiContent);

                string responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    SerialNumber++;
                    UploadedImages.
                        Add(new ImageUploadStatus { SerialNumber = SerialNumber, ImageName = formFile.FileName, Status = "Success" });
                }
                else
                {
                    // Log exception
                    throw new Exception(responseContent);
                }

                //Thread.Sleep(500);
            }
            ViewData["UploadedCount"] = SerialNumber;
            ViewData["ImageStatus"] = UploadedImages;
            Result = "Uploaded Successfully";
            return Page();
        }

    }
}