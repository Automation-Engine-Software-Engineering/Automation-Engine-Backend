using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork.Model.DTO;

namespace ViewModels.ViewModels.Entity
{
    public  class EntityValueDto
    {
        public ListDto<string> Header { get; set; }
        public ListDto<Dictionary<string , object>> Body { get; set; }
    }
}
