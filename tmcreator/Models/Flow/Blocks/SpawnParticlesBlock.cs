namespace tmcreator.Models.Flow;

public static class SpawnParticlesBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "spawn_particles",
        Name = "释放粒子",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "target", Label = "位置", Type = ParamType.Target, DefaultValue = "player" },
            new() { Name = "particle", Label = "粒子", Type = ParamType.Particle, DefaultValue = "15" },
            new() { Name = "amount", Label = "数量", Type = ParamType.Number, DefaultValue = "12" },
            new() { Name = "speed", Label = "速度", Type = ParamType.Number, DefaultValue = "2" }
        },
        AppendStatement = Append
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string selector = context.GetParamString(block, "target", "player");
        string particle = context.GetIntExpression(block, "particle", "15");
        string amount = context.GetIntExpression(block, "amount", "12");
        string speed = context.GetFloatExpression(block, "speed", "2");
        context.AppendLine(indent, $"Flow_ForEachNpc(player, npc, targetPlayer, \"{context.EscapeString(selector)}\", flowNpc => Flow_SpawnParticles(flowNpc.Center, {particle}, {amount}, {speed}));");
        context.AppendLine(indent, $"Flow_ForEachPlayer(player, targetPlayer, \"{context.EscapeString(selector)}\", flowPlayer => Flow_SpawnParticles(flowPlayer.Center, {particle}, {amount}, {speed}));");
    }
}
