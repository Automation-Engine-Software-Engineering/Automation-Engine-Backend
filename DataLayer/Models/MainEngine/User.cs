using DataLayer.Models.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models.MainEngine
{
    public class User
    {
        public int Id { get; set; }

        public List<WorkFlow_User> workFlowUser { get; set; }
    }
}
