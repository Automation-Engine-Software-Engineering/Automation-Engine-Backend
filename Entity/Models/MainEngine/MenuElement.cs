using System.ComponentModel.DataAnnotations.Schema;
using Entities.Models.Workflows;

namespace Entities.Models.MainEngine
{
    public class MenuElement
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MenuType { get; set; }
        public int? ParentMenuElemntId { get; set; }
        public string? link { get; set; }
        public string? icon { get; set; }
        public int? WorkflowId { get; set; }
        [ForeignKey(nameof(WorkflowId))]
        
        public Workflow? workflow { get; set; }
        public int RoleId { get; set; }
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }
    }

    public enum MenuType
    {
        parent = 1, linke = 2
    }
}