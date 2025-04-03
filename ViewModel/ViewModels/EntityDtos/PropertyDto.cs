using DataLayer.Models.TableBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels.Entity
{
    public class PropertyDto
    {
        public int Id { get; set; } = 0;
        public string? PreviewName { get; set; }
        public string? PropertyName { get; set; }
        public string? Description { get; set; }
        public string Type { get; set; } = "1";
        public string? DefaultValue { get; set; } = "مقدار فیلد را وارد کنید";
        public int? EntityId { get; set; }
    }
}
