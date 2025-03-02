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
        public string PreviewName { get; set; } = "";
        public string PeropertyName { get; set; } = "";
        public PeropertyTypes Type { get; set; }
        public bool AllowNull { get; set; }
        public string DefaultValue { get; set; } = "";

        public int EntityId { get; set; }
        [ForeignKey(nameof(EntityId))]
        public Entity? Entity { get; set; }
    }
    public enum PeropertyTypes
    {
        intiger = 1, nvarchar = 2, bit = 3, RadioBox = 4
    }
}
