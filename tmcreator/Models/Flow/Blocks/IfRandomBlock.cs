namespace tmcreator.Models.Flow;

public static class IfRandomBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "if_random",
        Name = "如果 随机概率",
        Category = BlockCategory.Condition,
        HasTrueBranch = true,
        HasFalseBranch = true,
        TrueLabel = "成功",
        FalseLabel = "失败",
        Params =
        {
            new() { Name = "chance", Label = "概率%", Type = ParamType.Number, DefaultValue = "50" }
        },
        BuildCondition = Build
    };

    private static string Build(FlowGenerationContext context, BlockInstance block)
    {
        string chance = context.GetFloatExpression(block, "chance", "50");
        return $"Main.rand.NextDouble() * 100.0 < {chance}";
    }
}
