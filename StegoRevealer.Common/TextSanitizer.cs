using System.Buffers;
using System.Globalization;
using System.Text;

namespace StegoRevealer.Common;

public static class TextSanitizer
{
    public sealed class Options
    {
        public bool AllowTab { get; set; } = false;
        public bool KeepPrivateUse { get; set; } = false;
        public bool KeepVariationSelectors { get; set; } = false;
        public bool KeepBidiMarks { get; set; } = false;
        public bool CollapseSpaces { get; set; } = false;
        public bool NormalizeCRLFtoLF { get; set; } = true;
        public NormalizationForm Normalization { get; set; } = NormalizationForm.FormC;
    }

    public static string FilterBadSymbols(string raw, Options? options = null)
    {
        if (string.IsNullOrEmpty(raw)) return raw ?? string.Empty;
        options ??= new Options();

        if (!raw.IsNormalized(options.Normalization))
            raw = raw.Normalize(options.Normalization);

        if (options.NormalizeCRLFtoLF)
            raw = raw.Replace("\r\n", "\n").Replace('\r', '\n');

        var sb = new StringBuilder(raw.Length);
        bool lastWasSpace = false;
        bool lastWasBase = false;

        for (int i = 0; i < raw.Length;)
        {
            var span = raw.AsSpan(i);
            var status = Rune.DecodeFromUtf16(span, out var rune, out int consumed);
            if (status != OperationStatus.Done)
            {
                i += Math.Max(consumed, 1);
                continue;
            }
            i += consumed;

            int cp = rune.Value;
            var cat = Rune.GetUnicodeCategory(rune);

            if (cat == UnicodeCategory.Control)
            {
                if (cp == '\n' || (options.AllowTab && cp == '\t'))
                    AppendRune(sb, rune, ref lastWasSpace, ref lastWasBase);
                continue;
            }

            if (cat == UnicodeCategory.Format)
            {
                if (options.KeepBidiMarks && IsBidiMark(cp))
                    AppendRune(sb, rune, ref lastWasSpace, ref lastWasBase);
                continue;
            }

            if (cat == UnicodeCategory.Surrogate ||
                cat == UnicodeCategory.OtherNotAssigned ||
                IsNonCharacter(cp))
                continue;

            if (!options.KeepPrivateUse && cat == UnicodeCategory.PrivateUse)
                continue;

            if (!options.KeepVariationSelectors && IsVariationSelector(cp))
                continue;

            if (cp == 0xFFFD || (cp >= 0xE0000 && cp <= 0xE007F))
                continue;

            if (IsSpaceLike(cp))
            {
                if (options.CollapseSpaces)
                {
                    if (!lastWasSpace)
                    {
                        sb.Append(' ');
                        lastWasSpace = true;
                        lastWasBase = false;
                    }
                }
                else
                {
                    sb.Append(' ');
                    lastWasSpace = true;
                    lastWasBase = false;
                }
                continue;
            }

            if (cat is UnicodeCategory.NonSpacingMark or
                     UnicodeCategory.SpacingCombiningMark or
                     UnicodeCategory.EnclosingMark)
            {
                if (!lastWasBase) continue;
                AppendRune(sb, rune, ref lastWasSpace, ref lastWasBase);
                continue;
            }

            AppendRune(sb, rune, ref lastWasSpace, ref lastWasBase, isBase: true);
        }

        return sb.ToString();
    }

    private static void AppendRune(StringBuilder sb, Rune r,
        ref bool lastWasSpace, ref bool lastWasBase, bool isBase = false)
    {
        sb.Append(r.ToString());
        lastWasSpace = false;
        if (isBase) lastWasBase = true;
    }

    private static bool IsBidiMark(int cp) =>
        (cp >= 0x200E && cp <= 0x200F) || // LRM/RLM
        (cp >= 0x202A && cp <= 0x202E) || // LRE/RLE/PDF/LRO/RLO
        (cp >= 0x2066 && cp <= 0x2069);   // LRI/RLI/FSI/PDI

    private static bool IsVariationSelector(int cp) =>
        (cp >= 0xFE00 && cp <= 0xFE0F) || (cp >= 0xE0100 && cp <= 0xE01EF);

    private static bool IsNonCharacter(int cp)
    {
        if (cp >= 0xFDD0 && cp <= 0xFDEF) return true;           // FDD0..FDEF
        if ((cp & 0xFFFF) is 0xFFFE or 0xFFFF) return true;      // xxFFFE/xxFFFF
        return false;
    }

    private static bool IsSpaceLike(int cp) =>
        cp == 0x0020 ||               // space
        cp == 0x00A0 ||               // NBSP
        cp == 0x1680 ||               // OGHAM SPACE MARK
        (cp >= 0x2000 && cp <= 0x200A) ||
        cp == 0x202F ||               // NNBSP
        cp == 0x205F ||               // MMSP
        cp == 0x3000;                 // IDEOGRAPHIC SPACE
}
