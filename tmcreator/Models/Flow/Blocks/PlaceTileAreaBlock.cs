namespace tmcreator.Models.Flow;

public static class PlaceTileAreaBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "place_tile_area",
        Name = "范围放置方块",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "start_x", Label = "起点X", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "start_y", Label = "起点Y", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "end_x", Label = "终点X", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "end_y", Label = "终点Y", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "tile_id", Label = "方块ID", Type = ParamType.Number, DefaultValue = "0" }
        },
        AppendStatement = Append,
        RequiredHelpers = { FlowHelperNames.PlaceTileArea }
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string startX = context.GetFloatExpression(block, "start_x", "0");
        string startY = context.GetFloatExpression(block, "start_y", "0");
        string endX = context.GetFloatExpression(block, "end_x", "0");
        string endY = context.GetFloatExpression(block, "end_y", "0");
        string tileId = context.GetIntExpression(block, "tile_id", "0");
        string startVar = context.Next("flowTileStart");
        string endVar = context.Next("flowTileEnd");

        context.AppendLine(indent, $"Vector2 {startVar} = new Vector2({startX}, {startY});");
        context.AppendLine(indent, $"Vector2 {endVar} = new Vector2({endX}, {endY});");
        context.AppendLine(indent, $"Flow_PlaceTileArea({startVar}, {endVar}, {tileId});");
    }
}
