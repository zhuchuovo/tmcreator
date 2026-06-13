namespace tmcreator.Models.Flow;

public static class AccessoryEquipBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "accessory_equip",
        Name = "安装饰品时",
        Category = BlockCategory.Event,
        EventComment = "When this accessory is equipped"
    };
}
