namespace tmcreator.Models.Flow;

public static class ValueMathBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "value_math",
        Name = "数值运算",
        Category = BlockCategory.Value,
        Params =
        {
            new() { Name = "left", Label = "左值", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "operator", Label = "运算", Type = ParamType.Dropdown, DefaultValue = "*", Options = new[] { "+", "-", "*", "/", "%", "=" } },
            new() { Name = "right", Label = "右值", Type = ParamType.Number, DefaultValue = "1" }
        },
        BuildValueExpression = BuildExpression,
        ResolveValueKind = ResolveKind
    };

    private static ValueExpressionKind ResolveKind(FlowGenerationContext context, BlockInstance block)
    {
        string op = context.GetParamString(block, "operator", "*");
        if (op == "=")
            return ValueExpressionKind.Number;

        return context.GetParamValueKind(block, "left") == ValueExpressionKind.Coordinate ||
               context.GetParamValueKind(block, "right") == ValueExpressionKind.Coordinate
            ? ValueExpressionKind.Coordinate
            : ValueExpressionKind.Number;
    }

    private static string BuildExpression(FlowGenerationContext context, BlockInstance block)
    {
        string op = context.GetParamString(block, "operator", "*");
        var leftKind = context.GetParamValueKind(block, "left");
        var rightKind = context.GetParamValueKind(block, "right");

        if (ResolveKind(context, block) == ValueExpressionKind.Coordinate)
            return BuildCoordinateExpression(context, block, op, leftKind, rightKind);

        return BuildNumberExpression(context, block, op, leftKind, rightKind);
    }

    private static string BuildNumberExpression(FlowGenerationContext context, BlockInstance block, string op, ValueExpressionKind leftKind, ValueExpressionKind rightKind)
    {
        if (op == "=" && (leftKind == ValueExpressionKind.Coordinate || rightKind == ValueExpressionKind.Coordinate))
        {
            string leftCoordinate = GetCoordinateParamExpression(context, block, "left", "0");
            string rightCoordinate = GetCoordinateParamExpression(context, block, "right", "1");
            return $"((({leftCoordinate}) - ({rightCoordinate})).LengthSquared() <= 0.01f ? 1 : 0)";
        }

        string left = context.GetFloatExpression(block, "left", "0");
        string right = context.GetFloatExpression(block, "right", "1");

        return op switch
        {
            "+" => $"(({left}) + ({right}))",
            "-" => $"(({left}) - ({right}))",
            "*" => $"(({left}) * ({right}))",
            "/" => $"(({right}) == 0 ? 0 : ({left}) / ({right}))",
            "%" => $"(({left}) * ({right}) / 100f)",
            "=" => $"(({left}) == ({right}) ? 1 : 0)",
            _ => $"(({left}) * ({right}))"
        };
    }

    private static string BuildCoordinateExpression(FlowGenerationContext context, BlockInstance block, string op, ValueExpressionKind leftKind, ValueExpressionKind rightKind)
    {
        if (leftKind == ValueExpressionKind.Coordinate && rightKind == ValueExpressionKind.Coordinate)
        {
            string left = GetCoordinateParamExpression(context, block, "left", "0");
            string right = GetCoordinateParamExpression(context, block, "right", "1");
            return op switch
            {
                "+" => $"(({left}) + ({right}))",
                "-" => $"(({left}) - ({right}))",
                "*" => $"new Vector2(({left}).X * ({right}).X, ({left}).Y * ({right}).Y)",
                "/" => $"new Vector2(({right}).X == 0 ? 0 : ({left}).X / ({right}).X, ({right}).Y == 0 ? 0 : ({left}).Y / ({right}).Y)",
                "%" => $"new Vector2(({left}).X * ({right}).X / 100f, ({left}).Y * ({right}).Y / 100f)",
                _ => $"(({left}) + ({right}))"
            };
        }

        if (leftKind == ValueExpressionKind.Coordinate)
        {
            string left = GetCoordinateParamExpression(context, block, "left", "0");
            string right = context.GetFloatExpression(block, "right", "1");
            return op switch
            {
                "+" => $"(({left}) + new Vector2({right}))",
                "-" => $"(({left}) - new Vector2({right}))",
                "*" => $"(({left}) * ({right}))",
                "/" => $"(({right}) == 0 ? Vector2.Zero : ({left}) / ({right}))",
                "%" => $"(({left}) * ({right}) / 100f)",
                _ => $"(({left}) * ({right}))"
            };
        }

        string leftNumber = context.GetFloatExpression(block, "left", "0");
        string rightCoordinate = GetCoordinateParamExpression(context, block, "right", "1");
        return op switch
        {
            "+" => $"(new Vector2({leftNumber}) + ({rightCoordinate}))",
            "-" => $"(new Vector2({leftNumber}) - ({rightCoordinate}))",
            "*" => $"(({rightCoordinate}) * ({leftNumber}))",
            "/" => $"new Vector2(({rightCoordinate}).X == 0 ? 0 : ({leftNumber}) / ({rightCoordinate}).X, ({rightCoordinate}).Y == 0 ? 0 : ({leftNumber}) / ({rightCoordinate}).Y)",
            "%" => $"(({rightCoordinate}) * ({leftNumber}) / 100f)",
            _ => $"(({rightCoordinate}) * ({leftNumber}))"
        };
    }

    private static string GetCoordinateParamExpression(FlowGenerationContext context, BlockInstance block, string paramName, string fallback)
    {
        if (block.ParamBlocks.TryGetValue(paramName, out var nestedBlock))
            return context.GetCoordinateValueBlockExpression(nestedBlock);

        return $"new Vector2({context.GetRawParamValueExpression(block, paramName, fallback)})";
    }
}
