using tmcreator.Models.Flow;

namespace tmcreator.Models;

public class ProjectData
{
    public int Version { get; set; } = 1;
    public string ProjectName { get; set; } = "未命名工程";
    public List<ModItemData> Items { get; set; } = new();
    public ProjectDraftData Draft { get; set; } = new();
}

public class ProjectDraftData
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ItemType Type { get; set; } = ItemType.Item;

    public int Damage { get; set; }
    public ModDamageKind DamageKind { get; set; } = ModDamageKind.Melee;
    public int UseTime { get; set; } = 30;
    public int UseStyleId { get; set; } = 1;
    public int Knockback { get; set; }
    public int CriticalChance { get; set; } = 4;
    public bool UsesProjectile { get; set; }
    public int ProjectileId { get; set; } = 1;
    public decimal ProjectileSpeed { get; set; } = 10;
    public bool ConsumeOnUse { get; set; }
    public int PickaxePower { get; set; }
    public int AxePower { get; set; }
    public int HammerPower { get; set; }

    public int Width { get; set; } = 20;
    public int Height { get; set; } = 20;
    public int Value { get; set; }
    public int Rarity { get; set; }
    public int MinPick { get; set; }

    public int AccessoryMeleeDamage { get; set; }
    public int AccessoryMagicDamage { get; set; }
    public int AccessoryRangedDamage { get; set; }
    public int AccessorySummonDamage { get; set; }
    public int AccessoryMeleeSpeed { get; set; }
    public int AccessoryMagicSpeed { get; set; }
    public int AccessoryRangedSpeed { get; set; }
    public int AccessorySummonSpeed { get; set; }
    public int AccessoryMeleeCrit { get; set; }
    public int AccessoryMagicCrit { get; set; }
    public int AccessoryRangedCrit { get; set; }
    public int AccessorySummonCrit { get; set; }
    public int AccessoryDefense { get; set; }
    public int AccessoryDamageReduction { get; set; }

    public string TexturePath { get; set; } = string.Empty;
    public bool IsMultiFrameTexture { get; set; }
    public int TextureFrameCount { get; set; } = 1;
    public BuffIconSource BuffIconSource { get; set; } = BuffIconSource.Custom;
    public int VanillaBuffIconId { get; set; } = 1;
    public bool AutoReuse { get; set; }
    public bool UseTurn { get; set; }
    public RecipeData Recipe { get; set; } = new();
    public FlowScript? Flow { get; set; }
}
