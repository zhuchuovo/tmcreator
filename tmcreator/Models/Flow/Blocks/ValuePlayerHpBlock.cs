namespace tmcreator.Models.Flow;

public static class ValuePlayerHpBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "value_player_hp",
        Name = "玩家生命值",
        Category = BlockCategory.Value,
        BuildValueExpression = BuildExpression
    };

    private static string BuildExpression(FlowGenerationContext context, BlockInstance block)
    {
        return "player.statLife";
    }
}
