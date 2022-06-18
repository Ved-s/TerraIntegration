using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Values;
using Terraria.UI;

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
        public static Vector2 Size(this CalculatedStyle style) => new(style.Width, style.Height);

        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> ienum, params Func<T, bool>[] predicates)
        {
            List<T>[] order = new List<T>[predicates.Length + 1];
            for (int i = 0; i < order.Length; i++)
                order[i] = new();

            foreach (T t in ienum)
                for (int i = 0; i < order.Length; i++)
                    if (i == predicates.Length || predicates[i](t))
                    {
                        order[i].Add(t);
                        break;
                    }
            

            foreach (var list in order)
                foreach (var item in list)
                    yield return item;
        }

        public static T NewInstance<T>(this T obj)
        {
            return (T)Activator.CreateInstance(obj.GetType());
        }

        public static void Deconstruct(this Vector2 vec, out float x, out float y)
        {
            x = vec.X;
            y = vec.Y;
        }

        public static Vector2 GetAlignedPos(this UIElement element, Vector2 align)
        {
            CalculatedStyle size = element.GetDimensions();
            return size.Position() + align * size.Size();
        }

        public static void NewFloatingText(this UIElement element, string text, Color color, int time = 100, float velocity = 1, Vector2? align = null)
        {
            if (align is null)
                align = new(.5f, 0);

            CalculatedStyle size = element.GetDimensions();
            Vector2 spawn = size.Position() + align.Value * size.Size();
            Vector2 spawnAlign = new Vector2(1) - align.Value;
            Vector2 vel = (align.Value - spawnAlign) * velocity;

            FloatingText.NewText(text, color, time, vel, spawn, spawnAlign);
        }

        public static VariableValue ToCollectionValue<T>(this IEnumerable<T> ienum, ReturnType collectionType) where T : VariableValue
        {
            return new CollectionList(ienum, collectionType);
        }

        public static bool MatchNull(this ReturnType? returnType, Type type)
            => returnType.HasValue && returnType.Value.Match(type);

        public static bool MatchNull(this ReturnType? returnType, ReturnType? type)
            => returnType.HasValue && returnType.Value.Match(type);
    }
}
