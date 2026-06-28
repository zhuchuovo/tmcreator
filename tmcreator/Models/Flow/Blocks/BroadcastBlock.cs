namespace tmcreator.Models.Flow;

public static class BroadcastBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "broadcast",
        Name = "发送消息",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "message", Label = "消息内容", Type = ParamType.Text, DefaultValue = "Hello World!" }
        },
        AppendStatement = Append,
        RequiredHelpers = { FlowHelperNames.Broadcast }
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string message = context.GetParamString(block, "message", "Hello World!");
        context.AppendLine(indent, $"Flow_Broadcast(\"{context.EscapeString(message)}\");");
    }
}
