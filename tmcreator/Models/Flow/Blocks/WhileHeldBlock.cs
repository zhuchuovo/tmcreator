namespace tmcreator.Models.Flow;

public static class WhileHeldBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "while_held",
        Name = "手持时持续",
        Category = BlockCategory.Event,
        EventComment = "While this item is held",
        EventDescriptor = new()
        {
            Host = FlowEventHost.Item,
            AppendCode = Append
        }
    };

    private static void Append(FlowEventGenerationContext context, IReadOnlyList<FlowEventGroup> groups)
    {
        if (groups.Count == 0)
            return;

        var sb = context.Builder;
        sb.AppendLine();
        sb.AppendLine("        public override void HoldItem(Player player)");
        sb.AppendLine("        {");
        sb.AppendLine("            NPC npc = null;");
        sb.AppendLine("            Player targetPlayer = null;");
        context.AppendGroupBodies(groups, 12);
        sb.AppendLine("        }");
    }
}
