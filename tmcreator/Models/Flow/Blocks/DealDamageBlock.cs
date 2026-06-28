namespace tmcreator.Models.Flow;

public static class DealDamageBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "deal_damage",
        Name = "造成伤害",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "target", Label = "对目标", Type = ParamType.Target, DefaultValue = "npc" },
            new() { Name = "amount", Label = "伤害值", Type = ParamType.Number, DefaultValue = "100" },
            new() { Name = "damage_type", Label = "伤害类型", Type = ParamType.DamageType, DefaultValue = "melee" }
        },
        AppendStatement = Append,
        RequiredHelpers = { FlowHelperNames.ForEachNpc, FlowHelperNames.ForEachPlayer, FlowHelperNames.DamageNpc, FlowHelperNames.DamagePlayer }
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string selector = context.GetParamString(block, "target", "npc");
        string amount = context.GetIntExpression(block, "amount", "100");
        context.AppendLine(indent, $"Flow_ForEachNpc(player, npc, targetPlayer, \"{context.EscapeString(selector)}\", flowNpc => Flow_DamageNpc(player, flowNpc, {amount}));");
        context.AppendLine(indent, $"Flow_ForEachPlayer(player, targetPlayer, \"{context.EscapeString(selector)}\", flowPlayer => Flow_DamagePlayer(player, flowPlayer, {amount}));");
    }
}
