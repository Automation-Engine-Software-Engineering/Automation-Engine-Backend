using DataLayer.Models.TableBuilder;
using System.ComponentModel;

namespace DataLayer.Models.FormBuilder
{
    [Description("Each form that have entity and property")]
    public class Form
    {
        public Form(string name, string? description, double? SizeHeight, string? backgroundImgPath, double sizeWidth ,
        string? htmlFormBody, bool isAutoHeight = false, string backgroundColor = "#ffffff", bool isRepeatedImage = false)
        {
            this.Name = name;
            this.Description = description;
            this.SizeHeight = SizeHeight;
            this.IsAutoHeight = isAutoHeight;
            this.SizeWidth = sizeWidth;
            this.BackgroundColor = backgroundColor;
            this.BackgroundImgPath = backgroundImgPath;
            this.IsRepeatedImage = isRepeatedImage;
            this.HtmlFormBody = htmlFormBody;
        }

        private Form() { }

        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public double SizeWidth { get; set; }
        public double? SizeHeight { get; set; }
        public bool IsAutoHeight { get; set; } = false;
        public string BackgroundColor { get; set; } = "#ffffff";
        public string? BackgroundImgPath { get; set; }
        public bool IsRepeatedImage { get; set; } = false;
        public string? HtmlFormBody { get; set; }

        #region relations
        public List<Entity>? Entities { get; set; }
        // public List<Node> nodes { get; set; }
        #endregion
    }
}