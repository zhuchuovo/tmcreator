namespace tmcreator.Models.Flow;

public static class IfHpBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "if_hp",
        Name = "如果 生命值",
        Category = BlockCategory.Condition,
        HasTrueBranch = true,
        HasFalseBranch = true,
        TrueLabel = "是",
        FalseLabel = "否",
        Params =
        {
            new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "npc" },
            new() { Name = "compare", Label = "比较", Type = ParamType.Dropdown, DefaultValue = "<", Options = new[] { "<", ">", "=", "≥", "≤" } },
            new() { Name = "value", Label = "数值", Type = ParamType.Number, DefaultValue = "50" }
        },
        BuildCondition = Build
    };

    private static string Build(FlowGenerationContext context, BlockInstance block)
    {
        string selector = context.GetParamString(block, "target", "npc");
        string compare = context.GetParamString(block, "compare", "<");
        string value = context.GetIntExpression(block, "value", "50");
        return $"Flow_Compare(Flow_GetLife(player, npc, targetPlayer, \"{context.EscapeString(selector)}\"), \"{context.EscapeString(compare)}\", {value})";
    }
}
