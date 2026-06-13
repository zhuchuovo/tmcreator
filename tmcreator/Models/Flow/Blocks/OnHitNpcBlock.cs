namespace tmcreator.Models.Flow;

public static class OnHitNpcBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "on_hit_npc",
        Name = "当击中NPC时",
        Category = BlockCategory.Event,
        EventComment = "When this item hits an NPC",
        Params =
        {
            new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "npc" }
        }
    };
}
