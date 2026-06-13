namespace tmcreator.Models.Flow;

public static class ProjectileOnHitNpcBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "projectile_on_hit_npc",
        Name = "当弹幕命中时",
        Category = BlockCategory.Event,
        EventComment = "When this projectile hits an NPC"
    };
}
