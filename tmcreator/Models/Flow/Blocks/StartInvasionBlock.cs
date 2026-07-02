namespace tmcreator.Models.Flow;

public static class StartInvasionBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "start_invasion",
        Name = "触发事件",
        Category = BlockCategory.Action,
        Params =
        {
            new()
            {
                Name = "invasion",
                Label = "事件",
                Type = ParamType.Dropdown,
                DefaultValue = "goblin",
                Options = new[] { "goblin 哥布林入侵", "pirate 海盗入侵", "martian 外星人入侵" }
            }
        },
        AppendStatement = Append,
        RequiredHelpers = { FlowHelperNames.StartInvasion }
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string invasion = context.GetParamString(block, "invasion", "goblin");
        context.AppendLine(indent, $"Flow_StartInvasion(\"{context.EscapeString(invasion)}\");");
    }
}
