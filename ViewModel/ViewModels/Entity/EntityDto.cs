using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels.Entity
{
    public class EntityDto
    {
        public int Id { get; set; } = 0;
        public string? PreviewName { get; set; }
        public string? TableName { get; set; }
        public string? Description { get; set; }
    }
}
