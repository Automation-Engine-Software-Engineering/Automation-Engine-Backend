using DataLayer.Models.TableBuilder;
using System.ComponentModel;

namespace DataLayer.Models.FormBuilder
{
    [Description("the form that have entity and property")]
    public class Form
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public double SizeWidth { get; set; }
        public double SizeHeight { get; set; }
        public string BackgroundColor { get; set; } = "#ffffff";
        public string? BackgroundImgPath { get; set; }
        public bool DuplicateImage { get; set; } = false;
        public string? HtmlFormBody { get; set; }

        #region relations
        public List<Entity>? Entities { get; set; }
        // public List<Node> nodes { get; set; }
        #endregion
    }
}
