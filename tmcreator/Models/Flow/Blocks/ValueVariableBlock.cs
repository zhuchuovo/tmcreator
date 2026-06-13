namespace tmcreator.Models.Flow;

public static class ValueVariableBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "value_variable",
        Name = "变量值",
        Category = BlockCategory.Value,
        Params =
        {
            new() { Name = "name", Label = "变量名", Type = ParamType.Text, DefaultValue = "myValue" }
        },
        BuildValueExpression = BuildExpression
    };

    private static string BuildExpression(FlowGenerationContext context, BlockInstance block)
    {
        string name = context.GetParamString(block, "name", "myValue");
        return $"Flow_GetVariable(\"{context.EscapeString(name)}\")";
    }
}
