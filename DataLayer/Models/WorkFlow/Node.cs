using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.Models.FormBuilder;

namespace DataLayer.Models.WorkFlow
{
    public class Node
    {
        [Key]
        public string Id { get; set; }
        public UnknownType Type { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        #region relation
        public int? FormId { get; set; }
        [ForeignKey(nameof(FormId))]
        public Form Form { get; set; }

        public string? LastNodeId { get; set; }
        [ForeignKey(nameof(LastNodeId))]
        public Node LastNode { get; set; }


        public string? NextNodeId { get; set; }
        [ForeignKey(nameof(NextNodeId))]
        public Node NextNode { get; set; }
        #endregion
    }

    public enum UnknownType
    {
        form = 1, dynamic = 2
    }
}
