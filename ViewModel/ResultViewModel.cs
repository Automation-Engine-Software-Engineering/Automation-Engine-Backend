using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public class ResultViewModel<T>
	{
		public T? Data { get; set; }
        public string? Message { get; set; }
        public bool Status { get; set; } = true;
        public int StatusCode { get; set; } = 200;
        public int TotalCount { get; set; }
        public int ListSize { get; set; }
        public int ListNumber { get; set; }
    }

}
