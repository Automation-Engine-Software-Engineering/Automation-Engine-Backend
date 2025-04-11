using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels.Workflow
{
    public class RoleUserDto
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int UserId { get; set; }
    }
}
