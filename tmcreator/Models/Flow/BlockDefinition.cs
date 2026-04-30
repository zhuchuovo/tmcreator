namespace tmcreator.Models.Flow;

public enum BlockCategory
{
    Event,
    Condition,
    Action,
    Value
}

public enum ParamType
{
    Text,
    Number,
    Dropdown,
    Target,
    Buff,
    Projectile,
    Particle,
    DamageType
}

public class BlockParam
{
    public string Name { get; set; } = "";
    public string Label { get; set; } = "";
    public ParamType Type { get; set; } = ParamType.Text;
    public string DefaultValue { get; set; } = "";
    public string[] Options { get; set; } = Array.Empty<string>();
}

public class BlockDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public BlockCategory Category { get; set; }
    public List<BlockParam> Params { get; set; } = new();
    public bool HasTrueBranch { get; set; }
    public bool HasFalseBranch { get; set; }
    public string TrueLabel { get; set; } = "";
    public string FalseLabel { get; set; } = "";

    public Color Color => Category switch
    {
        BlockCategory.Event => Color.FromArgb(220, 180, 40),
        BlockCategory.Condition => Color.FromArgb(220, 140, 40),
        BlockCategory.Action => Color.FromArgb(60, 160, 220),
        BlockCategory.Value => Color.FromArgb(140, 100, 220),
        _ => Color.Gray
    };
}

public class BlockInstance
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string BlockDefId { get; set; } = "";
    public Dictionary<string, string> ParamValues { get; set; } = new();
    public Dictionary<string, BlockInstance> ParamBlocks { get; set; } = new();
    public List<BlockInstance> TrueBranch { get; set; } = new();
    public List<BlockInstance> FalseBranch { get; set; } = new();
}

public class FlowScript
{
    public List<BlockInstance> Blocks { get; set; } = new();
}

