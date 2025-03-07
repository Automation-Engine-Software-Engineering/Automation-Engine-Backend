using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models.WorkFlow
{
    public class Edge
    {
        [Key]
        public string Id { get; set; }
        public string Source { get; set; }
        public string SourceHandle { get; set; }
        public string Target { get; set; }
        public string TargetHandle { get; set; }
    }
}
