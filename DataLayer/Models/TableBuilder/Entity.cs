using DataLayer.Models.FormBuilder;
using DataLayer.Models.WorkFlow;
using System.ComponentModel;

namespace DataLayer.Models.TableBuilder
{
    [Description("the table of database")]
    public class Entity
    {
        public int Id { get; set; }
        public string PreviewName { get; set; } //just for preview (each lang)
        public string TableName { get; set; } //for database (en and - and _)
        public string? Description { get; set; }

        #region relations
        public List<EntityProperty>? Properties { get; set; }
        public List<Form>? Forms { get; set; }
        //        public List<Node> nodes { get; set; }
        #endregion
    }
}
