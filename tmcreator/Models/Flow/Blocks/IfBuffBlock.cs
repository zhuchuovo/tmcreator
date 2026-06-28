namespace tmcreator.Models.Flow;

public static class IfBuffBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "if_buff",
        Name = "如果 有Buff",
        Category = BlockCategory.Condition,
        HasTrueBranch = true,
        HasFalseBranch = true,
        TrueLabel = "有",
        FalseLabel = "无",
        Params =
        {
            new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "npc" },
            new() { Name = "buff", Label = "Buff", Type = ParamType.Buff, DefaultValue = "1" }
        },
        BuildCondition = Build,
        RequiredHelpers = { FlowHelperNames.HasBuff }
    };

    private static string Build(FlowGenerationContext context, BlockInstance block)
    {
        string selector = context.GetParamString(block, "target", "npc");
        string buff = context.GetIntExpression(block, "buff", "1");
        return $"Flow_HasBuff(player, npc, targetPlayer, \"{context.EscapeString(selector)}\", {buff})";
    }
}
