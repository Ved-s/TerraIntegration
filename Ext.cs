using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraIntegration
{
    public static class Ext
    {
        public static bool IsNullEmptyOrWhitespace(this string str) => string.IsNullOrWhiteSpace(str) || str.Length == 0;
        public static string NullIfEmpty(this string str) => str.IsNullEmptyOrWhitespace() ? null : str;

        public static void AppendColorTag(this StringBuilder builder, Color color, string text)
        {
            if (text is null)
                return;

            uint v = color.PackedValue; // AABBGGRR

            v = (v & 0xff0000) >> 16 | (v & 0x00ff00) | (v & 0x0000ff) << 16; // RRGGBB

            builder.Append("[c/");
            builder.Append(v.ToString("x6"));
            builder.Append(':');
            builder.Append(text.Replace("\n", $"]\n[c/{v:x6}:"));
            builder.Append(']');
        }
    }
}
