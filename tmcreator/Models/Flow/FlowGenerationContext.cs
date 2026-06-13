namespace tmcreator.Models.Flow;

public sealed class FlowGenerationContext
{
    private readonly System.Text.StringBuilder _sb;
    private int _index;

    public FlowGenerationContext(System.Text.StringBuilder sb, string sourceExpression, string projectileExpression, string projectCodeName)
    {
        _sb = sb;
        SourceExpression = sourceExpression;
        ProjectileExpression = projectileExpression;
        ProjectCodeName = projectCodeName;
    }

    public string SourceExpression { get; }
    public string ProjectileExpression { get; }
    public string ProjectCodeName { get; }

    public string Next(string prefix)
    {
        _index++;
        return $"{prefix}{_index}";
    }

    public void AppendLine(int spaces, string text)
    {
        FlowCodeUtility.AppendLine(_sb, spaces, text);
    }

    public void AppendStatements(IEnumerable<BlockInstance> blocks, int indent)
    {
        FlowCodeGenerator.AppendFlowStatements(_sb, blocks, indent, this);
    }

    public string GetParamString(BlockInstance block, string paramName, string fallback)
    {
        return block.ParamValues.TryGetValue(paramName, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }

    public string GetIntExpression(BlockInstance block, string paramName, string fallback)
    {
        if (block.ParamBlocks.TryGetValue(paramName, out var nestedBlock))
            return GetIntValueBlockExpression(nestedBlock);

        return FlowCodeUtility.ToIntLiteral(GetParamString(block, paramName, fallback), fallback);
    }

    public string GetProjectileExpression(BlockInstance block, string paramName, string fallback)
    {
        return FlowCodeUtility.GetProjectileTypeExpression(GetParamString(block, paramName, fallback), fallback, ProjectCodeName);
    }

    public string GetFloatExpression(BlockInstance block, string paramName, string fallback)
    {
        if (block.ParamBlocks.TryGetValue(paramName, out var nestedBlock))
            return GetValueBlockExpression(nestedBlock);

        return FlowCodeUtility.ToFloatLiteral(GetParamString(block, paramName, fallback), fallback);
    }

    public string GetIntValueBlockExpression(BlockInstance block)
    {
        return $"((int)Math.Round((double)({GetValueBlockExpression(block)})))";
    }

    public string GetValueBlockExpression(BlockInstance block)
    {
        var definition = BlockRegistry.Get(block.BlockDefId);
        return definition?.BuildValueExpression?.Invoke(this, block) ?? "0";
    }

    public string EscapeString(string value) => FlowCodeUtility.EscapeString(value);

    public string EscapeComment(string value) => FlowCodeUtility.EscapeComment(value);
}
