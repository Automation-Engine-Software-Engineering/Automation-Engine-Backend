using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DataLayer.Models.TableBuilder
{
    [Description("The column of each table (entity)")]
    public class EntityProperty
    {
        public EntityProperty(string previewName, string propertyName, string description, bool allowNull, string defaultValue, PropertyType type, Entity? entity)
        {
            this.PreviewName = previewName;
            this.PropertyName = propertyName;
            this.Description = description;
            this.AllowNull = allowNull;
            this.DefaultValue = defaultValue;
            this.Type = type;
            this.Entity = entity;
            this.EntityId = entity.Id;
        }

        private EntityProperty(){}

        public int Id { get; set; }
        public string PreviewName { get; set; }  // For preview (each language)
        public string PropertyName { get; set; } // Database name (en, -, and _)
        public string? Description { get; set; }
        public bool AllowNull { get; set; } = true;
        public string? DefaultValue { get; set; }

        #region Relations
        public PropertyType Type { get; set; }
        public int EntityId { get; set; }
        [ForeignKey(nameof(EntityId))]
        public Entity? Entity { get; set; }
        #endregion
    }

    public enum PropertyType
    {
        INT = 1,
        Float = 2,
        NvarcharShort = 3,
        NvarcharLong = 4,
        BIT = 5,
        Time = 6,
        binaryLong = 7
    }
}