namespace tmcreator.Models.Flow;

public static class WhileHeldBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "while_held",
        Name = "手持时持续",
        Category = BlockCategory.Event,
        EventComment = "While this item is held"
    };
}
