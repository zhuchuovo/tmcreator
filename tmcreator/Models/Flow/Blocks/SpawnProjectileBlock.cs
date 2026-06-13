namespace tmcreator.Models.Flow;

public static class SpawnProjectileBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "spawn_projectile",
        Name = "发射弹幕",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "projectile", Label = "弹幕类型", Type = ParamType.Projectile, DefaultValue = "1" },
            new() { Name = "damage", Label = "伤害", Type = ParamType.Number, DefaultValue = "50" },
            new() { Name = "speed", Label = "速度", Type = ParamType.Number, DefaultValue = "10" }
        },
        AppendStatement = Append
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string projectile = context.GetProjectileExpression(block, "projectile", "1");
        string damage = context.GetIntExpression(block, "damage", "50");
        string speed = context.GetFloatExpression(block, "speed", "10");
        string directionVar = context.Next("flowDirection");
        context.AppendLine(indent, $"Vector2 {directionVar} = Main.MouseWorld - player.Center;");
        context.AppendLine(indent, $"if ({directionVar}.LengthSquared() < 0.01f)");
        context.AppendLine(indent, "{");
        context.AppendLine(indent + 4, $"{directionVar} = new Vector2(player.direction == 0 ? 1 : player.direction, 0f);");
        context.AppendLine(indent, "}");
        context.AppendLine(indent, $"{directionVar}.Normalize();");
        context.AppendLine(indent, $"Projectile.NewProjectile({context.SourceExpression}, player.Center, {directionVar} * {speed}, {projectile}, {damage}, 0f, player.whoAmI);");
    }
}
