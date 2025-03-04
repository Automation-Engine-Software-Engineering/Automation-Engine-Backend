using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models.WorkFlow
{
    public class WorkFlow
    {
        public int Id { get; set; }
        public List<Node> Nodes { get; set; }
        public List<Edge> Edges { get; set; }

        public List<WorkFlow_User> workFlowUser { get; set; }
    }
}