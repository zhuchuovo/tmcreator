namespace tmcreator.Models.Flow;

public static class ValueConstantBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "value_constant",
        Name = "数值",
        Category = BlockCategory.Value,
        Params =
        {
            new() { Name = "value", Label = "值", Type = ParamType.Number, DefaultValue = "0" }
        },
        BuildValueExpression = BuildExpression
    };

    private static string BuildExpression(FlowGenerationContext context, BlockInstance block)
    {
        return context.GetFloatExpression(block, "value", "0");
    }
}
