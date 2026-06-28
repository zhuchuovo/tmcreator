namespace tmcreator.Models.Flow;

public static class AccessoryUnequipBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "accessory_unequip",
        Name = "脱下饰品时",
        Category = BlockCategory.Event,
        EventComment = "When this accessory is removed",
        EventDescriptor = new()
        {
            Host = FlowEventHost.Accessory,
            AppendAccessoryPostUpdateCode = AppendPostUpdate
        }
    };

    private static void AppendPostUpdate(FlowEventGenerationContext context, IReadOnlyList<FlowEventGroup> groups)
    {
        if (groups.Count == 0)
            return;

        var sb = context.Builder;
        sb.AppendLine("            if (!_equippedThisTick && _wasEquipped)");
        sb.AppendLine("            {");
        sb.AppendLine("                Player player = Player;");
        sb.AppendLine("                NPC npc = null;");
        sb.AppendLine("                Player targetPlayer = player;");
        context.AppendGroupBodies(groups, 16, "player.GetSource_FromThis()");
        sb.AppendLine("            }");
        sb.AppendLine();
    }
}
