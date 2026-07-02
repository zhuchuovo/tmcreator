namespace tmcreator.Models.Flow;

public static class ValueSourcePositionBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "value_source_position",
        Name = "事件施加者的位置",
        Category = BlockCategory.Value,
        ValueKind = ValueExpressionKind.Coordinate,
        BuildValueExpression = BuildExpression,
        RequiredHelpers = { FlowHelperNames.GetSourcePosition }
    };

    private static string BuildExpression(FlowGenerationContext context, BlockInstance block)
    {
        return "Flow_GetSourcePosition(player)";
    }
}
