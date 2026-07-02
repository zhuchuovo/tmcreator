namespace tmcreator.Models.Flow;

public sealed class FlowGenerationContext
{
    private readonly System.Text.StringBuilder _sb;
    private int _index;

    public FlowGenerationContext(System.Text.StringBuilder sb, string sourceExpression, string projectileExpression, string projectCodeName, bool canModifyItemHoldoutOffset = false)
    {
        _sb = sb;
        SourceExpression = sourceExpression;
        ProjectileExpression = projectileExpression;
        ProjectCodeName = projectCodeName;
        CanModifyItemHoldoutOffset = canModifyItemHoldoutOffset;
    }

    public string SourceExpression { get; }
    public string ProjectileExpression { get; }
    public string ProjectCodeName { get; }
    public bool CanModifyItemHoldoutOffset { get; }

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
            return GetNumberValueBlockExpression(nestedBlock, paramName);

        return FlowCodeUtility.ToFloatLiteral(GetParamString(block, paramName, fallback), fallback);
    }

    public string GetIntValueBlockExpression(BlockInstance block)
    {
        return $"((int)Math.Round((double)({GetNumberValueBlockExpression(block)})))";
    }

    public string GetValueBlockExpression(BlockInstance block)
    {
        var definition = BlockRegistry.Get(block.BlockDefId);
        return definition?.BuildValueExpression?.Invoke(this, block) ?? "0";
    }

    public string GetNumberValueBlockExpression(BlockInstance block, string paramName = "")
    {
        string expression = GetValueBlockExpression(block);
        if (GetValueExpressionKind(block) == ValueExpressionKind.Coordinate)
            return $"({expression}).{GetCoordinateComponent(paramName)}";

        return expression;
    }

    public string GetCoordinateValueBlockExpression(BlockInstance block)
    {
        string expression = GetValueBlockExpression(block);
        return GetValueExpressionKind(block) == ValueExpressionKind.Coordinate
            ? expression
            : $"new Vector2({expression})";
    }

    public ValueExpressionKind GetParamValueKind(BlockInstance block, string paramName)
    {
        return block.ParamBlocks.TryGetValue(paramName, out var nestedBlock)
            ? GetValueExpressionKind(nestedBlock)
            : ValueExpressionKind.Number;
    }

    public ValueExpressionKind GetValueExpressionKind(BlockInstance block)
    {
        var definition = BlockRegistry.Get(block.BlockDefId);
        if (definition == null)
            return ValueExpressionKind.Number;

        return definition.ResolveValueKind?.Invoke(this, block) ?? definition.ValueKind;
    }

    public string GetRawParamValueExpression(BlockInstance block, string paramName, string fallback)
    {
        if (block.ParamBlocks.TryGetValue(paramName, out var nestedBlock))
            return GetValueBlockExpression(nestedBlock);

        return FlowCodeUtility.ToFloatLiteral(GetParamString(block, paramName, fallback), fallback);
    }

    public string EscapeString(string value) => FlowCodeUtility.EscapeString(value);

    public string EscapeComment(string value) => FlowCodeUtility.EscapeComment(value);

    private static string GetCoordinateComponent(string paramName)
    {
        string normalized = paramName.Trim().ToLowerInvariant();
        if (normalized == "y" || normalized.EndsWith("_y", StringComparison.Ordinal) || normalized.EndsWith("y", StringComparison.Ordinal))
            return "Y";

        return "X";
    }
}
