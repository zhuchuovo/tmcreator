namespace tmcreator.Models.Flow;

public static class FlowCodeGenerator
{
    public static readonly HashSet<string> BuffFlowEventIds = new()
    {
        "buff_on_gain",
        "buff_update",
        "buff_on_end"
    };

    public static readonly HashSet<string> ProjectileFlowEventIds = new()
    {
        "projectile_on_spawn",
        "projectile_update",
        "projectile_on_hit_npc",
        "projectile_on_hit_player"
    };

    public static readonly HashSet<string> AccessoryFlowEventIds = new()
    {
        "accessory_wearing",
        "accessory_attack",
        "accessory_unequip",
        "accessory_equip"
    };

    public static bool HasFlowEvents(FlowScript? flow, HashSet<string> eventIds, string defaultEventId)
    {
        if (flow == null || flow.Blocks.Count == 0)
            return false;

        return BuildFlowEventGroups(flow.Blocks, defaultEventId)
            .Any(group => eventIds.Contains(group.EventId));
    }

    public static void AppendItemFlowCode(System.Text.StringBuilder sb, FlowScript flow, string tempStatPlayerClassName, string tempStatNpcClassName, string projectCodeName)
    {
        var eventGroups = BuildFlowEventGroups(flow.Blocks, "on_use");
        if (eventGroups.Count == 0)
            return;

        sb.AppendLine();
        sb.AppendLine("        // Generated visual flow logic.");
        AppendUseItemFlow(sb, eventGroups.Where(group => group.EventId == "on_use").ToList(), projectCodeName);
        AppendHoldItemFlow(sb, eventGroups.Where(group => group.EventId == "while_held").ToList(), projectCodeName);
        AppendOnHitNpcFlow(sb, eventGroups.Where(group => group.EventId == "on_hit_npc").ToList(), projectCodeName);
        AppendOnHitPvpFlow(sb, eventGroups.Where(group => group.EventId == "on_hit_pvp").ToList(), projectCodeName);
        AppendFlowHelpers(sb, tempStatPlayerClassName, tempStatNpcClassName, projectCodeName);
    }

    public static List<FlowEventGroup> BuildFlowEventGroups(IEnumerable<BlockInstance> blocks, string defaultEventId)
    {
        var groups = new List<FlowEventGroup>();
        FlowEventGroup? current = null;

        foreach (var block in blocks)
        {
            var definition = BlockRegistry.Get(block.BlockDefId);
            if (definition?.Category == BlockCategory.Event)
            {
                current = new FlowEventGroup { EventId = block.BlockDefId, EventBlock = block };
                current.Blocks.AddRange(block.TrueBranch);
                groups.Add(current);
                continue;
            }

            if (current == null)
            {
                current = new FlowEventGroup { EventId = defaultEventId };
                groups.Add(current);
            }

            current.Blocks.Add(block);
        }

        return groups.Where(group => group.Blocks.Count > 0).ToList();
    }

