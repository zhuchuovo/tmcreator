namespace tmcreator.Models.Flow;

public static class KillNpcBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "kill_npc",
        Name = "杀死目标",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "npc" }
        },
        AppendStatement = Append,
        RequiredHelpers = { FlowHelperNames.ForEachNpc, FlowHelperNames.ForEachPlayer, FlowHelperNames.DamageNpc, FlowHelperNames.DamagePlayer }
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string selector = context.GetParamString(block, "target", "npc");
        context.AppendLine(indent, $"Flow_ForEachNpc(player, npc, targetPlayer, \"{context.EscapeString(selector)}\", flowNpc => Flow_DamageNpc(player, flowNpc, 999999));");
        context.AppendLine(indent, $"Flow_ForEachPlayer(player, targetPlayer, \"{context.EscapeString(selector)}\", flowPlayer => Flow_DamagePlayer(player, flowPlayer, 999999));");
    }
}
