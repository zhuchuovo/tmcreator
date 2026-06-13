namespace tmcreator.Models.Flow;

public sealed class FlowEventGroup
{
    public string EventId { get; set; } = "on_use";
    public BlockInstance? EventBlock { get; set; }
    public List<BlockInstance> Blocks { get; } = new();
}
