namespace tmcreator.Models.Flow;

public static class SetValueBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "set_value",
        Name = "设置数值",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "npc" },
            new() { Name = "stat", Label = "属性", Type = ParamType.Dropdown, DefaultValue = "life", Options = new[] { "life", "damage", "defense" } },
            new() { Name = "value", Label = "值", Type = ParamType.Number, DefaultValue = "0" }
        },
        AppendStatement = Append,
        RequiredHelpers = { FlowHelperNames.ForEachNpc, FlowHelperNames.ForEachPlayer, FlowHelperNames.SetNpcValue, FlowHelperNames.SetPlayerValue }
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string selector = context.GetParamString(block, "target", "npc");
        string stat = context.GetParamString(block, "stat", "life");
        string value = context.GetIntExpression(block, "value", "0");
        context.AppendLine(indent, $"Flow_ForEachNpc(player, npc, targetPlayer, \"{context.EscapeString(selector)}\", flowNpc => Flow_SetNpcValue(flowNpc, \"{context.EscapeString(stat)}\", {value}));");
        context.AppendLine(indent, $"Flow_ForEachPlayer(player, targetPlayer, \"{context.EscapeString(selector)}\", flowPlayer => Flow_SetPlayerValue(flowPlayer, \"{context.EscapeString(stat)}\", {value}));");
    }
}
