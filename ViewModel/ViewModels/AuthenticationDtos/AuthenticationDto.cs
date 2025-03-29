using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels.AuthenticationDtos
{
    public class ChangePasswordInputModel
    {
        public required string OldPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}
