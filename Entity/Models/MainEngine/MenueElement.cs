using System.ComponentModel.DataAnnotations.Schema;
using Entities.Models.Workflows;

namespace Entities.Models.MainEngine
{
    public class MenueElement
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MenueType { get; set; }
        public int? ParentMenueElemntId { get; set; }
        public int? WorkflowId { get; set; }
        [ForeignKey(nameof(WorkflowId))]
        
        public Workflow? workflow { get; set; }
        public int RoleId { get; set; }
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }
    }

    public enum MenueType
    {
        parent = 1, linke = 2
    }
}