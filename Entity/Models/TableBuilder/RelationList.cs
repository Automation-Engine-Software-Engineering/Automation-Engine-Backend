using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models.TableBuilder
{
    [Description("the table of database")]
    public class RelationList
    {

        public RelationList() { }

        public int Id { get; set; }
        public int WorkflowUserId { get; set; }
        public int? Element1 { get; set; }
        public int? Element2 { get; set; }
        public int RelationId { get; set; }
    }
}
