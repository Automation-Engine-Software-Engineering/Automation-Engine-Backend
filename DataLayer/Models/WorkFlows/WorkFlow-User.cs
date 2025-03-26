using DataLayer.Models.MainEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models.WorkFlows
{
    public class WorkFlow_User
    {
        public int Id { get; set; }
        public string WorkFlowState { get; set; }

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public int WorkFlowId { get; set; }
        [ForeignKey(nameof(WorkFlowId))]
        public WorkFlow WorkFlow { get; set; }
    }
}
