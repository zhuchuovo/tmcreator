namespace tmcreator.Models.Flow;

public static class AccessoryWearingBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "accessory_wearing",
        Name = "当穿戴饰品时",
        Category = BlockCategory.Event,
        EventComment = "While this accessory is worn"
    };
}
