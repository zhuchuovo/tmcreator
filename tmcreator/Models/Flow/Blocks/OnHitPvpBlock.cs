namespace tmcreator.Models.Flow;

public static class OnHitPvpBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "on_hit_pvp",
        Name = "当击中玩家时",
        Category = BlockCategory.Event,
        EventComment = "When this item hits a player",
        EventDescriptor = new()
        {
            Host = FlowEventHost.Item,
            AppendCode = Append
        },
        Params =
        {
            new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "player" }
        }
    };

    private static void Append(FlowEventGenerationContext context, IReadOnlyList<FlowEventGroup> groups)
    {
        if (groups.Count == 0)
            return;

        var sb = context.Builder;
        sb.AppendLine();
        sb.AppendLine("        public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)");
        sb.AppendLine("        {");
        sb.AppendLine("            NPC npc = null;");
        sb.AppendLine("            Player targetPlayer = target;");
        context.AppendGroupBodies(groups, 12);
        sb.AppendLine("        }");
    }
}
