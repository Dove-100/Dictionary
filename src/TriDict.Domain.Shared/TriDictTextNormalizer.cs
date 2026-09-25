using System.Globalization;
using System.Text;

namespace TriDict;

public static class TriDictTextNormalizer
{
    public static string NormalizeForSearch(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Normalize(NormalizationForm.FormKC).Trim().ToLowerInvariant();
        var builder = new StringBuilder(normalized.Length);
        var previousWasSpace = false;

        foreach (var rune in normalized.EnumerateRunes())
        {
            var isSpace = Rune.GetUnicodeCategory(rune) == UnicodeCategory.SpaceSeparator ||
                          rune.Value is '\t' or '\r' or '\n';
            if (isSpace)
            {
                if (!previousWasSpace)
                {
                    builder.Append(' ');
                    previousWasSpace = true;
                }

                continue;
            }

            builder.Append(rune.ToString());
            previousWasSpace = false;
        }

        return builder.ToString();
    }
}
