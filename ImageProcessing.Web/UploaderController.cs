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
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ImageProcessing.Web
{

    public class UploaderController : Controller
    {
        private readonly IConfiguration _config;
        private readonly string _targetFilePath;
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions;
        public UploaderController(IConfiguration config)
        {
            _targetFilePath = config.GetValue<string>("StoredFilesPath");
            _fileSizeLimit= config.GetValue<long>("FileSizeLimit");
            _permittedExtensions = config.GetValue<string>("PermittedExtensions")?.Split(',');

        }
        /// <summary>
        /// Index-
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public async Task<string[]> GetStadiumNames()
        {   
            string[] stadiumNames = Directory.GetDirectories(_targetFilePath).Select(Path.GetFileName)
                            .ToArray();
            return stadiumNames;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stdName"></param>
        /// <returns></returns>
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public async Task<List<string>> GetDatesOfStadium(string stdName)
        {
            string[] EventDates = Directory.GetDirectories(Path.Combine(_targetFilePath,stdName)).Select(Path.GetFileName)
                            .ToArray();
            List<string> datefinal = new List<string>();

          foreach (string datex in EventDates)
            {
                DateTime dt;
                bool isValid = ValidateDate(datex, out dt);
                if (isValid)
                {
                    datefinal.Add(dt.ToString("yyyy/MM/dd"));
                }
            }

            return datefinal;
        }
        static bool ValidateDate(string date, out DateTime dt)
        {
            bool isValid = DateTime.TryParseExact(date, "yyyy-MM-dd", null, 0, out dt);
            //bool isValid = DateTime.TryParseExact(date, "yyyy-MM-dd-HH-mm", null, 0, out dt);
            if (!isValid) return false;
            DateTime min = new DateTime(1967, 1, 1);
            DateTime max = new DateTime(3000, 1, 1);
            if (dt < min || dt > max) return false;
            return true;
        }
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<IActionResult> SubmitFormUpload(IFormCollection formCollection)
        {
            try
            {
                string message = string.Empty;
                if (Request.HasFormContentType)
                {
                    var form = Request.Form;

                    string stadiumName = formCollection["stadiumName"];
                    string eventDate = formCollection["eventDate"];
                    var files = form.Files[0];

                    message = "Stadium Name: " + stadiumName;
                    message += "\nEvent Date: " + eventDate;

                    var formFileContent = await FileHelpers.ProcessFormFile<BufferedMultipleFileUploadPhysical>(
                       form.Files[0], ModelState, _permittedExtensions,_fileSizeLimit);
                    var trustedFileNameForFileStorage = $"{Guid.NewGuid()}_{Path.GetFileName(form.Files[0].FileName)}";
                    var folderPath = Path.Combine(_targetFilePath, stadiumName, eventDate);
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

                return Content(message);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
    }
}
