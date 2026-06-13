namespace tmcreator.Models.Flow;

public static class PlaySoundBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "play_sound",
        Name = "播放声音",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "sound", Label = "声音ID", Type = ParamType.Number, DefaultValue = "1" }
        },
        AppendStatement = Append
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string sound = context.GetIntExpression(block, "sound", "1");
        context.AppendLine(indent, $"Flow_PlaySound(player, {sound});");
    }
}
