using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models.FormBuilder
{
    public class Entity
    {
        [Key]
        public int Id { get; set; }
        public string PreviewName { get; set; } = "";
        public string TableName { get; set; } = "";
        public string? Description { get; set; }

        public List<Peroperty>? Peroperties { get; set; }
        public List<Form>? Forms { get; set; }
    }
}
