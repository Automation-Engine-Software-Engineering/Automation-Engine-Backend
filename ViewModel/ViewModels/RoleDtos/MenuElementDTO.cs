using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Models.Workflows;

namespace ViewModels.ViewModels.RoleDtos
{
    public class MenuElementDTO
    {
        public string Name { get; set; }
        public int MenuType { get; set; }
        public string? link { get; set; }
        public string? icon { get; set; }

        public Entities.Models.Workflows.Workflow? workflow { get; set; }
        public List<MenuElementDTO> childs { get; set; } = new List<MenuElementDTO>();
    }

    public class MenuElementInsertDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MenuType { get; set; }
        public string? icon { get; set; }

        public string? link { get; set; }
        public int? ParentMenuElemntId { get; set; }
        public int WorkflowId { get; set; }
        public int RoleId { get; set; }
    }

}