    public static void AppendAccessoryFlowCode(System.Text.StringBuilder sb, FlowScript flow, string accessoryPlayerClassName, string tempStatPlayerClassName, string tempStatNpcClassName, string projectCodeName)
    {
        var eventGroups = BuildFlowEventGroups(flow.Blocks, "accessory_wearing");
        var wearingGroups = eventGroups.Where(group => group.EventId == "accessory_wearing").ToList();
        var attackGroups = eventGroups.Where(group => group.EventId == "accessory_attack").ToList();
        var unequipGroups = eventGroups.Where(group => group.EventId == "accessory_unequip").ToList();
        var equipGroups = eventGroups.Where(group => group.EventId == "accessory_equip").ToList();
        if (wearingGroups.Count == 0 && attackGroups.Count == 0 && unequipGroups.Count == 0 && equipGroups.Count == 0)
            return;

        sb.AppendLine();
        sb.AppendLine($"    public class {accessoryPlayerClassName} : ModPlayer");
        sb.AppendLine("    {");
        sb.AppendLine("        private bool _equippedThisTick;");
        sb.AppendLine("        private bool _wasEquipped;");
        sb.AppendLine();
        sb.AppendLine("        public void UpdateEquippedAccessory()");
        sb.AppendLine("        {");
        sb.AppendLine("            Player player = Player;");
        sb.AppendLine("            NPC npc = null;");
        sb.AppendLine("            Player targetPlayer = player;");
        sb.AppendLine();
        sb.AppendLine("            bool firstEquipTick = !_wasEquipped && !_equippedThisTick;");
        sb.AppendLine("            _equippedThisTick = true;");
        if (equipGroups.Count > 0)
        {
            sb.AppendLine("            if (firstEquipTick)");
            sb.AppendLine("            {");
            AppendFlowGroupBodies(sb, equipGroups, 16, "player.GetSource_FromThis()", projectCodeName: projectCodeName);
            sb.AppendLine("            }");
        }
        if (wearingGroups.Count > 0)
            AppendFlowGroupBodies(sb, wearingGroups, 12, "player.GetSource_FromThis()", projectCodeName: projectCodeName);
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        public override void ResetEffects()");
        sb.AppendLine("        {");
        sb.AppendLine("            _equippedThisTick = false;");
        sb.AppendLine("        }");

        if (attackGroups.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (!_equippedThisTick && !_wasEquipped)");
            sb.AppendLine("                return;");
            sb.AppendLine();
            sb.AppendLine("            Player player = Player;");
            sb.AppendLine("            NPC npc = target;");
            sb.AppendLine("            Player targetPlayer = null;");
            AppendFlowGroupBodies(sb, attackGroups, 12, "player.GetSource_FromThis()", projectCodeName: projectCodeName);
            sb.AppendLine("        }");
        }

        sb.AppendLine();
        sb.AppendLine("        public override void PostUpdate()");
        sb.AppendLine("        {");
        if (unequipGroups.Count > 0)
        {
            sb.AppendLine("            if (!_equippedThisTick && _wasEquipped)");
            sb.AppendLine("            {");
            sb.AppendLine("                Player player = Player;");
            sb.AppendLine("                NPC npc = null;");
            sb.AppendLine("                Player targetPlayer = player;");
            AppendFlowGroupBodies(sb, unequipGroups, 16, "player.GetSource_FromThis()", projectCodeName: projectCodeName);
            sb.AppendLine("            }");
            sb.AppendLine();
        }
        sb.AppendLine("            _wasEquipped = _equippedThisTick;");
        sb.AppendLine("        }");
        AppendFlowHelpers(sb, tempStatPlayerClassName, tempStatNpcClassName, projectCodeName);
        sb.AppendLine("    }");
    }

    public static void AppendBuffFlowCode(System.Text.StringBuilder sb, FlowScript flow, string buffClassName, string projectCodeName)
    {
        string tempStatPlayerClassName = $"{buffClassName}FlowStatsPlayer";
        string tempStatNpcClassName = $"{buffClassName}FlowStatsNpc";
        var eventGroups = BuildFlowEventGroups(flow.Blocks, "buff_update");
        var gainGroups = eventGroups.Where(group => group.EventId == "buff_on_gain").ToList();
        var updateGroups = eventGroups.Where(group => group.EventId == "buff_update").ToList();
        var endGroups = eventGroups.Where(group => group.EventId == "buff_on_end").ToList();
        if (gainGroups.Count == 0 && updateGroups.Count == 0 && endGroups.Count == 0)
            return;

        sb.AppendLine();
        sb.AppendLine($"    public class {buffClassName}Player : ModPlayer");
        sb.AppendLine("    {");
        sb.AppendLine("        private bool _hadBuff;");
        sb.AppendLine();
        sb.AppendLine("        public override void PostUpdateBuffs()");
        sb.AppendLine("        {");
        sb.AppendLine($"            bool hasBuff = Player.HasBuff(ModContent.BuffType<{buffClassName}>());");
        sb.AppendLine("            Player player = Player;");
        sb.AppendLine("            NPC npc = null;");
        sb.AppendLine("            Player targetPlayer = player;");
        sb.AppendLine();
        sb.AppendLine("            if (hasBuff)");
        sb.AppendLine("            {");
        if (gainGroups.Count > 0)
        {
            sb.AppendLine("                if (!_hadBuff)");
            sb.AppendLine("                {");
            AppendFlowGroupBodies(sb, gainGroups, 20, "player.GetSource_FromThis()", projectCodeName: projectCodeName);
            sb.AppendLine("                }");
        }
        if (updateGroups.Count > 0)
            AppendFlowGroupBodies(sb, updateGroups, 16, "player.GetSource_FromThis()", projectCodeName: projectCodeName);
        sb.AppendLine("            }");

        if (endGroups.Count > 0)
        {
            sb.AppendLine("            else if (_hadBuff)");
            sb.AppendLine("            {");
            AppendFlowGroupBodies(sb, endGroups, 16, "player.GetSource_FromThis()", projectCodeName: projectCodeName);
            sb.AppendLine("            }");
        }

        sb.AppendLine();
        sb.AppendLine("            _hadBuff = hasBuff;");
        sb.AppendLine("        }");
        AppendFlowHelpers(sb, tempStatPlayerClassName, tempStatNpcClassName, projectCodeName);
        sb.AppendLine("    }");

        AppendFlowTempStatsPlayerClass(sb, tempStatPlayerClassName);
        AppendFlowTempStatsNpcClass(sb, tempStatNpcClassName);
    }

