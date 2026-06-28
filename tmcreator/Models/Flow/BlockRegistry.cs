namespace tmcreator.Models.Flow;

public static class BlockRegistry
{
    public static List<BlockDefinition> AllBlocks { get; } = new()
    {
        OnUseBlock.Create(),
        OnHitNpcBlock.Create(),
        OnHitPvpBlock.Create(),
        WhileHeldBlock.Create(),
        BuffOnGainBlock.Create(),
        BuffUpdateBlock.Create(),
        BuffOnEndBlock.Create(),
        ProjectileOnSpawnBlock.Create(),
        ProjectileOnHitNpcBlock.Create(),
        ProjectileOnHitPlayerBlock.Create(),
        ProjectileUpdateBlock.Create(),
        AccessoryWearingBlock.Create(),
        AccessoryAttackBlock.Create(),
        AccessoryUnequipBlock.Create(),
        AccessoryEquipBlock.Create(),

        IfHpBlock.Create(),
        IfBuffBlock.Create(),
        IfRandomBlock.Create(),
        IfValueCompareBlock.Create(),
        IfValueEqualBlock.Create(),

        DealDamageBlock.Create(),
        HealPlayerBlock.Create(),
        RestoreManaBlock.Create(),
        SpawnProjectileBlock.Create(),
        SetProjectileSpeedBlock.Create(),
        SpawnParticlesBlock.Create(),
        AddBuffBlock.Create(),
        KillNpcBlock.Create(),
        SetValueBlock.Create(),
        TemporaryStatBlock.Create(),
        IfKeyDownBlock.Create(),
        CreateVariableBlock.Create(),
        BroadcastBlock.Create(),
        PlaySoundBlock.Create(),
        GiveItemBlock.Create(),
        TeleportBlock.Create(),

        ValueConstantBlock.Create(),
        ValueMathBlock.Create(),
        ValueVariableBlock.Create(),
        ValueNpcHpBlock.Create(),
        ValuePlayerHpBlock.Create(),
        ValuePlayerManaBlock.Create(),
    };

    public static List<BlockDefinition> GetByCategory(BlockCategory cat) =>
        AllBlocks.Where(b => b.Category == cat).ToList();

    public static BlockDefinition? Get(string id) =>
        AllBlocks.FirstOrDefault(b => b.Id == id);

    public static List<BlockDefinition> GetEventsByHost(FlowEventHost host) =>
        AllBlocks
            .Where(b => b.Category == BlockCategory.Event && b.EventDescriptor?.Host == host)
            .ToList();

    public static string GetDefaultEventId(FlowEventHost host) =>
        GetEventsByHost(host).FirstOrDefault(b => b.EventDescriptor?.IsDefault == true)?.Id
        ?? GetEventsByHost(host).FirstOrDefault()?.Id
        ?? "";

    public static HashSet<string> GetEventIds(FlowEventHost host) =>
        GetEventsByHost(host).Select(b => b.Id).ToHashSet();
}
