using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Models.TableBuilder
{
    [Description("The column of each table (entity)")]
    public class EntityProperty
    {
        public int Id { get; set; }
        public string PreviewName { get; set; } // For preview (each language)
        public string PropertyName { get; set; } // Database name (en, -, and _)
        public string? Description { get; set; }
        public bool AllowNull { get; set; } = true;
        public string? DefaultValue { get; set; }

        #region Relations
        public PropertyTypes Type { get; set; }
        public int EntityId { get; set; }
        [ForeignKey(nameof(EntityId))]
        public Entity? Entity { get; set; }
        #endregion
    }

    public enum PropertyTypes
    {
        INT = 1,
        Float = 2,
        NvarcharShort = 3,
        NvarcharLong = 4,
        BIT = 5,
        Time = 6
    }
}