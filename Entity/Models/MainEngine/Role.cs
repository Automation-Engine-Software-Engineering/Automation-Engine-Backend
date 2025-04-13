using Entities.Models.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models.MainEngine
{
    public class Role
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        #region relations
        public List<Role_Workflow>? role_Workflows{ get; set; }
        public List<Role_User>? role_User{ get; set; }
        #endregion
    }
}
