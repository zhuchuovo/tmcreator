namespace tmcreator.Models.Flow;

public static class OnHitPvpBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "on_hit_pvp",
        Name = "当击中玩家时",
        Category = BlockCategory.Event,
        EventComment = "When this item hits a player",
        Params =
        {
            new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "player" }
        }
    };
}
