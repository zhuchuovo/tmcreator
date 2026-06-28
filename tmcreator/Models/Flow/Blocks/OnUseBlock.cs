namespace tmcreator.Models.Flow;

public static class OnUseBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "on_use",
        Name = "当使用物品时",
        Category = BlockCategory.Event,
        EventComment = "When this item is used",
        EventDescriptor = new()
        {
            Host = FlowEventHost.Item,
            IsDefault = true,
            AppendCode = Append
        }
    };

    private static void Append(FlowEventGenerationContext context, IReadOnlyList<FlowEventGroup> groups)
    {
        if (groups.Count == 0)
            return;

        var sb = context.Builder;
        sb.AppendLine();
        sb.AppendLine("        public override bool? UseItem(Player player)");
        sb.AppendLine("        {");
        sb.AppendLine("            NPC npc = null;");
        sb.AppendLine("            Player targetPlayer = null;");
        context.AppendGroupBodies(groups, 12);
        sb.AppendLine("            return true;");
        sb.AppendLine("        }");
    }
}
