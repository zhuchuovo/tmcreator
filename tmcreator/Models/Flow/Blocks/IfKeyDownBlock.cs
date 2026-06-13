namespace tmcreator.Models.Flow;

public static class IfKeyDownBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "if_key_down",
        Name = "摁下按键时执行",
        Category = BlockCategory.Action,
        HasTrueBranch = true,
        HasFalseBranch = false,
        TrueLabel = "执行",
        Params =
        {
            new() { Name = "key", Label = "按键", Type = ParamType.Text, DefaultValue = "F" }
        },
        AppendStatement = Append
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string key = FlowCodeUtility.NormalizeKeyboardKeyName(context.GetParamString(block, "key", "F"));
        string keyVar = context.Next("flowKey");
        context.AppendLine(indent, $"if (System.Enum.TryParse<Microsoft.Xna.Framework.Input.Keys>(\"{context.EscapeString(key)}\", true, out var {keyVar}) && Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown({keyVar}))");
        context.AppendLine(indent, "{");
        context.AppendStatements(block.TrueBranch, indent + 4);
        context.AppendLine(indent, "}");
    }
}
