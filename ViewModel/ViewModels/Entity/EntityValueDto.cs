using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork.Model.DTO;

namespace ViewModels.ViewModels.Entity
{
	public class EntityValueDto
	{
		public IEnumerable<string>? Header { get; set; }
		public IEnumerable<Dictionary<string, object>> Body { get; set; }
	}
}
