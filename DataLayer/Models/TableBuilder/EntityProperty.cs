using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models.TableBuilder
{
    public class EntityProperty
    {
        public int Id { get; set; }
        public string PreviewName { get; set; }
        public string PropertyName { get; set; }
        public PropertyTypes Type { get; set; }
        public bool AllowNull { get; set; }
        public string? DefaultValue { get; set; }
        public double SizeWidth { get; set; }
        public double SizeHeight { get; set; }

        public int EntityId { get; set; }
        [ForeignKey(nameof(EntityId))]
        public Entity? Entity { get; set; }
    }

    public enum PropertyTypes
    {
        INT = 1, ShortNVARCHAR = 2 , LongNVARCHAR = 3, BIT = 4
    }
}
