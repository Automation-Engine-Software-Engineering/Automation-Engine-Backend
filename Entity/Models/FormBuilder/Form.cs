using Entities.Models.TableBuilder;
using System.ComponentModel;

namespace Entities.Models.FormBuilder
{
    [Description("Each form that have entity and property")]
    public class Form
    {
        public Form(string name, string? description, string? htmlFormBody)
        {
            this.Name = name;
            this.Description = description;
            this.HtmlFormBody = htmlFormBody;
        }

        private Form() { }

        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? HtmlFormBody { get; set; }

        #region relations
        public List<Entity>? Entities { get; set; }
        // public List<Node> nodes { get; set; }
        #endregion
    }
}