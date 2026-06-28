namespace tmcreator.Models.Flow;

public static class ProjectileUpdateBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "projectile_update",
        Name = "当弹幕存在时",
        Category = BlockCategory.Event,
        EventComment = "While this projectile exists",
        EventDescriptor = new()
        {
            Host = FlowEventHost.Projectile,
            IsDefault = true,
            InvokeWithoutGroups = true,
            AppendCode = Append
        }
    };

    private static void Append(FlowEventGenerationContext context, IReadOnlyList<FlowEventGroup> groups)
    {
        if (!context.HasAnimation && groups.Count == 0)
            return;

        var sb = context.Builder;
        sb.AppendLine();
        sb.AppendLine("        public override void AI()");
        sb.AppendLine("        {");
        if (context.HasAnimation)
        {
            sb.AppendLine("            Projectile.frameCounter++;");
            sb.AppendLine("            if (Projectile.frameCounter >= 5)");
            sb.AppendLine("            {");
            sb.AppendLine("                Projectile.frameCounter = 0;");
            sb.AppendLine($"                Projectile.frame = (Projectile.frame + 1) % {context.FrameCount};");
            sb.AppendLine("            }");
            if (groups.Count > 0)
                sb.AppendLine();
        }
        if (groups.Count > 0)
        {
            sb.AppendLine("            Player player = Main.player[Math.Clamp(Projectile.owner, 0, Main.maxPlayers - 1)];");
            sb.AppendLine("            NPC npc = null;");
            sb.AppendLine("            Player targetPlayer = null;");
            sb.AppendLine("            Projectile projectile = Projectile;");
            context.AppendGroupBodies(groups, 12, "player.GetSource_FromThis()", "projectile");
        }
        sb.AppendLine("        }");
    }
}
