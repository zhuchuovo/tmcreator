namespace tmcreator.Models.Flow;

public static class ProjectileOnHitPlayerBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "projectile_on_hit_player",
        Name = "当弹幕击中玩家时",
        Category = BlockCategory.Event,
        EventComment = "When this projectile hits a player",
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
        sb.AppendLine("        public override void OnHitPlayer(Player target, Player.HurtInfo info)");
        sb.AppendLine("        {");
        sb.AppendLine("            Player player = Main.player[Math.Clamp(Projectile.owner, 0, Main.maxPlayers - 1)];");
        sb.AppendLine("            NPC npc = null;");
        sb.AppendLine("            Player targetPlayer = target;");
        sb.AppendLine("            Projectile projectile = Projectile;");
        context.AppendGroupBodies(groups, 12, "player.GetSource_FromThis()", "projectile");
        sb.AppendLine("        }");
    }
}
