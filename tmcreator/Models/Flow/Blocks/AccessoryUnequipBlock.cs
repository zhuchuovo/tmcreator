namespace tmcreator.Models.Flow;

public static class AccessoryUnequipBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "accessory_unequip",
        Name = "脱下饰品时",
        Category = BlockCategory.Event,
        EventComment = "When this accessory is removed"
    };
}
