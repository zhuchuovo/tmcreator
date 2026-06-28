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
    public string EventComment { get; set; } = "";
    public Action<FlowGenerationContext, BlockInstance, int>? AppendStatement { get; set; }
    public Func<FlowGenerationContext, BlockInstance, string>? BuildCondition { get; set; }
    public Func<FlowGenerationContext, BlockInstance, string>? BuildValueExpression { get; set; }
    public FlowEventDescriptor? EventDescriptor { get; set; }
    public HashSet<string> RequiredHelpers { get; set; } = new();
    public List<FlowHelperDefinition> ProvidedHelpers { get; set; } = new();

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
    public int CanvasX { get; set; } = -1;
    public int CanvasY { get; set; } = -1;
    public Dictionary<string, string> ParamValues { get; set; } = new();
    public Dictionary<string, BlockInstance> ParamBlocks { get; set; } = new();
    public List<BlockInstance> TrueBranch { get; set; } = new();
    public List<BlockInstance> FalseBranch { get; set; } = new();
}

public class FlowScript
{
    public List<BlockInstance> Blocks { get; set; } = new();
}
