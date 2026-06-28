namespace tmcreator.Models.Flow;

public static class FlowCodeGenerator
{
    public static HashSet<string> BuffFlowEventIds => BlockRegistry.GetEventIds(FlowEventHost.Buff);
    public static HashSet<string> ProjectileFlowEventIds => BlockRegistry.GetEventIds(FlowEventHost.Projectile);
    public static HashSet<string> AccessoryFlowEventIds => BlockRegistry.GetEventIds(FlowEventHost.Accessory);
    public static string BuffDefaultEventId => BlockRegistry.GetDefaultEventId(FlowEventHost.Buff);
    public static string ProjectileDefaultEventId => BlockRegistry.GetDefaultEventId(FlowEventHost.Projectile);
    public static string AccessoryDefaultEventId => BlockRegistry.GetDefaultEventId(FlowEventHost.Accessory);

    public static bool HasFlowEvents(FlowScript? flow, HashSet<string> eventIds, string defaultEventId)
    {
        if (flow == null || flow.Blocks.Count == 0)
            return false;

        return BuildFlowEventGroups(flow.Blocks, defaultEventId)
            .Any(group => eventIds.Contains(group.EventId));
    }

    public static void AppendItemFlowCode(System.Text.StringBuilder sb, FlowScript flow, string tempStatPlayerClassName, string tempStatNpcClassName, string projectCodeName)
    {
        var groups = BuildFlowEventGroups(flow.Blocks, BlockRegistry.GetDefaultEventId(FlowEventHost.Item));
        if (groups.Count == 0)
            return;

        var context = new FlowEventGenerationContext(sb, projectCodeName, tempStatPlayerClassName, tempStatNpcClassName);
        sb.AppendLine();
        sb.AppendLine("        // Generated visual flow logic.");
        AppendEventBlocks(context, FlowEventHost.Item, groups, descriptor => descriptor.AppendCode);
        AppendRequiredHelpers(sb, groups, tempStatPlayerClassName, tempStatNpcClassName, projectCodeName);
    }

    public static void AppendAccessoryFlowCode(System.Text.StringBuilder sb, FlowScript flow, string accessoryPlayerClassName, string tempStatPlayerClassName, string tempStatNpcClassName, string projectCodeName)
    {
        var groups = BuildFlowEventGroups(flow.Blocks, BlockRegistry.GetDefaultEventId(FlowEventHost.Accessory));
        if (groups.Count == 0)
            return;

        var context = new FlowEventGenerationContext(
            sb,
            projectCodeName,
            tempStatPlayerClassName,
            tempStatNpcClassName,
            accessoryPlayerClassName);

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
        AppendEventBlocks(context, FlowEventHost.Accessory, groups, descriptor => descriptor.AppendAccessoryUpdateCode);
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        public override void ResetEffects()");
        sb.AppendLine("        {");
        sb.AppendLine("            _equippedThisTick = false;");
        sb.AppendLine("        }");

        AppendEventBlocks(context, FlowEventHost.Accessory, groups, descriptor => descriptor.AppendCode);

        sb.AppendLine();
        sb.AppendLine("        public override void PostUpdate()");
        sb.AppendLine("        {");
        AppendEventBlocks(context, FlowEventHost.Accessory, groups, descriptor => descriptor.AppendAccessoryPostUpdateCode);
        sb.AppendLine("            _wasEquipped = _equippedThisTick;");
        sb.AppendLine("        }");
        AppendRequiredHelpers(sb, groups, tempStatPlayerClassName, tempStatNpcClassName, projectCodeName);
        sb.AppendLine("    }");
    }

    public static void AppendBuffFlowCode(System.Text.StringBuilder sb, FlowScript flow, string buffClassName, string projectCodeName)
    {
        string tempStatPlayerClassName = $"{buffClassName}FlowStatsPlayer";
        string tempStatNpcClassName = $"{buffClassName}FlowStatsNpc";
        var groups = BuildFlowEventGroups(flow.Blocks, BlockRegistry.GetDefaultEventId(FlowEventHost.Buff));
        if (groups.Count == 0)
            return;

        var context = new FlowEventGenerationContext(
            sb,
            projectCodeName,
            tempStatPlayerClassName,
            tempStatNpcClassName,
            buffClassName: buffClassName);

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
        AppendEventBlocks(context, FlowEventHost.Buff, groups, descriptor => descriptor.AppendBuffActiveCode);
        sb.AppendLine("            }");

        if (HasEventBlock(groups, FlowEventHost.Buff, descriptor => descriptor.AppendBuffInactiveCode))
        {
            sb.AppendLine("            else if (_hadBuff)");
            sb.AppendLine("            {");
            AppendEventBlocks(context, FlowEventHost.Buff, groups, descriptor => descriptor.AppendBuffInactiveCode);
            sb.AppendLine("            }");
        }

        sb.AppendLine();
        sb.AppendLine("            _hadBuff = hasBuff;");
        sb.AppendLine("        }");
        AppendRequiredHelpers(sb, groups, tempStatPlayerClassName, tempStatNpcClassName, projectCodeName);
        sb.AppendLine("    }");

        AppendFlowTempStatsPlayerClass(sb, tempStatPlayerClassName);
        AppendFlowTempStatsNpcClass(sb, tempStatNpcClassName);
    }

