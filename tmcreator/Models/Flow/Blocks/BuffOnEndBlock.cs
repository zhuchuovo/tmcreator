namespace tmcreator.Models.Flow;

public static class BuffOnEndBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "buff_on_end",
        Name = "效果消失时",
        Category = BlockCategory.Event,
        EventComment = "When this buff ends",
        EventDescriptor = new()
        {
            Host = FlowEventHost.Buff,
            AppendBuffInactiveCode = AppendInactive
        }
    };

    private static void AppendInactive(FlowEventGenerationContext context, IReadOnlyList<FlowEventGroup> groups)
    {
        if (groups.Count > 0)
            context.AppendGroupBodies(groups, 16, "player.GetSource_FromThis()");
    }
}
