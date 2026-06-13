namespace tmcreator.Models.Flow;

public static class ProjectileOnSpawnBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "projectile_on_spawn",
        Name = "当弹幕发射时",
        Category = BlockCategory.Event,
        EventComment = "When this projectile is fired"
    };
}
