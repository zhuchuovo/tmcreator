namespace tmcreator.Models.Flow;

public static class TeleportBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "teleport",
        Name = "传送",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "player" },
            new() { Name = "x", Label = "X坐标", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "y", Label = "Y坐标", Type = ParamType.Number, DefaultValue = "0" }
        },
        AppendStatement = Append,
        RequiredHelpers = { FlowHelperNames.ForEachNpc, FlowHelperNames.ForEachPlayer }
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string selector = context.GetParamString(block, "target", "player");
        string x = context.GetFloatExpression(block, "x", "0");
        string y = context.GetFloatExpression(block, "y", "0");
        context.AppendLine(indent, $"Flow_ForEachNpc(player, npc, targetPlayer, \"{context.EscapeString(selector)}\", flowNpc => flowNpc.position = new Vector2({x}, {y}));");
        context.AppendLine(indent, $"Flow_ForEachPlayer(player, targetPlayer, \"{context.EscapeString(selector)}\", flowPlayer => flowPlayer.Teleport(new Vector2({x}, {y})));");
    }
}
