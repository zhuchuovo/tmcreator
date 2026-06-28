namespace tmcreator.Models.Flow;

public static class AccessoryEquipBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "accessory_equip",
        Name = "安装饰品时",
        Category = BlockCategory.Event,
        EventComment = "When this accessory is equipped",
        EventDescriptor = new()
        {
            Host = FlowEventHost.Accessory,
            AppendAccessoryUpdateCode = AppendUpdate
        }
    };

    private static void AppendUpdate(FlowEventGenerationContext context, IReadOnlyList<FlowEventGroup> groups)
    {
        if (groups.Count == 0)
            return;

        var sb = context.Builder;
        sb.AppendLine("            if (firstEquipTick)");
        sb.AppendLine("            {");
        context.AppendGroupBodies(groups, 16, "player.GetSource_FromThis()");
        sb.AppendLine("            }");
    }
}
