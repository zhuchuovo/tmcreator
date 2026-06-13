namespace tmcreator.Models.Flow;

public static class BuffUpdateBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "buff_update",
        Name = "持续时间内时",
        Category = BlockCategory.Event,
        EventComment = "While this buff is active"
    };
}
