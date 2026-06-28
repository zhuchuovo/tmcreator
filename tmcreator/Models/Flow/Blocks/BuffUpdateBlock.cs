namespace tmcreator.Models.Flow;

public static class BuffUpdateBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "buff_update",
        Name = "持续时间内时",
        Category = BlockCategory.Event,
        EventComment = "While this buff is active",
        EventDescriptor = new()
        {
            Host = FlowEventHost.Buff,
            IsDefault = true,
            AppendBuffActiveCode = AppendActive
        }
    };

    private static void AppendActive(FlowEventGenerationContext context, IReadOnlyList<FlowEventGroup> groups)
    {
        if (groups.Count > 0)
            context.AppendGroupBodies(groups, 16, "player.GetSource_FromThis()");
    }
}
