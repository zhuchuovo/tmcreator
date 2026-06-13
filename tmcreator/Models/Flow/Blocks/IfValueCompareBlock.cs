namespace tmcreator.Models.Flow;

public static class IfValueCompareBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "if_value_compare",
        Name = "如果 数值比较",
        Category = BlockCategory.Condition,
        HasTrueBranch = true,
        HasFalseBranch = true,
        TrueLabel = "执行",
        FalseLabel = "否则",
        Params =
        {
            new() { Name = "left", Label = "左值", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "compare", Label = "比较", Type = ParamType.Dropdown, DefaultValue = ">", Options = new[] { ">", "<", "=", "!=", ">=", "<=" } },
            new() { Name = "right", Label = "右值", Type = ParamType.Number, DefaultValue = "0" }
        },
        BuildCondition = Build
    };

    private static string Build(FlowGenerationContext context, BlockInstance block)
    {
        string left = context.GetFloatExpression(block, "left", "0");
        string compare = context.GetParamString(block, "compare", ">");
        string right = context.GetFloatExpression(block, "right", "0");
        return $"Flow_CompareFloat({left}, \"{context.EscapeString(compare)}\", {right})";
    }
}
