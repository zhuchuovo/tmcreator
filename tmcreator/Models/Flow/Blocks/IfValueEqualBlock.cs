namespace tmcreator.Models.Flow;

public static class IfValueEqualBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "if_value_equal",
        Name = "如果 数值 比较 数值",
        Category = BlockCategory.Condition,
        HasTrueBranch = true,
        HasFalseBranch = false,
        TrueLabel = "执行",
        Params =
        {
            new() { Name = "left", Label = "左值", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "compare", Label = "比较符", Type = ParamType.Dropdown, DefaultValue = "=", Options = new[] { "=", "!=", ">", "<", ">=", "<=" } },
            new() { Name = "right", Label = "右值", Type = ParamType.Number, DefaultValue = "0" }
        },
        BuildCondition = Build
    };

    private static string Build(FlowGenerationContext context, BlockInstance block)
    {
        string left = context.GetFloatExpression(block, "left", "0");
        string compare = context.GetParamString(block, "compare", "=");
        string right = context.GetFloatExpression(block, "right", "0");
        return $"Flow_CompareFloat({left}, \"{context.EscapeString(compare)}\", {right})";
    }
}
