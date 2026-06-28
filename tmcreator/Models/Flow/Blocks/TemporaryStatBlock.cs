namespace tmcreator.Models.Flow;

public static class TemporaryStatBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "temporary_stat",
        Name = "临时增减属性",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "operation", Label = "方式", Type = ParamType.Dropdown, DefaultValue = "add", Options = new[] { "add 增加", "subtract 减少" } },
            new() { Name = "target", Label = "事件目标", Type = ParamType.Target, DefaultValue = "player" },
            new() { Name = "amount", Label = "数值", Type = ParamType.Number, DefaultValue = "10" },
            new() { Name = "stat", Label = "属性", Type = ParamType.Dropdown, DefaultValue = "damage", Options = new[] { "defense 防御", "damage 伤害", "damage_reduction 百分比减伤", "crit 暴击率" } },
            new() { Name = "damage_type", Label = "职业", Type = ParamType.DamageType, DefaultValue = "generic" },
            new() { Name = "duration", Label = "持续(秒)", Type = ParamType.Number, DefaultValue = "5" }
        },
        AppendStatement = Append,
        RequiredHelpers = { FlowHelperNames.ForEachNpc, FlowHelperNames.ForEachPlayer, FlowHelperNames.AddTemporaryNpcStat, FlowHelperNames.AddTemporaryPlayerStat }
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string selector = context.GetParamString(block, "target", "player");
        string operation = context.GetParamString(block, "operation", "add");
        string amount = context.GetFloatExpression(block, "amount", "10");
        string stat = context.GetParamString(block, "stat", "damage");
        string damageType = context.GetParamString(block, "damage_type", "generic");
        string duration = context.GetIntExpression(block, "duration", "5");
        string signedAmount = operation == "subtract" ? $"-({amount})" : amount;
        string statKey = stat is "damage" or "crit" ? $"{damageType}_{stat}" : stat;
        context.AppendLine(indent, $"Flow_ForEachNpc(player, npc, targetPlayer, \"{context.EscapeString(selector)}\", flowNpc => Flow_AddTemporaryNpcStat(flowNpc, \"{context.EscapeString(statKey)}\", {signedAmount}, {duration} * 60));");
        context.AppendLine(indent, $"Flow_ForEachPlayer(player, targetPlayer, \"{context.EscapeString(selector)}\", flowPlayer => Flow_AddTemporaryPlayerStat(flowPlayer, \"{context.EscapeString(statKey)}\", {signedAmount}, {duration} * 60));");
    }
}
