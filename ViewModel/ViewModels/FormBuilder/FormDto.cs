using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels.FormBuilder
{
    public class FormDto
    {
        public int Id { get; set; } = 0;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public double SizeWidth { get; set; } = 10;
        public double SizeHeight { get; set; } = 10;
        public string BackgroundColor { get; set; } = "#ffffff";
        public string? BackgroundImgPath { get; set; }
        public string HtmlFormBody { get; set; } = "<p>ساخته شده توسط شرکت نرم افزاری پارسه آذین مبین<p/>";
    }
}