    public static void AppendProjectileFlowCode(System.Text.StringBuilder sb, FlowScript? flow, bool hasAnimation, int frameCount, string tempStatPlayerClassName, string tempStatNpcClassName, string projectCodeName)
    {
        var eventGroups = flow == null
            ? new List<FlowEventGroup>()
            : BuildFlowEventGroups(flow.Blocks, "projectile_update");
        var spawnGroups = eventGroups.Where(group => group.EventId == "projectile_on_spawn").ToList();
        var updateGroups = eventGroups.Where(group => group.EventId == "projectile_update").ToList();
        var hitNpcGroups = eventGroups.Where(group => group.EventId == "projectile_on_hit_npc").ToList();
        var hitPlayerGroups = eventGroups.Where(group => group.EventId == "projectile_on_hit_player").ToList();

        if (spawnGroups.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("        public override void OnSpawn(IEntitySource source)");
            sb.AppendLine("        {");
            sb.AppendLine("            Player player = Main.player[Math.Clamp(Projectile.owner, 0, Main.maxPlayers - 1)];");
            sb.AppendLine("            NPC npc = null;");
            sb.AppendLine("            Player targetPlayer = null;");
            sb.AppendLine("            Projectile projectile = Projectile;");
            AppendFlowGroupBodies(sb, spawnGroups, 12, "player.GetSource_FromThis()", "projectile", projectCodeName);
            sb.AppendLine("        }");
        }

        if (hasAnimation || updateGroups.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("        public override void AI()");
            sb.AppendLine("        {");
            if (hasAnimation)
            {
                sb.AppendLine("            Projectile.frameCounter++;");
                sb.AppendLine("            if (Projectile.frameCounter >= 5)");
                sb.AppendLine("            {");
                sb.AppendLine("                Projectile.frameCounter = 0;");
                sb.AppendLine($"                Projectile.frame = (Projectile.frame + 1) % {frameCount};");
                sb.AppendLine("            }");
                if (updateGroups.Count > 0)
                    sb.AppendLine();
            }
            if (updateGroups.Count > 0)
            {
                sb.AppendLine("            Player player = Main.player[Math.Clamp(Projectile.owner, 0, Main.maxPlayers - 1)];");
                sb.AppendLine("            NPC npc = null;");
                sb.AppendLine("            Player targetPlayer = null;");
                sb.AppendLine("            Projectile projectile = Projectile;");
                AppendFlowGroupBodies(sb, updateGroups, 12, "player.GetSource_FromThis()", "projectile", projectCodeName);
            }
            sb.AppendLine("        }");
        }

        if (hitNpcGroups.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)");
            sb.AppendLine("        {");
            sb.AppendLine("            Player player = Main.player[Math.Clamp(Projectile.owner, 0, Main.maxPlayers - 1)];");
            sb.AppendLine("            NPC npc = target;");
            sb.AppendLine("            Player targetPlayer = null;");
            sb.AppendLine("            Projectile projectile = Projectile;");
            AppendFlowGroupBodies(sb, hitNpcGroups, 12, "player.GetSource_FromThis()", "projectile", projectCodeName);
            sb.AppendLine("        }");
        }

        if (hitPlayerGroups.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("        public override void OnHitPlayer(Player target, Player.HurtInfo info)");
            sb.AppendLine("        {");
            sb.AppendLine("            Player player = Main.player[Math.Clamp(Projectile.owner, 0, Main.maxPlayers - 1)];");
            sb.AppendLine("            NPC npc = null;");
            sb.AppendLine("            Player targetPlayer = target;");
            sb.AppendLine("            Projectile projectile = Projectile;");
            AppendFlowGroupBodies(sb, hitPlayerGroups, 12, "player.GetSource_FromThis()", "projectile", projectCodeName);
            sb.AppendLine("        }");
        }

