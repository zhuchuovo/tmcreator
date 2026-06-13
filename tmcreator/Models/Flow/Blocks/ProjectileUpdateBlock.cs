namespace tmcreator.Models.Flow;

public static class ProjectileUpdateBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "projectile_update",
        Name = "当弹幕存在时",
        Category = BlockCategory.Event,
        EventComment = "While this projectile exists"
    };
}
