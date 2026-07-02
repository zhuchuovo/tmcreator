namespace tmcreator.Models.Flow;

public static class SpawnProjectileFromCoordinateBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "spawn_projectile_from_coordinate",
        Name = "从坐标发射弹幕",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "x", Label = "X坐标", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "y", Label = "Y坐标", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "target_x", Label = "发射到X坐标", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "target_y", Label = "发射到Y坐标", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "duration", Label = "持续tick", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "speed", Label = "速度", Type = ParamType.Number, DefaultValue = "10" },
            new() { Name = "projectile", Label = "弹幕", Type = ParamType.Projectile, DefaultValue = "1" }
        },
        AppendStatement = Append
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string x = context.GetFloatExpression(block, "x", "0");
        string y = context.GetFloatExpression(block, "y", "0");
        string targetX = context.GetFloatExpression(block, "target_x", "0");
        string targetY = context.GetFloatExpression(block, "target_y", "0");
        string duration = context.GetIntExpression(block, "duration", "0");
        string speed = context.GetFloatExpression(block, "speed", "10");
        string projectile = context.GetProjectileExpression(block, "projectile", "1");
        string positionVar = context.Next("flowPosition");
        string targetVar = context.Next("flowTarget");
        string velocityVar = context.Next("flowVelocity");
        string damageVar = context.Next("flowDamage");
        string durationVar = context.Next("flowDuration");
        string useTargetVar = context.Next("flowUseTarget");
        string projectileIndexVar = context.Next("flowProjectileIndex");

        context.AppendLine(indent, $"Vector2 {positionVar} = new Vector2({x}, {y});");
        context.AppendLine(indent, $"Vector2 {targetVar} = new Vector2({targetX}, {targetY});");
        context.AppendLine(indent, $"int {durationVar} = Math.Max(0, {duration});");
        context.AppendLine(indent, $"bool {useTargetVar} = {BuildUseTargetExpression(block, targetX, targetY)};");
        context.AppendLine(indent, $"Vector2 {velocityVar} = new Vector2(player != null && player.direction < 0 ? -1f : 1f, 0f) * Math.Max(0f, {speed});");
        context.AppendLine(indent, $"if ({useTargetVar})");
        context.AppendLine(indent, "{");
        context.AppendLine(indent + 4, $"{velocityVar} = {targetVar} - {positionVar};");
        context.AppendLine(indent + 4, $"if ({velocityVar}.LengthSquared() < 0.01f)");
        context.AppendLine(indent + 4, "{");
        context.AppendLine(indent + 8, $"{velocityVar} = new Vector2(player != null && player.direction < 0 ? -1f : 1f, 0f);");
        context.AppendLine(indent + 4, "}");
        context.AppendLine(indent + 4, $"else if ({durationVar} > 0)");
        context.AppendLine(indent + 4, "{");
        context.AppendLine(indent + 8, $"{velocityVar} /= {durationVar};");
        context.AppendLine(indent + 4, "}");
        context.AppendLine(indent + 4, "else");
        context.AppendLine(indent + 4, "{");
        context.AppendLine(indent + 8, $"{velocityVar}.Normalize();");
        context.AppendLine(indent + 8, $"{velocityVar} *= Math.Max(0f, {speed});");
        context.AppendLine(indent + 4, "}");
        context.AppendLine(indent, "}");
        context.AppendLine(indent, $"int {damageVar} = player != null ? Math.Max(1, player.HeldItem.damage) : 1;");
        context.AppendLine(indent, $"int {projectileIndexVar} = Projectile.NewProjectile({context.SourceExpression}, {positionVar}, {velocityVar}, {projectile}, {damageVar}, 0f, player != null ? player.whoAmI : Main.myPlayer);");
        context.AppendLine(indent, $"if ({durationVar} > 0 && {projectileIndexVar} >= 0 && {projectileIndexVar} < Main.maxProjectiles)");
        context.AppendLine(indent, "{");
        context.AppendLine(indent + 4, $"Main.projectile[{projectileIndexVar}].timeLeft = {durationVar};");
        context.AppendLine(indent, "}");
    }

    private static string BuildUseTargetExpression(BlockInstance block, string targetX, string targetY)
    {
        if (block.ParamBlocks.ContainsKey("target_x") || block.ParamBlocks.ContainsKey("target_y"))
            return "true";

        string rawX = block.ParamValues.GetValueOrDefault("target_x", "0");
        string rawY = block.ParamValues.GetValueOrDefault("target_y", "0");
        bool hasLiteralTarget =
            double.TryParse(rawX, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double literalX) && Math.Abs(literalX) > 0.0001 ||
            double.TryParse(rawY, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double literalY) && Math.Abs(literalY) > 0.0001;

        return hasLiteralTarget
            ? "true"
            : $"Math.Abs({targetX}) > 0.0001f || Math.Abs({targetY}) > 0.0001f";
    }
}
