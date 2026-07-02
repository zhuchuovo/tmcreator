namespace tmcreator.Models.Flow;

public sealed class FlowEventGenerationContext
{
    public FlowEventGenerationContext(
        System.Text.StringBuilder builder,
        string projectCodeName,
        string tempStatPlayerClassName,
        string tempStatNpcClassName,
        string accessoryPlayerClassName = "",
        string buffClassName = "",
        bool hasAnimation = false,
        int frameCount = 1,
        bool canModifyItemHoldoutOffset = false)
    {
        Builder = builder;
        ProjectCodeName = projectCodeName;
        TempStatPlayerClassName = tempStatPlayerClassName;
        TempStatNpcClassName = tempStatNpcClassName;
        AccessoryPlayerClassName = accessoryPlayerClassName;
        BuffClassName = buffClassName;
        HasAnimation = hasAnimation;
        FrameCount = frameCount;
        CanModifyItemHoldoutOffset = canModifyItemHoldoutOffset;
    }

    public System.Text.StringBuilder Builder { get; }
    public string ProjectCodeName { get; }
    public string TempStatPlayerClassName { get; }
    public string TempStatNpcClassName { get; }
    public string AccessoryPlayerClassName { get; }
    public string BuffClassName { get; }
    public bool HasAnimation { get; }
    public int FrameCount { get; }
    public bool CanModifyItemHoldoutOffset { get; }

    public void AppendGroupBodies(
        IEnumerable<FlowEventGroup> groups,
        int indent,
        string sourceExpression = "player.GetSource_ItemUse(Item)",
        string projectileExpression = "null")
    {
        var context = new FlowGenerationContext(Builder, sourceExpression, projectileExpression, ProjectCodeName, CanModifyItemHoldoutOffset);
        foreach (var group in groups)
        {
            FlowCodeUtility.AppendLine(Builder, indent, $"// {GetEventComment(group.EventId)}");
            FlowCodeGenerator.AppendFlowStatements(Builder, group.Blocks, indent, context);
        }
    }

    private static string GetEventComment(string eventId)
    {
        var definition = BlockRegistry.Get(eventId);
        return string.IsNullOrWhiteSpace(definition?.EventComment)
            ? "When this item is used"
            : definition.EventComment;
    }
}
