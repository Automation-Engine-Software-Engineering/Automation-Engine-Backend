using Entities.Models.MainEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models.Workflows
{
    public class Workflow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Node> Nodes { get; set; }
        public List<Role_Workflow> Role_Workflows { get; set; }
        public List<Workflow_User> workflowUser { get; set; }
    }
}