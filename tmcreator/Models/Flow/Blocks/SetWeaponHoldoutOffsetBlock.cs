namespace tmcreator.Models.Flow;

public static class SetWeaponHoldoutOffsetBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "set_weapon_holdout_offset",
        Name = "修改武器位置偏移",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "x", Label = "X偏移", Type = ParamType.Number, DefaultValue = "0" },
            new() { Name = "y", Label = "Y偏移", Type = ParamType.Number, DefaultValue = "0" }
        },
        AppendStatement = Append
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        if (!context.CanModifyItemHoldoutOffset)
        {
            context.AppendLine(indent, "// 修改武器位置偏移 only works inside item weapon/tool flow.");
            return;
        }

        string x = context.GetFloatExpression(block, "x", "0");
        string y = context.GetFloatExpression(block, "y", "0");
        context.AppendLine(indent, $"Flow_SetWeaponHoldoutOffset({x}, {y});");
    }
}
