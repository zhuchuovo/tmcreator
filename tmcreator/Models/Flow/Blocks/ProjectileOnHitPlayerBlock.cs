namespace tmcreator.Models.Flow;

public static class ProjectileOnHitPlayerBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "projectile_on_hit_player",
        Name = "当弹幕击中玩家时",
        Category = BlockCategory.Event,
        EventComment = "When this projectile hits a player"
    };
}
