namespace tmcreator.Models.Flow;

public static class BuffOnGainBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "buff_on_gain",
        Name = "获得效果时",
        Category = BlockCategory.Event,
        EventComment = "When this buff is gained",
        EventDescriptor = new()
        {
            Host = FlowEventHost.Buff,
            AppendBuffActiveCode = AppendActive
        }
    };

    private static void AppendActive(FlowEventGenerationContext context, IReadOnlyList<FlowEventGroup> groups)
    {
        if (groups.Count == 0)
            return;

        var sb = context.Builder;
        sb.AppendLine("                if (!_hadBuff)");
        sb.AppendLine("                {");
        context.AppendGroupBodies(groups, 20, "player.GetSource_FromThis()");
        sb.AppendLine("                }");
    }
}