        if (eventGroups.Count > 0)
            AppendFlowHelpers(sb, tempStatPlayerClassName, tempStatNpcClassName, projectCodeName);
    }

    internal static void AppendFlowStatements(System.Text.StringBuilder sb, IEnumerable<BlockInstance> blocks, int indent, FlowGenerationContext context)
    {
        foreach (var block in blocks)
        {
            var definition = BlockRegistry.Get(block.BlockDefId);
            if (definition == null)
            {
                FlowCodeUtility.AppendLine(sb, indent, $"// Unknown flow block: {FlowCodeUtility.EscapeComment(block.BlockDefId)}");
                continue;
            }

            switch (definition.Category)
            {
                case BlockCategory.Action:
                    if (definition.AppendStatement != null)
                        definition.AppendStatement(context, block, indent);
                    else
                        FlowCodeUtility.AppendLine(sb, indent, $"// Unsupported action block: {FlowCodeUtility.EscapeComment(block.BlockDefId)}");
                    break;
                case BlockCategory.Condition:
                    AppendConditionStatement(sb, block, indent, context, definition);
                    break;
                case BlockCategory.Value:
                    FlowCodeUtility.AppendLine(sb, indent, $"// Value block \"{FlowCodeUtility.EscapeComment(definition.Name)}\" is only used when dropped into a numeric parameter slot.");
                    break;
                case BlockCategory.Event:
                    FlowCodeUtility.AppendLine(sb, indent, $"// Nested event \"{FlowCodeUtility.EscapeComment(definition.Name)}\" is ignored. Place event blocks at the top level.");
                    break;
            }
        }
    }

    private static void AppendFlowGroupBodies(System.Text.StringBuilder sb, IEnumerable<FlowEventGroup> groups, int indent, string sourceExpression = "player.GetSource_ItemUse(Item)", string projectileExpression = "null", string projectCodeName = "")
    {
        var context = new FlowGenerationContext(sb, sourceExpression, projectileExpression, projectCodeName);
        foreach (var group in groups)
        {
            FlowCodeUtility.AppendLine(sb, indent, $"// {GetFlowEventComment(group.EventId)}");
            AppendFlowStatements(sb, group.Blocks, indent, context);
        }
    }

    private static void AppendUseItemFlow(System.Text.StringBuilder sb, List<FlowEventGroup> groups, string projectCodeName)
    {
        if (groups.Count == 0)
            return;

        sb.AppendLine();
        sb.AppendLine("        public override bool? UseItem(Player player)");
        sb.AppendLine("        {");
        sb.AppendLine("            NPC npc = null;");
        sb.AppendLine("            Player targetPlayer = null;");
        AppendFlowGroupBodies(sb, groups, 12, projectCodeName: projectCodeName);
        sb.AppendLine("            return true;");
        sb.AppendLine("        }");
    }

    private static void AppendHoldItemFlow(System.Text.StringBuilder sb, List<FlowEventGroup> groups, string projectCodeName)
    {
        if (groups.Count == 0)
            return;

        sb.AppendLine();
        sb.AppendLine("        public override void HoldItem(Player player)");
        sb.AppendLine("        {");
        sb.AppendLine("            NPC npc = null;");
        sb.AppendLine("            Player targetPlayer = null;");
        AppendFlowGroupBodies(sb, groups, 12, projectCodeName: projectCodeName);
        sb.AppendLine("        }");
    }

    private static void AppendOnHitNpcFlow(System.Text.StringBuilder sb, List<FlowEventGroup> groups, string projectCodeName)
    {
        if (groups.Count == 0)
            return;

        sb.AppendLine();
        sb.AppendLine("        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)");
        sb.AppendLine("        {");
        sb.AppendLine("            NPC npc = target;");
        sb.AppendLine("            Player targetPlayer = null;");
        AppendFlowGroupBodies(sb, groups, 12, projectCodeName: projectCodeName);
        sb.AppendLine("        }");
    }

    private static void AppendOnHitPvpFlow(System.Text.StringBuilder sb, List<FlowEventGroup> groups, string projectCodeName)
    {
        if (groups.Count == 0)
            return;

        sb.AppendLine();
        sb.AppendLine("        public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)");
        sb.AppendLine("        {");
        sb.AppendLine("            NPC npc = null;");
        sb.AppendLine("            Player targetPlayer = target;");
        AppendFlowGroupBodies(sb, groups, 12, projectCodeName: projectCodeName);
        sb.AppendLine("        }");
    }

    private static void AppendConditionStatement(System.Text.StringBuilder sb, BlockInstance block, int indent, FlowGenerationContext context, BlockDefinition definition)
    {
        string condition = definition.BuildCondition?.Invoke(context, block) ?? "false";

        FlowCodeUtility.AppendLine(sb, indent, $"if ({condition})");
        FlowCodeUtility.AppendLine(sb, indent, "{");
        AppendFlowStatements(sb, block.TrueBranch, indent + 4, context);
        FlowCodeUtility.AppendLine(sb, indent, "}");

        if (block.FalseBranch.Count > 0)
        {
            FlowCodeUtility.AppendLine(sb, indent, "else");
            FlowCodeUtility.AppendLine(sb, indent, "{");
            AppendFlowStatements(sb, block.FalseBranch, indent + 4, context);
            FlowCodeUtility.AppendLine(sb, indent, "}");
        }
    }

    private static string GetFlowEventComment(string eventId)
    {
        var definition = BlockRegistry.Get(eventId);
        return string.IsNullOrWhiteSpace(definition?.EventComment)
            ? "When this item is used"
            : definition.EventComment;
    }

    private static void AppendFlowHelpers(System.Text.StringBuilder sb, string tempStatPlayerClassName, string tempStatNpcClassName, string projectCodeName)
    {
        sb.AppendLine();
        sb.AppendLine("        private void Flow_ForEachNpc(Player player, NPC npc, Player targetPlayer, string selector, Action<NPC> action)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (selector == \"npc\")");
        sb.AppendLine("            {");
        sb.AppendLine("                if (npc != null && npc.active)");
        sb.AppendLine("                    action(npc);");
        sb.AppendLine("                return;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            if (selector == \"all_npc\")");
        sb.AppendLine("            {");
        sb.AppendLine("                for (int i = 0; i < Main.maxNPCs; i++)");
        sb.AppendLine("                {");
        sb.AppendLine("                    NPC candidate = Main.npc[i];");
        sb.AppendLine("                    if (candidate.active && !candidate.friendly && candidate.life > 0)");
        sb.AppendLine("                        action(candidate);");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private void Flow_ForEachPlayer(Player player, Player targetPlayer, string selector, Action<Player> action)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (selector == \"owner\")");
        sb.AppendLine("            {");
        sb.AppendLine("                action(player);");
        sb.AppendLine("                return;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            if (selector == \"player\")");
        sb.AppendLine("            {");
        sb.AppendLine("                action(targetPlayer ?? player);");
        sb.AppendLine("                return;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            if (selector == \"all_player\")");
        sb.AppendLine("            {");
        sb.AppendLine("                for (int i = 0; i < Main.maxPlayers; i++)");
        sb.AppendLine("                {");
        sb.AppendLine("                    Player candidate = Main.player[i];");
        sb.AppendLine("                    if (candidate.active && !candidate.dead)");
        sb.AppendLine("                        action(candidate);");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private int Flow_GetLife(Player player, NPC npc, Player targetPlayer, string selector)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (selector == \"npc\")");
        sb.AppendLine("                return npc != null && npc.active ? npc.life : 0;");
        sb.AppendLine("            if (selector == \"owner\")");
        sb.AppendLine("                return player.statLife;");
        sb.AppendLine("            if (selector == \"player\")");
        sb.AppendLine("                return (targetPlayer ?? player).statLife;");
        sb.AppendLine("            if (selector == \"all_npc\")");
        sb.AppendLine("            {");
        sb.AppendLine("                int total = 0;");
        sb.AppendLine("                for (int i = 0; i < Main.maxNPCs; i++)");
        sb.AppendLine("                {");
        sb.AppendLine("                    NPC candidate = Main.npc[i];");
        sb.AppendLine("                    if (candidate.active && !candidate.friendly && candidate.life > 0)");
        sb.AppendLine("                        total += candidate.life;");
        sb.AppendLine("                }");
        sb.AppendLine("                return total;");
        sb.AppendLine("            }");
        sb.AppendLine("            if (selector == \"all_player\")");
        sb.AppendLine("            {");
        sb.AppendLine("                int total = 0;");
        sb.AppendLine("                for (int i = 0; i < Main.maxPlayers; i++)");
        sb.AppendLine("                {");
        sb.AppendLine("                    Player candidate = Main.player[i];");
        sb.AppendLine("                    if (candidate.active && !candidate.dead)");
        sb.AppendLine("                        total += candidate.statLife;");
        sb.AppendLine("                }");
        sb.AppendLine("                return total;");
        sb.AppendLine("            }");
        sb.AppendLine("            return 0;");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private bool Flow_HasBuff(Player player, NPC npc, Player targetPlayer, string selector, int buff)");
        sb.AppendLine("        {");
        sb.AppendLine("            bool result = false;");
        sb.AppendLine("            Flow_ForEachNpc(player, npc, targetPlayer, selector, flowNpc => result |= flowNpc.HasBuff(buff));");
        sb.AppendLine("            Flow_ForEachPlayer(player, targetPlayer, selector, flowPlayer => result |= flowPlayer.HasBuff(buff));");
        sb.AppendLine("            return result;");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private float Flow_GetVariable(string name)");
        sb.AppendLine("        {");
        sb.AppendLine($"            return global::{projectCodeName}.TMCreatorFlowVariables.Get(name);");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private void Flow_SetVariable(string name, float value)");
        sb.AppendLine("        {");
        sb.AppendLine($"            global::{projectCodeName}.TMCreatorFlowVariables.Set(name, value);");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private static bool Flow_Compare(int left, string op, int right)");
        sb.AppendLine("        {");
        sb.AppendLine("            return op switch");
        sb.AppendLine("            {");
        sb.AppendLine("                \">\" => left > right,");
        sb.AppendLine("                \"=\" => left == right,");
        sb.AppendLine("                \">=\" or \"≥\" => left >= right,");
        sb.AppendLine("                \"<=\" or \"≤\" => left <= right,");
        sb.AppendLine("                _ => left < right");
        sb.AppendLine("            };");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private static bool Flow_CompareFloat(float left, string op, float right)");
        sb.AppendLine("        {");
        sb.AppendLine("            return op switch");
        sb.AppendLine("            {");
        sb.AppendLine("                \">\" => left > right,");
        sb.AppendLine("                \"=\" => Math.Abs(left - right) <= 0.0001f,");
        sb.AppendLine("                \"!=\" => Math.Abs(left - right) > 0.0001f,");
        sb.AppendLine("                \">=\" => left >= right,");
        sb.AppendLine("                \"<=\" => left <= right,");
        sb.AppendLine("                _ => left < right");
        sb.AppendLine("            };");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private void Flow_DamageNpc(Player player, NPC target, int damage)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (target == null || !target.active || damage <= 0)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine("            int hitDirection = target.Center.X >= player.Center.X ? 1 : -1;");
        sb.AppendLine("            var hitInfo = new NPC.HitInfo");
        sb.AppendLine("            {");
        sb.AppendLine("                Damage = damage,");
        sb.AppendLine("                Knockback = 0f,");
        sb.AppendLine("                HitDirection = hitDirection");
        sb.AppendLine("            };");
        sb.AppendLine("            target.StrikeNPC(hitInfo);");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private void Flow_DamagePlayer(Player player, Player target, int damage)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (target == null || !target.active || target.dead || damage <= 0)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine("            int hitDirection = target.Center.X >= player.Center.X ? 1 : -1;");
        sb.AppendLine("            target.Hurt(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral($\"{target.name} was hit by {player.name}.\")), damage, hitDirection, pvp: true);");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private static void Flow_HealPlayer(Player player, int amount)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (amount <= 0)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine("            int before = player.statLife;");
        sb.AppendLine("            player.statLife = Math.Min(player.statLifeMax2, player.statLife + amount);");
        sb.AppendLine("            player.HealEffect(player.statLife - before);");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private static void Flow_RestoreMana(Player player, int amount)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (player == null || !player.active || player.dead || amount <= 0)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine("            int before = player.statMana;");
        sb.AppendLine("            player.statMana = Math.Min(player.statManaMax2, player.statMana + amount);");
        sb.AppendLine("            player.ManaEffect(player.statMana - before);");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private static void Flow_SetNpcValue(NPC target, string stat, int value)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (target == null || !target.active)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine("            switch (stat)");
        sb.AppendLine("            {");
        sb.AppendLine("                case \"life\": target.life = Math.Clamp(value, 0, target.lifeMax); break;");
        sb.AppendLine("                case \"damage\": target.damage = Math.Max(0, value); break;");
        sb.AppendLine("                case \"defense\": target.defense = Math.Max(0, value); break;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private static void Flow_SetPlayerValue(Player target, string stat, int value)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (target == null || !target.active)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine("            switch (stat)");
        sb.AppendLine("            {");
        sb.AppendLine("                case \"life\": target.statLife = Math.Clamp(value, 0, target.statLifeMax2); break;");
        sb.AppendLine("                case \"damage\": break;");
        sb.AppendLine("                case \"defense\": target.statDefense = Player.DefenseStat.Default + Math.Max(0, value); break;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private static void Flow_AddTemporaryPlayerStat(Player target, string stat, float amount, int durationTicks)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (target == null || !target.active || target.dead)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine($"            target.GetModPlayer<{tempStatPlayerClassName}>().AddModifier(stat, amount, durationTicks);");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private static void Flow_AddTemporaryNpcStat(NPC target, string stat, float amount, int durationTicks)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (target == null || !target.active)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine($"            target.GetGlobalNPC<{tempStatNpcClassName}>().AddModifier(stat, amount, durationTicks);");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private static void Flow_Broadcast(string message)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (Main.netMode != NetmodeID.Server)");
        sb.AppendLine("                Main.NewText(message);");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private static void Flow_PlaySound(Player player, int soundId)");
        sb.AppendLine("        {");
        sb.AppendLine("            switch (soundId)");
        sb.AppendLine("            {");
        sb.AppendLine("                case 2: SoundEngine.PlaySound(SoundID.Item2, player.Center); break;");
        sb.AppendLine("                case 3: SoundEngine.PlaySound(SoundID.Item3, player.Center); break;");
        sb.AppendLine("                case 4: SoundEngine.PlaySound(SoundID.Item4, player.Center); break;");
        sb.AppendLine("                case 5: SoundEngine.PlaySound(SoundID.Item5, player.Center); break;");
        sb.AppendLine("                default: SoundEngine.PlaySound(SoundID.Item1, player.Center); break;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private static void Flow_SetProjectileSpeed(Projectile projectile, Player player, float speed)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (projectile == null || !projectile.active)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine("            Vector2 direction = projectile.velocity;");
        sb.AppendLine("            if (direction.LengthSquared() < 0.001f)");
        sb.AppendLine("                direction = new Vector2(player != null && player.direction < 0 ? -1f : 1f, 0f);");
        sb.AppendLine("            direction.Normalize();");
        sb.AppendLine("            projectile.velocity = direction * Math.Max(0f, speed);");
        sb.AppendLine("        }");

        sb.AppendLine();
        sb.AppendLine("        private static void Flow_SpawnParticles(Vector2 center, int dustId, int count, float speed)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (Main.netMode == NetmodeID.Server)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine("            count = Math.Clamp(count, 1, 200);");
        sb.AppendLine("            speed = Math.Max(0f, speed);");
        sb.AppendLine("            for (int i = 0; i < count; i++)");
        sb.AppendLine("            {");
        sb.AppendLine("                float angle = (float)(Main.rand.NextDouble() * MathHelper.TwoPi);");
        sb.AppendLine("                float magnitude = (float)Main.rand.NextDouble() * speed;");
        sb.AppendLine("                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * magnitude;");
        sb.AppendLine("                Dust.NewDustPerfect(center, dustId, velocity, 150, Color.White, 1f);");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
    }

    public static void AppendFlowTempStatsPlayerClass(System.Text.StringBuilder sb, string className)
    {
        sb.AppendLine();
        sb.AppendLine($"    public class {className} : ModPlayer");
        sb.AppendLine("    {");
        sb.AppendLine("        private readonly System.Collections.Generic.List<FlowTimedStatModifier> _modifiers = new();");
        sb.AppendLine();
        sb.AppendLine("        public void AddModifier(string stat, float amount, int durationTicks)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (string.IsNullOrWhiteSpace(stat) || Math.Abs(amount) <= 0.0001f)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine("            for (int i = 0; i < _modifiers.Count; i++)");
        sb.AppendLine("            {");
        sb.AppendLine("                FlowTimedStatModifier existing = _modifiers[i];");
        sb.AppendLine("                if (existing.Stat == stat && Math.Abs(existing.Amount - amount) <= 0.0001f)");
        sb.AppendLine("                {");
        sb.AppendLine("                    existing.TimeLeft = Math.Max(existing.TimeLeft, Math.Max(1, durationTicks));");
        sb.AppendLine("                    _modifiers[i] = existing;");
        sb.AppendLine("                    return;");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            _modifiers.Add(new FlowTimedStatModifier");
        sb.AppendLine("            {");
        sb.AppendLine("                Stat = stat,");
        sb.AppendLine("                Amount = amount,");
        sb.AppendLine("                TimeLeft = Math.Max(1, durationTicks)");
        sb.AppendLine("            });");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        public override void ResetEffects()");
        sb.AppendLine("        {");
        sb.AppendLine("            for (int i = _modifiers.Count - 1; i >= 0; i--)");
        sb.AppendLine("            {");
        sb.AppendLine("                FlowTimedStatModifier modifier = _modifiers[i];");
        sb.AppendLine("                ApplyModifier(modifier.Stat, modifier.Amount);");
        sb.AppendLine("                modifier.TimeLeft--;");
        sb.AppendLine();
        sb.AppendLine("                if (modifier.TimeLeft <= 0)");
        sb.AppendLine("                    _modifiers.RemoveAt(i);");
        sb.AppendLine("                else");
        sb.AppendLine("                    _modifiers[i] = modifier;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        private void ApplyModifier(string stat, float amount)");
        sb.AppendLine("        {");
        sb.AppendLine("            switch (stat)");
        sb.AppendLine("            {");
        sb.AppendLine("                case \"defense\": Player.statDefense += (int)Math.Round(amount); break;");
        sb.AppendLine("                case \"damage_reduction\": Player.endurance += amount / 100f; break;");
        sb.AppendLine("                case \"melee_damage\": Player.GetDamage(DamageClass.Melee) += amount / 100f; break;");
        sb.AppendLine("                case \"magic_damage\": Player.GetDamage(DamageClass.Magic) += amount / 100f; break;");
        sb.AppendLine("                case \"ranged_damage\": Player.GetDamage(DamageClass.Ranged) += amount / 100f; break;");
        sb.AppendLine("                case \"summon_damage\": Player.GetDamage(DamageClass.Summon) += amount / 100f; break;");
        sb.AppendLine("                case \"generic_damage\": Player.GetDamage(DamageClass.Generic) += amount / 100f; break;");
        sb.AppendLine("                case \"melee_crit\": Player.GetCritChance(DamageClass.Melee) += amount; break;");
        sb.AppendLine("                case \"magic_crit\": Player.GetCritChance(DamageClass.Magic) += amount; break;");
        sb.AppendLine("                case \"ranged_crit\": Player.GetCritChance(DamageClass.Ranged) += amount; break;");
        sb.AppendLine("                case \"summon_crit\": Player.GetCritChance(DamageClass.Summon) += amount; break;");
        sb.AppendLine("                case \"generic_crit\": Player.GetCritChance(DamageClass.Generic) += amount; break;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        private struct FlowTimedStatModifier");
        sb.AppendLine("        {");
        sb.AppendLine("            public string Stat;");
        sb.AppendLine("            public float Amount;");
        sb.AppendLine("            public int TimeLeft;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
    }

    public static void AppendFlowTempStatsNpcClass(System.Text.StringBuilder sb, string className)
    {
        sb.AppendLine();
        sb.AppendLine($"    public class {className} : GlobalNPC");
        sb.AppendLine("    {");
        sb.AppendLine("        private readonly System.Collections.Generic.List<FlowTimedStatModifier> _modifiers = new();");
        sb.AppendLine();
        sb.AppendLine("        public override bool InstancePerEntity => true;");
        sb.AppendLine();
        sb.AppendLine("        public void AddModifier(string stat, float amount, int durationTicks)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (string.IsNullOrWhiteSpace(stat) || Math.Abs(amount) <= 0.0001f)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine("            for (int i = 0; i < _modifiers.Count; i++)");
        sb.AppendLine("            {");
        sb.AppendLine("                FlowTimedStatModifier existing = _modifiers[i];");
        sb.AppendLine("                if (existing.Stat == stat && Math.Abs(existing.Amount - amount) <= 0.0001f)");
        sb.AppendLine("                {");
        sb.AppendLine("                    existing.TimeLeft = Math.Max(existing.TimeLeft, Math.Max(1, durationTicks));");
        sb.AppendLine("                    _modifiers[i] = existing;");
        sb.AppendLine("                    return;");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            _modifiers.Add(new FlowTimedStatModifier");
        sb.AppendLine("            {");
        sb.AppendLine("                Stat = stat,");
        sb.AppendLine("                Amount = amount,");
        sb.AppendLine("                TimeLeft = Math.Max(1, durationTicks)");
        sb.AppendLine("            });");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        public override void ResetEffects(NPC npc)");
        sb.AppendLine("        {");
        sb.AppendLine("            for (int i = _modifiers.Count - 1; i >= 0; i--)");
        sb.AppendLine("            {");
        sb.AppendLine("                FlowTimedStatModifier modifier = _modifiers[i];");
        sb.AppendLine("                ApplyModifier(npc, modifier.Stat, modifier.Amount);");
        sb.AppendLine("                modifier.TimeLeft--;");
        sb.AppendLine();
        sb.AppendLine("                if (modifier.TimeLeft <= 0)");
        sb.AppendLine("                    _modifiers.RemoveAt(i);");
        sb.AppendLine("                else");
        sb.AppendLine("                    _modifiers[i] = modifier;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        private static void ApplyModifier(NPC npc, string stat, float amount)");
        sb.AppendLine("        {");
        sb.AppendLine("            switch (stat)");
        sb.AppendLine("            {");
        sb.AppendLine("                case \"defense\": npc.defense += (int)Math.Round(amount); break;");
        sb.AppendLine("                case \"melee_damage\":");
        sb.AppendLine("                case \"magic_damage\":");
        sb.AppendLine("                case \"ranged_damage\":");
        sb.AppendLine("                case \"summon_damage\":");
        sb.AppendLine("                case \"generic_damage\":");
        sb.AppendLine("                    npc.damage = Math.Max(0, npc.damage + (int)Math.Round(amount));");
        sb.AppendLine("                    break;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        private struct FlowTimedStatModifier");
        sb.AppendLine("        {");
        sb.AppendLine("            public string Stat;");
        sb.AppendLine("            public float Amount;");
        sb.AppendLine("            public int TimeLeft;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
    }
}
