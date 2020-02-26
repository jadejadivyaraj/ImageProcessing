using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageProcessing.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessing.Web.ViewComponents
{
    public class SearchResultViewComponent:ViewComponent
    {

        public async Task<IViewComponentResult> InvokeAsync(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                return View("Default", new SearchResult(){Images = Enumerable.Empty<ImageMeta>()});
            }

            var curretDirectory = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot");
            var res = new SearchResult() {Images = Directory.GetFiles(folder).Select(x=>new ImageMeta()
            {
                Name = Path.GetFileName(x),
                Path = x.Replace(curretDirectory,"",StringComparison.OrdinalIgnoreCase)
            }).ToList()};
           
            return View("Default", res);
        }
    }
}
