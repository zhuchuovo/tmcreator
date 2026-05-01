using tmcreator.Models.Flow;

namespace tmcreator.Models;

public enum ItemType
{
    Tool,
    Weapon,
    Block,
    Item,
    Buff,
    Projectile,
    Accessory
}

public enum ModDamageKind
{
    Melee,
    Ranged,
    Magic,
    Summon,
    Generic
}

public enum BuffIconSource
{
    Custom,
    Vanilla
}

public class RecipeIngredientData
{
    public int ItemId { get; set; }
    public string DisplayName { get; set; } = "";
    public int Stack { get; set; } = 1;

    public override string ToString() => $"{ItemId} - {DisplayName} x{Stack}";
}

public class RecipeData
{
    public bool Enabled { get; set; }
    public List<RecipeIngredientData> Ingredients { get; set; } = new();
    public string CraftingStationKey { get; set; } = "";
    public string CraftingStationDisplay { get; set; } = "徒手";
}

public class ModItemData
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
    public FlowScript? Flow { get; set; }

    public bool AutoReuse { get; set; }
    public bool UseTurn { get; set; }
    public int UseAnimation { get; set; } = 30;
    public RecipeData Recipe { get; set; } = new();

    public string TypeDisplay => Type switch
    {
        ItemType.Tool => "工具",
        ItemType.Weapon => "武器",
        ItemType.Block => "方块",
        ItemType.Item => "物品",
        ItemType.Buff => "Buff",
        ItemType.Projectile => "弹幕",
        ItemType.Accessory => "饰品",
        _ => "未知"
    };

    public string DamageKindDisplay => DamageKind switch
    {
        ModDamageKind.Melee => "近战",
        ModDamageKind.Ranged => "远程",
        ModDamageKind.Magic => "魔法",
        ModDamageKind.Summon => "召唤",
        ModDamageKind.Generic => "普通伤害",
        _ => "近战"
    };
}
