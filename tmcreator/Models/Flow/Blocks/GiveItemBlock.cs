namespace tmcreator.Models.Flow;

public static class GiveItemBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "give_item",
        Name = "给予物品",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "item_id", Label = "物品ID", Type = ParamType.Number, DefaultValue = "1" },
            new() { Name = "amount", Label = "数量", Type = ParamType.Number, DefaultValue = "1" }
        },
        AppendStatement = Append
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string itemId = context.GetIntExpression(block, "item_id", "1");
        string amount = context.GetIntExpression(block, "amount", "1");
        context.AppendLine(indent, $"player.QuickSpawnItem({context.SourceExpression}, {itemId}, {amount});");
    }
}
