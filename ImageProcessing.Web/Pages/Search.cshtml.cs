using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
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
        private readonly string _strSessionID;
        private readonly string _logPath;

        public SearchModel(IConfiguration config)
        {

            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");

            _targetFilePath = config.GetValue<string>("StoredFilesPath");

            _permittedExtensions = config.GetValue<string>("PermittedExtensions")?.Split(',');
           
            _config = config;

            _logPath = "myapplog.txt";


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
            else
            {

                try
                {
                    //Delete the Person Folder
                    Directory.Delete(Path.Combine(_targetFilePath, FileUpload.Stadium, personName), true);

                    //Delete the P File
                    foreach (string file in Directory.GetFiles(Path.Combine(Path.GetDirectoryName(folderPath), FileUpload.DateTime.ToString("yyyy-MM-dd")), "*.p"))
                    {
                        System.IO.File.Delete(file);
                    }
                    //Create again Person Folder
                    Directory.CreateDirectory(Path.Combine(_targetFilePath, FileUpload.Stadium, personName));
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(_logPath, $"{DateTime.Now.ToString("o")} [ERR] {ex.Message}" + Environment.NewLine);
                }

            }

            using (var fileStream = System.IO.File.Create(filePath))
            {
                await fileStream.WriteAsync(formFileContent);

                // To work directly with the FormFiles, use the following
                // instead:
                //await formFile.CopyToAsync(fileStream);
            }

            try
            {
                ProcessHelper.SearchFile(_config,
                Path.Combine(Path.GetDirectoryName(folderPath), FileUpload.DateTime.ToString("yyyy-MM-dd")),
                Path.GetDirectoryName(filePath)
                , personName, FileUpload.ResultCount);

                ////wait till algo runs
                ProcessHelper.WaitUntillAlgoComplete(_config);

                //create the thumbnail folder and process all images 
                //Ref Path : C:\inetpub\wwwroot\wwwroot\images\1014\PersonName\PersonName
                string resultImagesPath = Path.Combine(folderPath, personName);
                int SerialNumber = 0;
                if (Directory.Exists(resultImagesPath))
                {
                    if (Directory.GetFiles(resultImagesPath, "*", SearchOption.AllDirectories).Length > 0)
                    {

                        foreach (string filename in Directory.GetFiles(resultImagesPath))
                        {
                            Bitmap sourceImage = new Bitmap(filename);
                            using (Bitmap objBitmap = new Bitmap(200, 200))
                            {
                                //Check for exif data to determin orientation of camera when photo was taken and rotate to what's expected
                                if (sourceImage.PropertyIdList.Contains(0x112)) //0x112 = Orientation
                                {
                                    var prop = sourceImage.GetPropertyItem(0x112);
                                    if (prop.Type == 3 && prop.Len == 2)
                                    {
                                        UInt16 orientationExif = BitConverter.ToUInt16(sourceImage.GetPropertyItem(0x112).Value, 0);
                                        if (orientationExif == 8)
                                        {
                                            sourceImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                        }
                                        else if (orientationExif == 3)
                                        {
                                            sourceImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                        }
                                        else if (orientationExif == 6)
                                        {
                                            sourceImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                        }
                                    }
                                }
                                objBitmap.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
                                using (Graphics objGraphics = Graphics.FromImage(objBitmap))
                                {
                                    // Set the graphic format for better result cropping   
                                    objGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                                    objGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                    objGraphics.DrawImage(sourceImage, 0, 0, 200, 200);

                                    // Save the file path, note we use png format to support png file
                                    //create thumbnail folders inside Person Folder

                                    if (!Directory.Exists(Path.Combine(folderPath, "thumbnails")))
                                    {
                                        Directory.CreateDirectory(Path.Combine(folderPath, "thumbnails"));
                                    }

                                    objBitmap.Save(Path.Combine(folderPath, "thumbnails", Path.GetFileName(filename)));
                                    SerialNumber++;
                                }
                            }


                        }

                    }
                }

                ResultPath = Path.Combine(folderPath, "thumbnails");
                if (SerialNumber > 0)
                {
                    ViewData["showButtonDownloaddAll"] = "yes";
                }
                else
                {
                    ViewData["showButtonDownloaddAll"] = "no";
                }
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(_logPath, $"{DateTime.Now.ToString("o")} [ERR] {ex.Message}" + Environment.NewLine);
                throw ex;
            }
            
            ViewData["StadiumName"] = FileUpload.Stadium;
            ViewData["DateTimePosted"] = FileUpload.DateTime.ToString("yyyy-MM-dd");
            return await OnPostUploadedAsync();
        }
        public bool ThumbnailCallback()
        {
            return false;
        }

        public async Task<IActionResult> OnPostUploadedAsync()
        {
            return Page();
        }

        public async Task<FileResult> OnPostDownloadAllZip(string stadium, string datetime)
        {

            try
            {
                
                if (stadium != "" && datetime != "")
                {
                    string zipFilePath = Path.Combine(_targetFilePath,stadium, "PersonName");
                    // Name of the ZIP File
                    var fileName = string.Format("{0}_ResultSet.zip", stadium + datetime.ToString() + "_1");

                    //Temp Path
                    var tempOutPutPath = Path.Combine(zipFilePath, fileName);

                    if (System.IO.File.Exists(tempOutPutPath))
                        System.IO.File.Delete(tempOutPutPath);

                    using (ZipOutputStream s = new ZipOutputStream(System.IO.File.Create(tempOutPutPath)))
                    {
                        s.SetLevel(9); // 0-9, 9 being the highest compression  

                        byte[] buffer = new byte[4096];


                        //read the images from the below path
                        var folderPath = Path.Combine(_targetFilePath, stadium, "PersonName", "PersonName");
                        foreach (string filename in Directory.GetFiles(folderPath,"*.*",SearchOption.TopDirectoryOnly))
                        {
                            ZipEntry entry = new ZipEntry(Path.GetFileName(filename));
                            entry.DateTime = DateTime.Now;
                            entry.IsUnicodeText = true;
                            s.PutNextEntry(entry);

                            using (FileStream fs = System.IO.File.OpenRead(filename))
                            {
                                int sourceBytes;
                                do
                                {
                                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                    s.Write(buffer, 0, sourceBytes);
                                } while (sourceBytes > 0);
                            }
                        }

                        s.Finish();
                        s.Flush();
                        s.Close();
                    }

                    byte[] finalResult = System.IO.File.ReadAllBytes(tempOutPutPath);
                    if (System.IO.File.Exists(tempOutPutPath))
                        System.IO.File.Delete(tempOutPutPath);

                    if (finalResult == null || !finalResult.Any())
                        throw new Exception(String.Format("No Files found with Image"));

                    return File(finalResult, "application/zip", fileName);
                }
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
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
        [Range(typeof(int), "1", "1000")]
        public int ResultCount { get; set; }

        [HiddenInput]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime DateTime { get; set; }
    }
}