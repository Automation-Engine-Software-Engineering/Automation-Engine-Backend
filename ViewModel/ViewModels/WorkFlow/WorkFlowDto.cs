namespace ViewModels.ViewModels.WorkFlow
{
    public class WorkFlowDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<NodeDto> Nodes { get; set; }
        public List<EdgeDto> Edges { get; set; }
    }
    public class NodeDto
    {
        public string Id { get; set; }
        public position Position { get; set; }
        public Data Data { get; set; }
        public int Type { get; set; }
    }
    public class Data
    {
        public string Icon { get; set; }
        public string Name { get; set; }
    }
    public class position
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }
    public class EdgeDto
    {
        public string Id { get; set; }
        public string Source { get; set; }
        public string SourceHandle { get; set; }
        public string Target { get; set; }
        public string TargetHandle { get; set; }
    }

}
