namespace tmcreator.Models.Flow;

public enum FlowEventHost
{
    Item,
    Accessory,
    Buff,
    Projectile
}

public sealed class FlowEventDescriptor
{
    public FlowEventHost Host { get; set; }
    public bool IsDefault { get; set; }
    public bool InvokeWithoutGroups { get; set; }
    public Action<FlowEventGenerationContext, IReadOnlyList<FlowEventGroup>>? AppendCode { get; set; }
    public Action<FlowEventGenerationContext, IReadOnlyList<FlowEventGroup>>? AppendAccessoryUpdateCode { get; set; }
    public Action<FlowEventGenerationContext, IReadOnlyList<FlowEventGroup>>? AppendAccessoryPostUpdateCode { get; set; }
    public Action<FlowEventGenerationContext, IReadOnlyList<FlowEventGroup>>? AppendBuffActiveCode { get; set; }
    public Action<FlowEventGenerationContext, IReadOnlyList<FlowEventGroup>>? AppendBuffInactiveCode { get; set; }
}
