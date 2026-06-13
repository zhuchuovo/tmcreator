namespace tmcreator.Models.Flow;

public static class AccessoryAttackBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "accessory_attack",
        Name = "穿戴饰品攻击时",
        Category = BlockCategory.Event,
        EventComment = "When attacking while this accessory is worn"
    };
}
