using DataLayer.Models.WorkFlows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models.MainEngine
{
    public class Role_WorkFlow
    {
        public int Id { get; set; }
        public int WorkFlowId { get; set; }
        public int RoleId { get; set; }
        public WorkFlow? WorkFlow { get; set; }
        public Role? Role { get; set; }
    }
}
