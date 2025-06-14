﻿using Entities.Models.FormBuilder;
using System.ComponentModel;

namespace Entities.Models.TableBuilder
{
    [Description("the table of database")]
    public class Entity
    {
        public Entity(string previewName, string tableName, string description, List<EntityProperty>? properties, List<Form>? forms)
        {
            this.PreviewName = previewName;
            this.TableName = tableName;
            this.Description = description;
            this.Properties = properties;
            this.Forms = forms;
        }

        public Entity() { }

        public int Id { get; set; }
        public string? PreviewName { get; set; } //just for preview (each lang)
        public string TableName { get; set; } = "";//for database (en and - and _)
        public string? Description { get; set; }

        #region relations
        public List<EntityProperty>? Properties { get; set; }
        public List<Form>? Forms { get; set; }
        public  List<Entity_EntityRelation> entity_EntityRelation{ get; set; }
        //  public List<Node> nodes { get; set; }
        #endregion
    }
}
