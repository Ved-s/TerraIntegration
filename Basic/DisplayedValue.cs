using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraIntegration.Basic
{
    public abstract class DisplayedValue
    {
        public static Dictionary<string, DisplayedValue> ByType = new();
        public abstract string Type { get; }

        public abstract string HoverText { get; }

        public abstract void Draw(Rectangle screenRect, SpriteBatch spriteBatch);

        protected abstract void SendCustomData(BinaryWriter writer);
        protected abstract DisplayedValue ReceiveCustomData(BinaryReader reader);

        public void SendData(BinaryWriter writer)
        {
            writer.Write(Type);

            long lenPos = writer.BaseStream.Position;
            writer.Write((ushort)0);
            long startPos = writer.BaseStream.Position;
            SendCustomData(writer);
            long endPos = writer.BaseStream.Position;

            writer.BaseStream.Seek(lenPos, SeekOrigin.Begin);
            writer.Write((ushort)(endPos - startPos));
            writer.BaseStream.Seek(endPos, SeekOrigin.Begin);
        }
        public static DisplayedValue ReceiveData(BinaryReader reader)
        {
            string type = reader.ReadString();
            ushort length = reader.ReadUInt16();

            if (ByType.TryGetValue(type, out DisplayedValue value))
            {
                long endPos = reader.BaseStream.Position + length;

                DisplayedValue result = value.ReceiveCustomData(reader);
                reader.BaseStream.Seek(endPos, SeekOrigin.Begin);

                return result;
            }

            reader.ReadBytes(length);
            return null;
        }

        public static void Register(DisplayedValue value)
        {
            ByType[value.Type] = value;
        }
        internal static void Unregister()
        {
            ByType.Clear();
        }

        public override bool Equals(object obj)
            => obj is DisplayedValue value
            && Type == value.Type
            && Equals(value);

        public abstract bool Equals(DisplayedValue value);
    }
}
