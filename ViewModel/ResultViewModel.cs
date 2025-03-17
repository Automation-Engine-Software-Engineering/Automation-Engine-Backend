using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public class ResultViewModel<T> : ResultViewModel
	{
		public T? Data { get; set; }
	}
	public class ResultViewModel
	{
		public string? Message { get; set; }
		public bool Status { get; set; } = true;
		public int StatusCode { get; set; } = 200;
		public int TotalCount { get; set; }
		public int ListSize { get; set; }
		public int ListNumber { get; set; }
		public object? Data { get; set; }

	}
}
