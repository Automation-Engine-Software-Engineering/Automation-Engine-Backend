﻿using Entities.Models.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models.TableBuilder
{
    [Description("The column of each table (entity)")]
    public class EntityProperty
    {
        public EntityProperty(string? previewName, string propertyName, string? description, string? defaultValue, PropertyType type, Entity? entity)
        {
            this.PreviewName = previewName;
            this.PropertyName = propertyName;
            this.Description = description;
            this.DefaultValue = defaultValue;
            this.Type = type;
            this.Entity = entity;
            this.EntityId = entity.Id;
        }

        public EntityProperty(){}

        public int Id { get; set; }
        public string? PreviewName { get; set; }  // For preview (each language)
        public string PropertyName { get; set; } = "";// Database name (en, -, and _)
        public string? Description { get; set; }
        public string? DefaultValue { get; set; }
        public string? IsRequiredErrorMessage { get; set; }
        public string? DefaultErrorMessage { get; set; } = null;
        public string? ToolType { get; set; } = null;
        public string? IconClass { get; set; } = null;
        public PropertyType Type { get; set; }

        #region Relations
        public int EntityId { get; set; }
        [ForeignKey(nameof(EntityId))]
        public Entity? Entity { get; set; }
        #endregion
    }
}