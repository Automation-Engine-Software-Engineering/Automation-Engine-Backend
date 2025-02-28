using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models.FormBuilder
{
    public class Peroperty
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PeropertyTypes Type { get; set; }
        public int EntityId { get; set; }
        [ForeignKey(nameof(EntityId))]
        public Entity Entity { get; set; }
    }
    public enum PeropertyTypes
    {
        Intiger = 1, Text = 2, CheckBox = 3, RadioBox = 4
    }
}
