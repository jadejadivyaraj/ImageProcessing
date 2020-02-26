using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProcessing.Web.Models
{
    public class SearchResult
    {
        public IEnumerable<ImageMeta> Images { get; set; }
    }

    public class ImageMeta
    {
        public string Name { get; set; }

        public string Path { get; set; }    
    }
}
