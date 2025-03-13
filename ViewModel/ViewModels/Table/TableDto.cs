using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels.Table
{
    public class TableDto
    {
        public List<string> Header { get; set; }
        public List<List<string>> Body { get; set; }
    }
    public class TableInputDto
    {
        public int Id { get; set; }
    }
}
