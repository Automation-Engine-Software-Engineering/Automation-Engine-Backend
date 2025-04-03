using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels.RoleDtos
{
    public class TokenResultViewModel
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public bool NeedNewPassword { get; set; }
    }
}
