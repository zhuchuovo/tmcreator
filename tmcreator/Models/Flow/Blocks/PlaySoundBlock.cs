namespace tmcreator.Models.Flow;

public static class PlaySoundBlock
{
    public static BlockDefinition Create() => new()
    {
        Id = "play_sound",
        Name = "播放音效",
        Category = BlockCategory.Action,
        Params =
        {
            new() { Name = "sound", Label = "音效", Type = ParamType.Text, DefaultValue = "Item1" }
        },
        AppendStatement = Append,
        RequiredHelpers = { FlowHelperNames.PlaySound }
    };

    private static void Append(FlowGenerationContext context, BlockInstance block, int indent)
    {
        string sound = context.GetParamString(block, "sound", "Item1");
        context.AppendLine(indent, $"Flow_PlaySound(player, \"{context.EscapeString(sound)}\");");
    }
}
