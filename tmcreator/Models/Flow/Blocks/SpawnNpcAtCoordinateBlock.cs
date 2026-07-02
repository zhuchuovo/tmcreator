namespace tmcreator.Models.Flow;

public static class SpawnNpcAtCoordinateBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "spawn_npc_at_coordinate",
        Name = "从坐标生成生物",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "x", Label = "X坐标", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "y", Label = "Y坐标", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "npc_id", Label = "生物ID", Type = ParamType.Number, DefaultValue = "1" }
        },
        AppendStatement = Append
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string x = context.GetFloatExpression(block, "x", "0");
        string y = context.GetFloatExpression(block, "y", "0");
        string npcId = context.GetIntExpression(block, "npc_id", "1");
        string positionVar = context.Next("flowNpcPosition");

        context.AppendLine(indent, $"Vector2 {positionVar} = new Vector2({x}, {y});");
        context.AppendLine(indent, $"NPC.NewNPC({context.SourceExpression}, (int)Math.Round({positionVar}.X), (int)Math.Round({positionVar}.Y), {npcId});");
    }
}
