using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels.WorkFlow
{
    public class WorkflowAccess
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsAccess { get; set; }
    }
}
