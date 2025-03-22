using Microsoft.AspNetCore.Http;
using DataLayer.Models.TableBuilder;
using ViewModels.ViewModels.Entity;

namespace ViewModels.ViewModels.FormBuilder
{
    public class FormDto
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; }
        public string? Description { get; set; }
        public double SizeWidth { get; set; } = 0;
        public double SizeHeight { get; set; } = 0;
        public bool IsAutoHeight { get; set; }
        public string BackgroundColor { get; set; } = "#ffffff";
        public string? BackgroundImgPath { get; set; }
        public bool IsRepeatedImage { get; set; }
        public string HtmlFormBody { get; set; }
    }
    public class UpdateFormInputModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public double? SizeWidth { get; set; }
        public double? SizeHeight { get; set; }
        public bool? IsAutoHeight { get; set; }
        public string? BackgroundColor { get; set; }
        public IFormFile? BackgroundImg { get; set; }
        public bool? IsRepeatedImage { get; set; }
        public string? HtmlFormBody { get; set; }
    }
}
