namespace tmcreator.Models.Flow;

public static class ValueMathBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "value_math",
        Name = "数值运算",
        Category = BlockCategory.Value,
        Params =
        {
            new() { Name = "left", Label = "左值", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "operator", Label = "运算", Type = ParamType.Dropdown, DefaultValue = "*", Options = new[] { "+", "-", "*", "/", "%", "=" } },
            new() { Name = "right", Label = "右值", Type = ParamType.Number, DefaultValue = "1" }
        },
        BuildValueExpression = BuildExpression
    };

    private static string BuildExpression(FlowGenerationContext context, BlockInstance block)
    {
        string left = context.GetFloatExpression(block, "left", "0");
        string right = context.GetFloatExpression(block, "right", "1");
        string op = context.GetParamString(block, "operator", "*");

        return op switch
        {
            "+" => $"(({left}) + ({right}))",
            "-" => $"(({left}) - ({right}))",
            "*" => $"(({left}) * ({right}))",
            "/" => $"(({right}) == 0 ? 0 : ({left}) / ({right}))",
            "%" => $"(({left}) * ({right}) / 100f)",
            "=" => $"(({left}) == ({right}) ? 1 : 0)",
            _ => $"(({left}) * ({right}))"
        };
    }
}
