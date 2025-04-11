using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels.Workflow
{
    public class WorkflowUserDto
    {
        public int Id { get; set; }
        public int WorkflowId { get; set; }
    }
}
