namespace tmcreator.Models.Flow;

public static class BuffOnEndBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "buff_on_end",
        Name = "效果消失时",
        Category = BlockCategory.Event,
        EventComment = "When this buff ends"
    };
}
