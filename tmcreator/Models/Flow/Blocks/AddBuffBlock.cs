namespace tmcreator.Models.Flow;

public static class AddBuffBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "add_buff",
        Name = "添加Buff",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "player" },
            new() { Name = "buff", Label = "Buff类型", Type = ParamType.Buff, DefaultValue = "1" },
            new() { Name = "duration", Label = "持续时间(秒)", Type = ParamType.Number, DefaultValue = "60" }
        },
        AppendStatement = Append
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string selector = context.GetParamString(block, "target", "player");
        string buff = context.GetIntExpression(block, "buff", "1");
        string duration = context.GetIntExpression(block, "duration", "60");
        context.AppendLine(indent, $"Flow_ForEachNpc(player, npc, targetPlayer, \"{context.EscapeString(selector)}\", flowNpc => flowNpc.AddBuff({buff}, {duration} * 60));");
        context.AppendLine(indent, $"Flow_ForEachPlayer(player, targetPlayer, \"{context.EscapeString(selector)}\", flowPlayer => flowPlayer.AddBuff({buff}, {duration} * 60));");
    }
}
