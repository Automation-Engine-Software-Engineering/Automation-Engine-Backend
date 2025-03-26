using DataLayer.Models.FormBuilder;
using DataLayer.Models.TableBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models.WorkFlows
{
    public class Node
    {
        [Key]
        public string Id { get; set; }
        public UnknownType Type { get; set; }
        public int Icon { get; set; }
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public int? formId { get; set; }
        [ForeignKey(nameof(formId))]
        public Form? form { get; set; }

        public int? entityId { get; set; }
        [ForeignKey(nameof(entityId))]
        public Entity? entity { get; set; }
    }
    public enum UnknownType
    {
        table = 1, form = 2, noDynamic = 3
    }
}
