namespace tmcreator.Models.Flow;

public static class BuffOnGainBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "buff_on_gain",
        Name = "获得效果时",
        Category = BlockCategory.Event,
        EventComment = "When this buff is gained"
    };
}
