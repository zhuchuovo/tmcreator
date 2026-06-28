namespace tmcreator.Models.Flow;

public static class RestoreManaBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "restore_mana",
        Name = "恢复魔力",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "player" },
            new() { Name = "amount", Label = "魔力量", Type = ParamType.Number, DefaultValue = "20" }
        },
        AppendStatement = Append,
        RequiredHelpers = { FlowHelperNames.ForEachPlayer, FlowHelperNames.RestoreMana }
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string selector = context.GetParamString(block, "target", "player");
        string amount = context.GetIntExpression(block, "amount", "20");
        context.AppendLine(indent, $"Flow_ForEachPlayer(player, targetPlayer, \"{context.EscapeString(selector)}\", flowPlayer => Flow_RestoreMana(flowPlayer, {amount}));");
    }
}
