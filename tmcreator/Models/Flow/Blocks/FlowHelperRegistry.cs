namespace tmcreator.Models.Flow;

public static class FlowHelperNames
{
    public const string ForEachNpc = "Flow_ForEachNpc";
    public const string ForEachPlayer = "Flow_ForEachPlayer";
    public const string GetLife = "Flow_GetLife";
    public const string HasBuff = "Flow_HasBuff";
    public const string GetVariable = "Flow_GetVariable";
    public const string SetVariable = "Flow_SetVariable";
    public const string Compare = "Flow_Compare";
    public const string CompareFloat = "Flow_CompareFloat";
    public const string DamageNpc = "Flow_DamageNpc";
    public const string DamagePlayer = "Flow_DamagePlayer";
    public const string HealPlayer = "Flow_HealPlayer";
    public const string RestoreMana = "Flow_RestoreMana";
    public const string SetNpcValue = "Flow_SetNpcValue";
    public const string SetPlayerValue = "Flow_SetPlayerValue";
    public const string AddTemporaryPlayerStat = "Flow_AddTemporaryPlayerStat";
    public const string AddTemporaryNpcStat = "Flow_AddTemporaryNpcStat";
    public const string Broadcast = "Flow_Broadcast";
    public const string PlaySound = "Flow_PlaySound";
    public const string SetProjectileSpeed = "Flow_SetProjectileSpeed";
    public const string SpawnParticles = "Flow_SpawnParticles";
}

public sealed class FlowHelperDefinition
{
    public FlowHelperDefinition(string id, Action<System.Text.StringBuilder, FlowHelperGenerationContext> append, params string[] dependencies)
    {
        Id = id;
        Append = append;
        Dependencies = dependencies;
    }

    public string Id { get; }
    public Action<System.Text.StringBuilder, FlowHelperGenerationContext> Append { get; }
    public string[] Dependencies { get; }
}

public sealed class FlowHelperGenerationContext
{
    public FlowHelperGenerationContext(string tempStatPlayerClassName, string tempStatNpcClassName, string projectCodeName)
    {
        TempStatPlayerClassName = tempStatPlayerClassName;
        TempStatNpcClassName = tempStatNpcClassName;
        ProjectCodeName = projectCodeName;
    }

    public string TempStatPlayerClassName { get; }
    public string TempStatNpcClassName { get; }
    public string ProjectCodeName { get; }
}

public static class FlowHelperRegistry
{
    private static readonly List<FlowHelperDefinition> BuiltInDefinitions = new()
    {
        new(FlowHelperNames.ForEachNpc, AppendForEachNpc),
        new(FlowHelperNames.ForEachPlayer, AppendForEachPlayer),
        new(FlowHelperNames.GetLife, AppendGetLife),
        new(FlowHelperNames.HasBuff, AppendHasBuff, FlowHelperNames.ForEachNpc, FlowHelperNames.ForEachPlayer),
        new(FlowHelperNames.GetVariable, AppendGetVariable),
        new(FlowHelperNames.SetVariable, AppendSetVariable),
        new(FlowHelperNames.Compare, AppendCompare),
        new(FlowHelperNames.CompareFloat, AppendCompareFloat),
        new(FlowHelperNames.DamageNpc, AppendDamageNpc),
        new(FlowHelperNames.DamagePlayer, AppendDamagePlayer),
        new(FlowHelperNames.HealPlayer, AppendHealPlayer),
        new(FlowHelperNames.RestoreMana, AppendRestoreMana),
        new(FlowHelperNames.SetNpcValue, AppendSetNpcValue),
        new(FlowHelperNames.SetPlayerValue, AppendSetPlayerValue),
        new(FlowHelperNames.AddTemporaryPlayerStat, AppendAddTemporaryPlayerStat),
        new(FlowHelperNames.AddTemporaryNpcStat, AppendAddTemporaryNpcStat),
        new(FlowHelperNames.Broadcast, AppendBroadcast),
        new(FlowHelperNames.PlaySound, AppendPlaySound),
        new(FlowHelperNames.SetProjectileSpeed, AppendSetProjectileSpeed),
        new(FlowHelperNames.SpawnParticles, AppendSpawnParticles),
    };

