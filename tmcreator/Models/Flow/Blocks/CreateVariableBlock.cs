namespace tmcreator.Models.Flow;

public static class CreateVariableBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "create_variable",
        Name = "创建变量",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "name", Label = "变量名", Type = ParamType.Text, DefaultValue = "myValue" },
            new() { Name = "value", Label = "数值", Type = ParamType.Number, DefaultValue = "0" }
        },
        AppendStatement = Append
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string name = context.GetParamString(block, "name", "myValue");
        string value = context.GetFloatExpression(block, "value", "0");
        context.AppendLine(indent, $"Flow_SetVariable(\"{context.EscapeString(name)}\", {value});");
    }
}
