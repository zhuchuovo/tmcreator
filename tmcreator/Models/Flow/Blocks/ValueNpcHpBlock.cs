namespace tmcreator.Models.Flow;

public static class ValueNpcHpBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "value_npc_hp",
        Name = "目标生命值",
        Category = BlockCategory.Value,
        Params =
        {
            new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "npc" }
        },
        BuildValueExpression = BuildExpression,
        RequiredHelpers = { FlowHelperNames.GetLife }
    };

    private static string BuildExpression(FlowGenerationContext context, BlockInstance block)
    {
        string selector = context.GetParamString(block, "target", "npc");
        return $"Flow_GetLife(player, npc, targetPlayer, \"{context.EscapeString(selector)}\")";
    }
}
