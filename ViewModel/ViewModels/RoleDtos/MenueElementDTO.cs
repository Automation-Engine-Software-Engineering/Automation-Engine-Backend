using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Models.Workflows;

namespace ViewModels.ViewModels.RoleDtos
{
    public class MenueElementDTO
    {
        public string Name { get; set; }
        public int MenueType { get; set; }
        public Entities.Models.Workflows.Workflow? workflow { get; set; }
        public List<MenueElementDTO> childs { get; set; } = new List<MenueElementDTO>();
    }

    public class MenueElementInsertDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MenueType { get; set; }
        public int? ParentMenueElemntId { get; set; }
        public int WorkflowId { get; set; }
        public int RoleId { get; set; }
    }

}
