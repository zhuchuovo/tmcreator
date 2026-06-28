namespace tmcreator.Models.Flow;

public static class ProjectileOnHitNpcBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "projectile_on_hit_npc",
        Name = "当弹幕命中时",
        Category = BlockCategory.Event,
        EventComment = "When this projectile hits an NPC",
        EventDescriptor = new()
        {
            Host = FlowEventHost.Projectile,
            AppendCode = Append
        }
    };

    private static void Append(FlowEventGenerationContext context, IReadOnlyList<FlowEventGroup> groups)
    {
        if (groups.Count == 0)
            return;

        var sb = context.Builder;
        sb.AppendLine();
        sb.AppendLine("        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)");
        sb.AppendLine("        {");
        sb.AppendLine("            Player player = Main.player[Math.Clamp(Projectile.owner, 0, Main.maxPlayers - 1)];");
        sb.AppendLine("            NPC npc = target;");
        sb.AppendLine("            Player targetPlayer = null;");
        sb.AppendLine("            Projectile projectile = Projectile;");
        context.AppendGroupBodies(groups, 12, "player.GetSource_FromThis()", "projectile");
        sb.AppendLine("        }");
    }
}
