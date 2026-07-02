namespace tmcreator.Models.Flow;

public static class SpawnProjectileBlock
{
    private static readonly string[] UseStyleOptions =
    {
        "0 None",
        "1 Swing",
        "2 EatFood",
        "3 Thrust",
        "4 HoldUp",
        "5 Shoot",
        "6 DrinkLong",
        "7 DrinkOld",
        "8 GolfPlay",
        "9 DrinkLiquid",
        "10 HiddenAnimation",
        "11 MowTheLawn",
        "12 Guitar",
        "13 Rapier"
    };

    private static readonly string[] DirectionModeOptions =
    {
        "mouse 鼠标方向",
        "angle 指定角度"
    };

    public static BlockDefinition Create() => new()
    {
        Id = "spawn_projectile",
        Name = "发射弹幕",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "projectile", Label = "弹幕类型", Type = ParamType.Projectile, DefaultValue = "1" },
            new() { Name = "damage", Label = "伤害", Type = ParamType.Number, DefaultValue = "50" },
            new() { Name = "speed", Label = "速度", Type = ParamType.Number, DefaultValue = "10" },
            new() { Name = "direction_mode", Label = "方向", Type = ParamType.Dropdown, DefaultValue = "mouse", Options = DirectionModeOptions },
            new() { Name = "angle", Label = "角度0-360", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "use_style", Label = "Item.useStyle", Type = ParamType.Dropdown, DefaultValue = "5", Options = UseStyleOptions }
        },
        AppendStatement = Append,
        ResolveItemUseStyleId = ResolveUseStyleId
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string projectile = context.GetProjectileExpression(block, "projectile", "1");
        string damage = context.GetIntExpression(block, "damage", "50");
        string speed = context.GetFloatExpression(block, "speed", "10");
        string directionMode = context.GetParamString(block, "direction_mode", "mouse");
        string angle = context.GetFloatExpression(block, "angle", "0");
        string directionVar = context.Next("flowDirection");

        if (directionMode == "angle")
        {
            string radiansVar = context.Next("flowAngleRadians");
            context.AppendLine(indent, $"float {radiansVar} = MathHelper.ToRadians(Math.Clamp({angle}, 0f, 360f));");
            context.AppendLine(indent, $"Vector2 {directionVar} = new Vector2((float)Math.Cos({radiansVar}), (float)Math.Sin({radiansVar}));");
        }
        else
        {
            context.AppendLine(indent, $"Vector2 {directionVar} = Main.MouseWorld - player.Center;");
            context.AppendLine(indent, $"if ({directionVar}.LengthSquared() < 0.01f)");
            context.AppendLine(indent, "{");
            context.AppendLine(indent + 4, $"{directionVar} = new Vector2(player.direction == 0 ? 1 : player.direction, 0f);");
            context.AppendLine(indent, "}");
            context.AppendLine(indent, $"{directionVar}.Normalize();");
        }

        context.AppendLine(indent, $"player.ChangeDir({directionVar}.X >= 0f ? 1 : -1);");
        context.AppendLine(indent, $"player.itemRotation = (float)Math.Atan2({directionVar}.Y * player.direction, {directionVar}.X * player.direction);");
        context.AppendLine(indent, $"Projectile.NewProjectile({context.SourceExpression}, player.Center, {directionVar} * {speed}, {projectile}, {damage}, 0f, player.whoAmI);");
    }

    private static int? ResolveUseStyleId(BlockInstance block)
    {
        string value = block.ParamValues.GetValueOrDefault("use_style", "5");
        int spaceIndex = value.IndexOf(' ');
        if (spaceIndex > 0)
            value = value[..spaceIndex];

        return int.TryParse(value, out int useStyleId) && useStyleId >= 0
            ? useStyleId
            : null;
    }
}
