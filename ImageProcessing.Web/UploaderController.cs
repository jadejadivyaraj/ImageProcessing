using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageProcessing.Web.Models;
using ImageProcessing.Web.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ImageProcessing.Web
{

    public class UploaderController : Controller
    {
        private IHostingEnvironment hostingEnvironment;

        public UploaderController(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public string Index()
        {

            return "hello";
        }

        [Microsoft.AspNetCore.Mvc.HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Index(FileUploadPostData fileUploadPostData)
        {
            //new string[] { ".gif", ".png", ".jpeg",".jpg", ".zip" }

            if (Request.HasFormContentType)
            {
                var form = Request.Form;
                var formFileContent = await FileHelpers.ProcessFormFile<BufferedMultipleFileUploadPhysical>(
                     form.Files[0], ModelState, JsonConvert.DeserializeObject<string[]>(fileUploadPostData.PermittedExtensions),
                     long.Parse(fileUploadPostData.FileSizeLimit));
                var trustedFileNameForFileStorage = $"{Guid.NewGuid()}_{Path.GetFileName(form.Files[0].FileName)}";
                var folderPath = Path.Combine(fileUploadPostData.TargetPath, fileUploadPostData.Stadium, fileUploadPostData.DateTime);
                var filePath = Path.Combine(folderPath, trustedFileNameForFileStorage);


                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                using (var fileStream = System.IO.File.Create(filePath))
                {
                    await fileStream.WriteAsync(formFileContent);

                }
            }

            return this.Content("success");
        }

    }
}