public static class BlockRegistry
{
    public static List<BlockDefinition> AllBlocks { get; } = new()
    {
        // Events
        new() { Id = "on_use", Name = "当使用物品时", Category = BlockCategory.Event },
        new() { Id = "on_hit_npc", Name = "当击中NPC时", Category = BlockCategory.Event,
            Params = { new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "npc" } } },
        new() { Id = "on_hit_pvp", Name = "当击中玩家时", Category = BlockCategory.Event,
            Params = { new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "player" } } },
        new() { Id = "while_held", Name = "手持时持续", Category = BlockCategory.Event },
        new() { Id = "buff_on_gain", Name = "获得效果时", Category = BlockCategory.Event },
        new() { Id = "buff_update", Name = "持续时间内时", Category = BlockCategory.Event },
        new() { Id = "buff_on_end", Name = "效果消失时", Category = BlockCategory.Event },

        // Conditions
        new() { Id = "if_hp", Name = "如果 生命值", Category = BlockCategory.Condition,
            HasTrueBranch = true, HasFalseBranch = true, TrueLabel = "是", FalseLabel = "否",
            Params = {
                new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "npc" },
                new() { Name = "compare", Label = "比较", Type = ParamType.Dropdown, DefaultValue = "<", Options = new[]{"<", ">", "=", "≥", "≤"} },
                new() { Name = "value", Label = "数值", Type = ParamType.Number, DefaultValue = "50" }
            }},
        new() { Id = "if_buff", Name = "如果 有Buff", Category = BlockCategory.Condition,
            HasTrueBranch = true, HasFalseBranch = true, TrueLabel = "有", FalseLabel = "无",
            Params = {
                new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "npc" },
                new() { Name = "buff", Label = "Buff", Type = ParamType.Buff, DefaultValue = "1" }
            }},
        new() { Id = "if_random", Name = "如果 随机概率", Category = BlockCategory.Condition,
            HasTrueBranch = true, HasFalseBranch = true, TrueLabel = "成功", FalseLabel = "失败",
            Params = {
                new() { Name = "chance", Label = "概率%", Type = ParamType.Number, DefaultValue = "50" }
            }},
        new() { Id = "if_value_compare", Name = "如果 数值比较", Category = BlockCategory.Condition,
            HasTrueBranch = true, HasFalseBranch = true, TrueLabel = "执行", FalseLabel = "否则",
            Params = {
                new() { Name = "left", Label = "左值", Type = ParamType.Number, DefaultValue = "0" },
                new() { Name = "compare", Label = "比较", Type = ParamType.Dropdown, DefaultValue = ">", Options = new[]{">", "<", "=", "!=", ">=", "<="} },
                new() { Name = "right", Label = "右值", Type = ParamType.Number, DefaultValue = "0" }
            }},
        new() { Id = "if_value_equal", Name = "如果 数值 比较 数值", Category = BlockCategory.Condition,
            HasTrueBranch = true, HasFalseBranch = false, TrueLabel = "执行",
            Params = {
                new() { Name = "left", Label = "左值", Type = ParamType.Number, DefaultValue = "0" },
                new() { Name = "compare", Label = "比较符", Type = ParamType.Dropdown, DefaultValue = "=", Options = new[]{"=", "!=", ">", "<", ">=", "<="} },
                new() { Name = "right", Label = "右值", Type = ParamType.Number, DefaultValue = "0" }
            }},

        // Actions
        new() { Id = "deal_damage", Name = "造成伤害", Category = BlockCategory.Action,
            Params = {
                new() { Name = "target", Label = "对目标", Type = ParamType.Target, DefaultValue = "npc" },
                new() { Name = "amount", Label = "伤害值", Type = ParamType.Number, DefaultValue = "100" },
                new() { Name = "damage_type", Label = "伤害类型", Type = ParamType.DamageType, DefaultValue = "melee" }
            }},
        new() { Id = "heal_player", Name = "治疗玩家", Category = BlockCategory.Action,
            Params = {
                new() { Name = "amount", Label = "治疗量", Type = ParamType.Number, DefaultValue = "20" }
            }},
        new() { Id = "restore_mana", Name = "恢复魔力", Category = BlockCategory.Action,
            Params = {
                new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "player" },
                new() { Name = "amount", Label = "魔力量", Type = ParamType.Number, DefaultValue = "20" }
            }},
        new() { Id = "spawn_projectile", Name = "发射弹幕", Category = BlockCategory.Action,
            Params = {
                new() { Name = "projectile", Label = "弹幕类型", Type = ParamType.Projectile, DefaultValue = "1" },
                new() { Name = "damage", Label = "伤害", Type = ParamType.Number, DefaultValue = "50" },
                new() { Name = "speed", Label = "速度", Type = ParamType.Number, DefaultValue = "10" }
            }},
        new() { Id = "spawn_particles", Name = "释放粒子", Category = BlockCategory.Action,
            Params = {
                new() { Name = "target", Label = "位置", Type = ParamType.Target, DefaultValue = "player" },
                new() { Name = "particle", Label = "粒子", Type = ParamType.Particle, DefaultValue = "15" },
                new() { Name = "amount", Label = "数量", Type = ParamType.Number, DefaultValue = "12" },
                new() { Name = "speed", Label = "速度", Type = ParamType.Number, DefaultValue = "2" }
            }},
        new() { Id = "add_buff", Name = "添加Buff", Category = BlockCategory.Action,
            Params = {
                new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "player" },
                new() { Name = "buff", Label = "Buff类型", Type = ParamType.Buff, DefaultValue = "1" },
                new() { Name = "duration", Label = "持续时间(秒)", Type = ParamType.Number, DefaultValue = "60" }
            }},
        new() { Id = "kill_npc", Name = "杀死目标", Category = BlockCategory.Action,
            Params = {
                new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "npc" }
            }},
        new() { Id = "set_value", Name = "设置数值", Category = BlockCategory.Action,
            Params = {
                new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "npc" },
                new() { Name = "stat", Label = "属性", Type = ParamType.Dropdown, DefaultValue = "life", Options = new[]{"life", "damage", "defense"} },
                new() { Name = "value", Label = "值", Type = ParamType.Number, DefaultValue = "0" }
            }},
        new() { Id = "create_variable", Name = "创建变量", Category = BlockCategory.Action,
            Params = {
                new() { Name = "name", Label = "变量名", Type = ParamType.Text, DefaultValue = "myValue" },
                new() { Name = "value", Label = "数值", Type = ParamType.Number, DefaultValue = "0" }
            }},
        new() { Id = "broadcast", Name = "发送消息", Category = BlockCategory.Action,
            Params = {
                new() { Name = "message", Label = "消息内容", Type = ParamType.Text, DefaultValue = "Hello World!" }
            }},
        new() { Id = "play_sound", Name = "播放声音", Category = BlockCategory.Action,
            Params = {
                new() { Name = "sound", Label = "声音ID", Type = ParamType.Number, DefaultValue = "1" }
            }},
        new() { Id = "give_item", Name = "给予物品", Category = BlockCategory.Action,
            Params = {
                new() { Name = "item_id", Label = "物品ID", Type = ParamType.Number, DefaultValue = "1" },
                new() { Name = "amount", Label = "数量", Type = ParamType.Number, DefaultValue = "1" }
            }},
        new() { Id = "teleport", Name = "传送", Category = BlockCategory.Action,
            Params = {
                new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "player" },
                new() { Name = "x", Label = "X坐标", Type = ParamType.Number, DefaultValue = "0" },
                new() { Name = "y", Label = "Y坐标", Type = ParamType.Number, DefaultValue = "0" }
            }},

        // Values
        new() { Id = "value_constant", Name = "数值", Category = BlockCategory.Value,
            Params = { new() { Name = "value", Label = "值", Type = ParamType.Number, DefaultValue = "0" } }},
        new() { Id = "value_math", Name = "数值运算", Category = BlockCategory.Value,
            Params = {
                new() { Name = "left", Label = "左值", Type = ParamType.Number, DefaultValue = "0" },
                new() { Name = "operator", Label = "运算", Type = ParamType.Dropdown, DefaultValue = "*", Options = new[]{"+", "-", "*", "/", "%", "="} },
                new() { Name = "right", Label = "右值", Type = ParamType.Number, DefaultValue = "1" }
            }},
        new() { Id = "value_variable", Name = "变量值", Category = BlockCategory.Value,
            Params = { new() { Name = "name", Label = "变量名", Type = ParamType.Text, DefaultValue = "myValue" } }},
        new() { Id = "value_npc_hp", Name = "目标生命值", Category = BlockCategory.Value,
            Params = { new() { Name = "target", Label = "目标", Type = ParamType.Target, DefaultValue = "npc" } }},
        new() { Id = "value_player_hp", Name = "玩家生命值", Category = BlockCategory.Value },
        new() { Id = "value_player_mana", Name = "玩家魔力值", Category = BlockCategory.Value },
    };

    public static List<BlockDefinition> GetByCategory(BlockCategory cat) =>
        AllBlocks.Where(b => b.Category == cat).ToList();

    public static BlockDefinition? Get(string id) =>
        AllBlocks.FirstOrDefault(b => b.Id == id);
}
