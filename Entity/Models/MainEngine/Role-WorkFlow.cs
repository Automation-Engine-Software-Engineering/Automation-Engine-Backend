using Entities.Models.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models.MainEngine
{
    public class Role_Workflow
    {
        public int Id { get; set; }
        #region relations
        public int WorkflowId { get; set; }
        public int RoleId { get; set; }
        public Workflow? Workflow { get; set; }
        public Role? Role { get; set; }
        #endregion
    }
}
