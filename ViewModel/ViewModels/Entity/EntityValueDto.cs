using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels.Entity
{
    public  class EntityValueDto
    {
        public List<string> Header { get; set; }
        public List<Dictionary<string , object>> Body { get; set; }
    }
}
