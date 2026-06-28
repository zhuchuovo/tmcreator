namespace tmcreator.Models.Flow;

public static class ProjectileOnSpawnBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "projectile_on_spawn",
        Name = "当弹幕发射时",
        Category = BlockCategory.Event,
        EventComment = "When this projectile is fired",
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
        sb.AppendLine("        public override void OnSpawn(IEntitySource source)");
        sb.AppendLine("        {");
        sb.AppendLine("            Player player = Main.player[Math.Clamp(Projectile.owner, 0, Main.maxPlayers - 1)];");
        sb.AppendLine("            NPC npc = null;");
        sb.AppendLine("            Player targetPlayer = null;");
        sb.AppendLine("            Projectile projectile = Projectile;");
        context.AppendGroupBodies(groups, 12, "player.GetSource_FromThis()", "projectile");
        sb.AppendLine("        }");
    }
}
