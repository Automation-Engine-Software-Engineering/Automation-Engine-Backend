using DataLayer.Models.MainEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels.WorkFlow
{
    public class WorkFlowUserDto
    {
        public int Id { get; set; }
        public string WorkFlowState { get; set; }
        public int UserId { get; set; }
        public int WorkFlowId { get; set; }
    }
}
