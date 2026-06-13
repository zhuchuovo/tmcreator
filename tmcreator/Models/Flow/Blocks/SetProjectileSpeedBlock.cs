namespace tmcreator.Models.Flow;

public static class SetProjectileSpeedBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "set_projectile_speed",
        Name = "弹幕速度 =",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "speed", Label = "速度", Type = ParamType.Number, DefaultValue = "10" }
        },
        AppendStatement = Append
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string speed = context.GetFloatExpression(block, "speed", "10");
        context.AppendLine(indent, $"Flow_SetProjectileSpeed({context.ProjectileExpression}, player, {speed});");
    }
}
