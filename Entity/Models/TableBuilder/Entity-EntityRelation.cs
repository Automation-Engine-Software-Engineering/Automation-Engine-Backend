using Entities.Models.FormBuilder;
using System.ComponentModel;

namespace Entities.Models.TableBuilder
{
    [Description("the table of database")]
    public class Entity_EntityRelation
    {

        public Entity_EntityRelation() { }

        public int Id { get; set; }
        public int ParentId { get; set; }

        public int ChildId { get; set; }
    }
}
