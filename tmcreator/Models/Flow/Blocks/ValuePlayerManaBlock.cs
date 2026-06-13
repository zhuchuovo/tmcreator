namespace tmcreator.Models.Flow;

public static class ValuePlayerManaBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "value_player_mana",
        Name = "玩家魔力值",
        Category = BlockCategory.Value,
        BuildValueExpression = BuildExpression
    };

    private static string BuildExpression(FlowGenerationContext context, BlockInstance block)
    {
        return "player.statMana";
    }
}
