namespace tmcreator.Models.Flow;

public static class ValueEventTargetPositionBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "value_event_target_position",
        Name = "事件目标的位置",
        Category = BlockCategory.Value,
        ValueKind = ValueExpressionKind.Coordinate,
        BuildValueExpression = BuildExpression,
        RequiredHelpers = { FlowHelperNames.GetEventTargetPosition }
    };

    private static string BuildExpression(FlowGenerationContext context, BlockInstance block)
    {
        return "Flow_GetEventTargetPosition(player, npc, targetPlayer)";
    }
}
