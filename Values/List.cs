using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.DisplayedValues;
using TerraIntegration.Interfaces;
using TerraIntegration.Items;
using TerraIntegration.UI;
using TerraIntegration.Variables;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;

namespace TerraIntegration.Values
{
    public class List : VariableValue, ICollection, IOwnProgrammerInterface
    {
        public override string Type => "list";
        public override string TypeDisplay => "List";
        public override Color TypeColor => new(120, 120, 120);

        public Type CollectionType { get; set; } = typeof(VariableValue);

        public UIPanel Interface { get; set; }
        List<ListEntry> Entries = new();
        UIList SubInterfaces;
        UIScrollbar Scroll;
        UIVariableSwitch NewValueSwitch;
        UITextPanel<string> NewValueAdd;

        public IEnumerable<VariableValue> Values;

        public List() { }
        public List(IEnumerable<VariableValue> values)
        {
            Values = values;
        }

        public IEnumerable<VariableValue> Enumerate() => Values;

        public void SetupInterface()
        {
            Interface.PaddingTop = 6;
            Interface.PaddingBottom = 6;
            Interface.PaddingRight = 6;
            Interface.PaddingLeft = 6;
            
            Entries.Clear();

            Interface.Append(NewValueSwitch = new()
            {
                Left = new(-96, 1),

                SwitchVariableTypes = new[] { "const" },
                SwitchValueTypes = ByType.Where(kvp => kvp.Value is not List and IOwnProgrammerInterface).Select(kvp => kvp.Key).ToArray()
            });
            Interface.Append(NewValueAdd = new("Add")
            {
                Left = new(-55, 1),
                PaddingTop = 8,
                PaddingBottom = 8,
            });
            Interface.Append(Scroll = new()
            {
                Top = new(40, 0),
                Left = new(-20, 1),
                Height = new(-46, 1)
            });
            Interface.Append(SubInterfaces = new()
            {
                Top = new(34, 0),
                Width = new(-22, 1),
                Height = new(-34, 1),

            });
            SubInterfaces.SetScrollbar(Scroll);
            NewValueAdd.OnClick += AddEntry;
        }
        public Variables.Variable WriteVariable()
        {
            VariableValue[] array = new VariableValue[Entries.Count];

            Type type = null;
            bool commonType = true;

            for (int i = 0; i < Entries.Count; i++)
            {
                Variables.Variable entry = Entries[i].Owner.WriteVariable();
                if (entry is not Constant @const) return null;
                array[i] = @const.Value;

                if (type is null) type = Entries[i].Type;
                else if (type != Entries[i].Type) commonType = false;
            }
            List list = new List(array);
            if (commonType && type is not null)
                list.CollectionType = type;

            return new Constant(list);
        }

        private void AddEntry(Terraria.UI.UIMouseEvent evt, Terraria.UI.UIElement listeningElement)
        {
            Type type = NewValueSwitch.CurrentValueType;
            if (type is null || !ByType.TryGetValue(type, out VariableValue val)) return;
            if (val is not IOwnProgrammerInterface) return;

            IOwnProgrammerInterface owner = (IOwnProgrammerInterface)val.Clone();
            owner.Interface = null;
            owner.SetupInterfaceIfNeeded();

            UIPanel p = new()
            {
                Width = new(0, 1),
                Height = new(60, 0),

                PaddingTop = 0,
                PaddingBottom = 0,
                PaddingLeft = 0,
                PaddingRight = 0,
            };
            ListEntry entry = new()
            {
                Type = type,
                Owner = owner,
                Panel = p,
            };

            p.Append(new UIDrawing()
            {
                Top = new(-16, .5f),
                Left = new(8, 0),

                OnDraw = (e, sb, st) => VariableRenderer.DrawVariableOverlay(sb, true, entry.Type, null, st.Position(), new(32), Color.White, 0f, Vector2.Zero)
            });

            p.Append(entry.Index = new(Entries.Count.ToString())
            {
                PaddingTop = 0,
                PaddingBottom = 0,
                PaddingLeft = 0,
                PaddingRight = 3,

                Top = new(-8, .5f),
                Left = new(10, 0),
                Width = new(32, 0),

                BackgroundColor = Color.Transparent,
                BorderColor = Color.Transparent,
            });

            UIPanel remove = new()
            {
                Top = new(-16, .5f),
                Left = new(-38, 1),
                Width = new(32, 0),
                Height = new(32, 0),
                PaddingTop = 5,
                PaddingBottom = 5,
                PaddingLeft = 5,
                PaddingRight = 5,
            };
            remove.Append(new UIDrawing()
            {
                Width = new(0, 1),
                Height = new(0, 1),

                OnDraw = (e, sb, st) => sb.Draw(TextureAssets.Trash.Value, st.ToRectangle(), Color.White)
            });
            p.Append(remove);

            UIPanel interf = owner.Interface;

            interf.Top = new();
            interf.Left = new(35, 0);
            interf.Width = new(-75, 1);
            interf.Height = new(0, 1);
            interf.BackgroundColor = Color.Transparent;
            interf.BorderColor = Color.Transparent;

            remove.OnClick += (e, el) => RemoveEntry(entry);

            p.Append(interf);
            SubInterfaces.Add(p);
            Entries.Add(entry);

            SoundEngine.PlaySound(SoundID.MenuTick);
        }
        private void RemoveEntry(ListEntry entry)
        {
            int index = Entries.IndexOf(entry);
            if (index == -1) return;

            Entries.RemoveAt(index);
            SubInterfaces.Remove(entry.Panel);

            for (int i = index; i < Entries.Count; i++)
            {
                Entries[i].Index.SetText(i.ToString());
            }

            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(TypeToString(CollectionType) ?? "");
            writer.Write((ushort)Values.Count());
            foreach (VariableValue value in Values)
                value.SaveData(writer);
        }
        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            Type collectionType = null;
            string type = reader.ReadString();
            if (!type.IsNullEmptyOrWhitespace())
                collectionType = StringToType(type);

            VariableValue[] values = new VariableValue[reader.ReadInt16()];
            for (int i = 0; i < values.Length; i++)
                values[i] = LoadData(reader);

            return new List(values) { CollectionType = collectionType ?? typeof(VariableValue) };
        }

        public override DisplayedValue Display()
        {
            return new ColorTextDisplay($"[\n {string.Join(",\n ", Values.Select(v => v.Display().HoverText))}\n]", Color.White);
        }

        class ListEntry
        {
            public Type Type;
            public IOwnProgrammerInterface Owner;
            public UIPanel Panel;
            public UITextPanel<string> Index;
        }
    }
}
