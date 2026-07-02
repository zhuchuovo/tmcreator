namespace tmcreator.Models.Flow;

public static class ValueNearestPlayerPositionBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "value_nearest_player_position",
        Name = "距离事件目标最近的玩家的位置",
        Category = BlockCategory.Value,
        ValueKind = ValueExpressionKind.Coordinate,
        BuildValueExpression = BuildExpression,
        RequiredHelpers = { FlowHelperNames.GetNearestPlayerPosition }
    };

    private static string BuildExpression(FlowGenerationContext context, BlockInstance block)
    {
        return "Flow_GetNearestPlayerPosition(player, npc, targetPlayer)";
    }
}
