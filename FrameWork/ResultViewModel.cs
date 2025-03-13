using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameWork
{
    public class ResultViewModel
    {
        public string message { get; set; }
        public bool status { get; set; }
        public int statusCode { get; set; }
        public object data { get; set; }
    }
}
