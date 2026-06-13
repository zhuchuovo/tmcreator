namespace tmcreator.Models.Flow;

public static class FlowCodeUtility
{
    public static void AppendLine(System.Text.StringBuilder sb, int spaces, string text)
    {
        sb.Append(' ', spaces);
        sb.AppendLine(text);
    }

    public static string EscapeString(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    public static string EscapeComment(string value)
    {
        return value.Replace("\r", " ").Replace("\n", " ");
    }

    public static string ToIntLiteral(string value, string fallback)
    {
        if (int.TryParse(value, out var intValue))
            return intValue.ToString(System.Globalization.CultureInfo.InvariantCulture);

        if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var doubleValue))
            return ((int)Math.Round(doubleValue)).ToString(System.Globalization.CultureInfo.InvariantCulture);

        return int.TryParse(fallback, out var fallbackValue)
            ? fallbackValue.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : "0";
    }

    public static string ToFloatLiteral(string value, string fallback)
    {
        if (!double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var doubleValue))
            double.TryParse(fallback, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out doubleValue);

        return doubleValue.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + "f";
    }

    public static string GetProjectileTypeExpression(string? reference, string fallback, string projectCodeName)
    {
        string value = NormalizeProjectileReference(reference, GetProjectileIdFallback(fallback));
        if (int.TryParse(value, out int intValue))
            return Math.Max(0, intValue).ToString(System.Globalization.CultureInfo.InvariantCulture);

        if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double doubleValue))
            return Math.Max(0, (int)Math.Round(doubleValue)).ToString(System.Globalization.CultureInfo.InvariantCulture);

        if (!IsAsciiIdentifier(value))
            return ToIntLiteral(fallback, "1");

        string className = SanitizeClassName(value);
        if (string.IsNullOrWhiteSpace(projectCodeName))
            return $"ModContent.ProjectileType<{className}>()";

        return $"ModContent.ProjectileType<global::{projectCodeName}.Projectiles.{className}>()";
    }

    public static string NormalizeKeyboardKeyName(string? key)
    {
        string value = key?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(value))
            return "F";

        return value switch
        {
            "空格" => "Space",
            "回车" => "Enter",
            "上" => "Up",
            "下" => "Down",
            "左" => "Left",
            "右" => "Right",
            "左Shift" or "左shift" => "LeftShift",
            "右Shift" or "右shift" => "RightShift",
            "左Ctrl" or "左ctrl" => "LeftControl",
            "右Ctrl" or "右ctrl" => "RightControl",
            _ => value
        };
    }

    private static int GetProjectileIdFallback(string? reference)
    {
        if (int.TryParse(reference, out int intValue))
            return Math.Max(0, intValue);

        if (double.TryParse(reference, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double doubleValue))
            return Math.Max(0, (int)Math.Round(doubleValue));

        return 1;
    }

    private static string NormalizeProjectileReference(string? reference, int projectileId)
    {
        string trimmed = reference?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
            return Math.Max(0, projectileId).ToString(System.Globalization.CultureInfo.InvariantCulture);

        if (trimmed == "1" && projectileId != 1)
            return Math.Max(0, projectileId).ToString(System.Globalization.CultureInfo.InvariantCulture);

        return trimmed;
    }

    private static bool IsAsciiIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        char first = value[0];
        if (!((first >= 'A' && first <= 'Z') || (first >= 'a' && first <= 'z') || first == '_'))
            return false;

        return value.All(c =>
            (c >= 'A' && c <= 'Z') ||
            (c >= 'a' && c <= 'z') ||
            (c >= '0' && c <= '9') ||
            c == '_');
    }

    private static string SanitizeClassName(string name)
    {
        var sb = new System.Text.StringBuilder();
        bool capitalize = true;

        foreach (char c in name)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                sb.Append(capitalize ? char.ToUpperInvariant(c) : c);
                capitalize = false;
            }
            else if (c == ' ')
            {
                capitalize = true;
            }
        }

        string result = sb.ToString();
        if (result.Length == 0) return "CustomItem";
        if (char.IsDigit(result[0])) result = "Item" + result;
        return result;
    }
}
