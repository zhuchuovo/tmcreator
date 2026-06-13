namespace tmcreator.Models.Flow;

public static class HealPlayerBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "heal_player",
        Name = "治疗玩家",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "amount", Label = "治疗量", Type = ParamType.Number, DefaultValue = "20" }
        },
        AppendStatement = Append
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string amount = context.GetIntExpression(block, "amount", "20");
        context.AppendLine(indent, $"Flow_HealPlayer(player, {amount});");
    }
}
