using Microsoft.AspNetCore.Http;
using Entities.Models.TableBuilder;
using ViewModels.ViewModels.Entity;

namespace ViewModels.ViewModels.FormBuilder
{
    public class FormDto
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; }
        public string? Description { get; set; }
        public string HtmlFormBody { get; set; }
    }
    public class UpdateFormInputModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? HtmlFormBody { get; set; }
    }
    public class ImageModel
    {
        public string? ImageUrl { get; set; }
    }
}