    public static void AppendProjectileFlowCode(System.Text.StringBuilder sb, FlowScript? flow, bool hasAnimation, int frameCount, string tempStatPlayerClassName, string tempStatNpcClassName, string projectCodeName)
    {
        var groups = flow == null
            ? new List<FlowEventGroup>()
            : BuildFlowEventGroups(flow.Blocks, BlockRegistry.GetDefaultEventId(FlowEventHost.Projectile));
        if (groups.Count == 0 && !hasAnimation)
            return;

        var context = new FlowEventGenerationContext(
            sb,
            projectCodeName,
            tempStatPlayerClassName,
            tempStatNpcClassName,
            hasAnimation: hasAnimation,
            frameCount: frameCount);

        AppendEventBlocks(context, FlowEventHost.Projectile, groups, descriptor => descriptor.AppendCode, includeEmptyDescriptors: hasAnimation);
        if (groups.Count > 0)
            AppendRequiredHelpers(sb, groups, tempStatPlayerClassName, tempStatNpcClassName, projectCodeName);
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

    public static void AppendFlowTempStatsPlayerClass(System.Text.StringBuilder sb, string className)
    {
        FlowHelperRegistry.AppendFlowTempStatsPlayerClass(sb, className);
    }

    public static void AppendFlowTempStatsNpcClass(System.Text.StringBuilder sb, string className)
    {
        FlowHelperRegistry.AppendFlowTempStatsNpcClass(sb, className);
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

    private static void AppendEventBlocks(
        FlowEventGenerationContext context,
        FlowEventHost host,
        IReadOnlyList<FlowEventGroup> groups,
        Func<FlowEventDescriptor, Action<FlowEventGenerationContext, IReadOnlyList<FlowEventGroup>>?> getAppender,
        bool includeEmptyDescriptors = false)
    {
        foreach (var definition in BlockRegistry.GetEventsByHost(host))
        {
            var descriptor = definition.EventDescriptor;
            var appender = descriptor == null ? null : getAppender(descriptor);
            if (descriptor == null || appender == null)
                continue;

            var eventGroups = groups.Where(group => group.EventId == definition.Id).ToList();
            if (eventGroups.Count == 0 && !(includeEmptyDescriptors && descriptor.InvokeWithoutGroups))
                continue;

            appender(context, eventGroups);
        }
    }

    private static bool HasEventBlock(
        IReadOnlyList<FlowEventGroup> groups,
        FlowEventHost host,
        Func<FlowEventDescriptor, Delegate?> getAppender)
    {
        foreach (var definition in BlockRegistry.GetEventsByHost(host))
        {
            var descriptor = definition.EventDescriptor;
            if (descriptor == null || getAppender(descriptor) == null)
                continue;

            if (groups.Any(group => group.EventId == definition.Id))
                return true;
        }

        return false;
    }

    private static void AppendRequiredHelpers(
        System.Text.StringBuilder sb,
        IReadOnlyList<FlowEventGroup> groups,
        string tempStatPlayerClassName,
        string tempStatNpcClassName,
        string projectCodeName)
    {
        var helpers = CollectRequiredHelpers(groups.SelectMany(group => group.Blocks));
        FlowHelperRegistry.AppendRequiredHelpers(
            sb,
            helpers.Required,
            helpers.Provided,
            tempStatPlayerClassName,
            tempStatNpcClassName,
            projectCodeName);
    }

    private sealed class FlowHelperCollection
    {
        public HashSet<string> Required { get; } = new();
        public List<FlowHelperDefinition> Provided { get; } = new();
    }

    private static FlowHelperCollection CollectRequiredHelpers(IEnumerable<BlockInstance> blocks)
    {
        var helpers = new FlowHelperCollection();
        foreach (var block in blocks)
            CollectRequiredHelpers(block, helpers);
        return helpers;
    }

    private static void CollectRequiredHelpers(BlockInstance block, FlowHelperCollection helpers)
    {
        var definition = BlockRegistry.Get(block.BlockDefId);
        if (definition != null)
        {
            foreach (string helper in definition.RequiredHelpers)
                helpers.Required.Add(helper);
            helpers.Provided.AddRange(definition.ProvidedHelpers);
        }

        foreach (var nestedBlock in block.ParamBlocks.Values)
            CollectRequiredHelpers(nestedBlock, helpers);
        foreach (var child in block.TrueBranch)
            CollectRequiredHelpers(child, helpers);
        foreach (var child in block.FalseBranch)
            CollectRequiredHelpers(child, helpers);
    }
}
