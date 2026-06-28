namespace tmcreator.Models.Flow;

public static class AccessoryWearingBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "accessory_wearing",
        Name = "当穿戴饰品时",
        Category = BlockCategory.Event,
        EventComment = "While this accessory is worn",
        EventDescriptor = new()
        {
            Host = FlowEventHost.Accessory,
            IsDefault = true,
            AppendAccessoryUpdateCode = AppendUpdate
        }
    };

    private static void AppendUpdate(FlowEventGenerationContext context, IReadOnlyList<FlowEventGroup> groups)
    {
        if (groups.Count > 0)
            context.AppendGroupBodies(groups, 12, "player.GetSource_FromThis()");
    }
}