    public static void AppendRequiredHelpers(
        System.Text.StringBuilder sb,
        IEnumerable<string> helperIds,
        IEnumerable<FlowHelperDefinition> helperDefinitions,
        string tempStatPlayerClassName,
        string tempStatNpcClassName,
        string projectCodeName)
    {
        var definitions = BuiltInDefinitions.Concat(helperDefinitions).GroupBy(definition => definition.Id).Select(group => group.Last()).ToList();
        var required = ResolveDependencies(helperIds, definitions);
        if (required.Count == 0)
            return;

        var context = new FlowHelperGenerationContext(tempStatPlayerClassName, tempStatNpcClassName, projectCodeName);
        foreach (var definition in definitions)
        {
            if (required.Contains(definition.Id))
                definition.Append(sb, context);
        }
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

    private static HashSet<string> ResolveDependencies(IEnumerable<string> helperIds, List<FlowHelperDefinition> definitions)
    {
        var byId = definitions.ToDictionary(definition => definition.Id);
        var result = new HashSet<string>();

        void Add(string id)
        {
            if (!byId.TryGetValue(id, out var definition) || !result.Add(id))
                return;

            foreach (string dependency in definition.Dependencies)
                Add(dependency);
        }

        foreach (string helperId in helperIds)
            Add(helperId);

        return result;
    }

    private static void AppendForEachNpc(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
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
    }

    private static void AppendForEachPlayer(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
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
    }

    private static void AppendGetLife(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
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
    }

    private static void AppendHasBuff(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
        sb.AppendLine();
        sb.AppendLine("        private bool Flow_HasBuff(Player player, NPC npc, Player targetPlayer, string selector, int buff)");
        sb.AppendLine("        {");
        sb.AppendLine("            bool result = false;");
        sb.AppendLine("            Flow_ForEachNpc(player, npc, targetPlayer, selector, flowNpc => result |= flowNpc.HasBuff(buff));");
        sb.AppendLine("            Flow_ForEachPlayer(player, targetPlayer, selector, flowPlayer => result |= flowPlayer.HasBuff(buff));");
        sb.AppendLine("            return result;");
        sb.AppendLine("        }");
    }

    private static void AppendGetVariable(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
        sb.AppendLine();
        sb.AppendLine("        private float Flow_GetVariable(string name)");
        sb.AppendLine("        {");
        sb.AppendLine($"            return global::{context.ProjectCodeName}.TMCreatorFlowVariables.Get(name);");
        sb.AppendLine("        }");
    }

    private static void AppendSetVariable(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
        sb.AppendLine();
        sb.AppendLine("        private void Flow_SetVariable(string name, float value)");
        sb.AppendLine("        {");
        sb.AppendLine($"            global::{context.ProjectCodeName}.TMCreatorFlowVariables.Set(name, value);");
        sb.AppendLine("        }");
    }

    private static void AppendCompare(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
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
    }

    private static void AppendCompareFloat(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
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
    }

    private static void AppendDamageNpc(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
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
    }

    private static void AppendDamagePlayer(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
        sb.AppendLine();
        sb.AppendLine("        private void Flow_DamagePlayer(Player player, Player target, int damage)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (target == null || !target.active || target.dead || damage <= 0)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine("            int hitDirection = target.Center.X >= player.Center.X ? 1 : -1;");
        sb.AppendLine("            target.Hurt(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral($\"{target.name} was hit by {player.name}.\")), damage, hitDirection, pvp: true);");
        sb.AppendLine("        }");
    }

    private static void AppendHealPlayer(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
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
    }

    private static void AppendRestoreMana(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
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
    }

    private static void AppendSetNpcValue(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
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
    }

    private static void AppendSetPlayerValue(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
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
    }

    private static void AppendAddTemporaryPlayerStat(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
        sb.AppendLine();
        sb.AppendLine("        private static void Flow_AddTemporaryPlayerStat(Player target, string stat, float amount, int durationTicks)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (target == null || !target.active || target.dead)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine($"            target.GetModPlayer<{context.TempStatPlayerClassName}>().AddModifier(stat, amount, durationTicks);");
        sb.AppendLine("        }");
    }

    private static void AppendAddTemporaryNpcStat(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
        sb.AppendLine();
        sb.AppendLine("        private static void Flow_AddTemporaryNpcStat(NPC target, string stat, float amount, int durationTicks)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (target == null || !target.active)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine($"            target.GetGlobalNPC<{context.TempStatNpcClassName}>().AddModifier(stat, amount, durationTicks);");
        sb.AppendLine("        }");
    }

    private static void AppendBroadcast(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
        sb.AppendLine();
        sb.AppendLine("        private static void Flow_Broadcast(string message)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (Main.netMode != NetmodeID.Server)");
        sb.AppendLine("                Main.NewText(message);");
        sb.AppendLine("        }");
    }

    private static void AppendPlaySound(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
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
    }

    private static void AppendSetProjectileSpeed(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
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
    }

    private static void AppendSpawnParticles(System.Text.StringBuilder sb, FlowHelperGenerationContext context)
    {
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
}
