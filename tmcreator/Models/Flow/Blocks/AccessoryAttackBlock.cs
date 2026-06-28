namespace tmcreator.Models.Flow;

public static class AccessoryAttackBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "accessory_attack",
        Name = "穿戴饰品攻击时",
        Category = BlockCategory.Event,
        EventComment = "When attacking while this accessory is worn",
        EventDescriptor = new()
        {
            Host = FlowEventHost.Accessory,
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
        sb.AppendLine("            if (!_equippedThisTick && !_wasEquipped)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine("            Player player = Player;");
        sb.AppendLine("            NPC npc = target;");
        sb.AppendLine("            Player targetPlayer = null;");
        context.AppendGroupBodies(groups, 12, "player.GetSource_FromThis()");
        sb.AppendLine("        }");
    }
}
