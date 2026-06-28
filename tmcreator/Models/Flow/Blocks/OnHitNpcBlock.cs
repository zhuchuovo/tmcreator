namespace tmcreator.Models.Flow;

public static class OnHitNpcBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "on_hit_npc",
        Name = "当击中NPC时",
        Category = BlockCategory.Event,
        EventComment = "When this item hits an NPC",
        EventDescriptor = new()
        {
            Host = FlowEventHost.Item,
            AppendCode = Append
        },
        Params =
        {
            new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "npc" }
        }
    };

    private static void Append(FlowEventGenerationContext context, IReadOnlyList<FlowEventGroup> groups)
    {
        if (groups.Count == 0)
            return;

        var sb = context.Builder;
        sb.AppendLine();
        sb.AppendLine("        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)");
        sb.AppendLine("        {");
        sb.AppendLine("            NPC npc = target;");
        sb.AppendLine("            Player targetPlayer = null;");
        context.AppendGroupBodies(groups, 12);
        sb.AppendLine("        }");
    }
}
