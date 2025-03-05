using DataLayer.Models.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels.WorkFlow
{
    public class WorkFlowDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<NodeDto> Nodes { get; set; }
        public List<EdgeDto> Edges { get; set; }
    }
    public class NodeDto
    {
        public UnknownType Type { get; set; }
        public int Icon { get; set; }
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public int formId { get; set; }
        public int entityId { get; set; }
    }
    public class EdgeDto
    {
        public string Source { get; set; }
        public string SourceHandle { get; set; }
        public string Target { get; set; }
        public string TargetHandle { get; set; }
    }

}
