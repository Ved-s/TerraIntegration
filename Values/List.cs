using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.DisplayedValues;
using TerraIntegration.Interfaces;
using TerraIntegration.Interfaces.Value;
using TerraIntegration.Items;
using TerraIntegration.UI;
using TerraIntegration.Utilities;
using TerraIntegration.Variables;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class List : VariableValue, ICollection, IEquatable, IProgrammable
    {
        public override string TypeName => "list";
        public override string TypeDefaultDisplayName => "List";
        public override string TypeDefaultDescription => "List of values";

        public override Color TypeColor => new(120, 120, 120);

        public override SpriteSheetPos SpriteSheetPos => new(ValueSheet, 0, 2);

        public ReturnType CollectionType { get; set; } = typeof(VariableValue);

        public UIPanel Interface { get; set; }
        public bool HasComplexInterface => true;

        List<ListEntry> Entries = new();
        UIList SubInterfaces;
        UIScrollbar Scroll;
        UIVariableSwitch NewValueSwitch;
        UITextPanel<string> NewValueAdd;

        public ListValueEntry[] Values;
        List<Error> Errors = new();
        List<string> Strings = new();

        public List() { }
        public List(ListValueEntry[] values)
        {
            Values = values;
        }

        public IEnumerable<VariableValue> Enumerate(ComponentSystem system, List<Error> errors) 
        {
            if (Values is null) yield break;

            foreach (ListValueEntry value in Values)
            {
                if (!value.IsRef)
                {
                    yield return value.Value;
                    continue;
                }

                yield return system.GetVariableValue(value.Id, errors);
            }
        }

        public void SetupInterface()
        {
            Interface.PaddingTop = 6;
            Interface.PaddingBottom = 6;
            Interface.PaddingRight = 6;
            Interface.PaddingLeft = 6;
            
            Entries.Clear();

            List<ValueVariablePair> switches = new() { new(null, "ref") };
            switches.AddRange(from kvp in ByType
                              where kvp.Value is IProgrammable owner && !owner.HasComplexInterface
                              select new ValueVariablePair(kvp.Key, "const"));

            Interface.Append(NewValueSwitch = new()
            {
                Left = new(-96, 1),

                SwitchValues = switches.ToArray()
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
        public Basic.Variable WriteVariable()
        {
            ListValueEntry[] array = new ListValueEntry[Entries.Count];

            ReturnType? type = null;
            bool commonType = true;

            for (int i = 0; i < Entries.Count; i++)
            {
                Basic.Variable entry = Entries[i].Owner.WriteVariable();

                if (entry is Reference @ref)
                {
                    array[i] = new(true, null, @ref.VariableId);
                    ReturnType? refType = @ref.VariableReturnType;

                    if (type is null) type = refType;
                    else if (type != refType) commonType = false;
                    continue;
                }

                if (entry is not Constant @const) return null;
                array[i] = new(false, @const.Value, default);

                if (type is null) type = Entries[i].Type;
                else if (type != Entries[i].Type) commonType = false;
            }
            List list = new List(array);
            if (commonType && type is not null)
                list.CollectionType = type.Value;

            return new Constant(list);
        }

        private void AddEntry(Terraria.UI.UIMouseEvent evt, Terraria.UI.UIElement listeningElement)
        {
            ValueVariablePair current = NewValueSwitch.Current.Value;
            Type entryType = current.ValueType?.Type;

            IProgrammable owner;
            if (current.VariableType == "ref")
            {
                if (!Basic.Variable.ByTypeName.TryGetValue(current.VariableType, out var var)
                    || var is not IProgrammable) return;
                owner = (IProgrammable)var.Clone();
                entryType = null;
            }
            else
            {
                if (entryType is null || !ByType.TryGetValue(entryType, out VariableValue val)) return;
                if (val is not IProgrammable) return;
                owner = (IProgrammable)val.Clone();
            }

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
                Type = entryType,
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

        public override string FormatReturnType(ReturnType type, bool colored)
        {
            if (type.SubType?.Length is null or 0 || type.SubType[0].Type == typeof(VariableValue))
                return Util.ColorTag(TypeColor, TypeDisplayName);

            return $"{Util.ColorTag(TypeColor, TypeDisplayName)} of {type.SubType[0].ToStringName(colored)}";
        }

        public override ReturnType GetReturnType()
        {
            return new(typeof(List), CollectionType);
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(CollectionType.ToTypeString());
            writer.Write((ushort)Values.Count());
            foreach (ListValueEntry value in Values)
            {
                writer.Write(value.IsRef);
                if (value.IsRef)
                    writer.Write(value.Id.ToByteArray());
                else
                    SaveData(value.Value, writer);
            }
        }
        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            Type collectionType = null;
            string type = reader.ReadString();
            if (!type.IsNullEmptyOrWhitespace())
                collectionType = StringToType(type);

            ListValueEntry[] values = new ListValueEntry[reader.ReadInt16()];
            for (int i = 0; i < values.Length; i++)
            {
                bool isRef = reader.ReadBoolean();
                if (isRef)
                    values[i] = new(isRef, null, new(reader.ReadBytes(16)));
                else 
                    values[i] = new(isRef, LoadData(reader), default);
            }

            return new List(values) { CollectionType = collectionType ?? typeof(VariableValue) };
        }

        public override DisplayedValue Display(ComponentSystem system)
        {
            Strings.Clear();
            Errors.Clear();

            foreach (var v in Values)
            {
                VariableValue value = v.Value;

                if (v.IsRef)
                {
                    if (system is null)
                    {
                        Strings.Add($"Ref {ComponentWorld.Instance.Guids.GetShortGuid(v.Id)}");
                        continue;
                    }
                    else value = system.GetVariableValue(v.Id, Errors);
                }
                Strings.Add(value?.Display(system).HoverText);
            }

            if (Errors.Count > 0)
                return new ErrorDisplay(Errors.ToArray());

            return new ColorTextDisplay($"[\n {string.Join(",\n ", Strings)}\n]", Color.White)
            {
                TextAlign = new(0, .5f)
            };
        }

        public override bool Equals(VariableValue value)
        {
            foreach (var (First, Second) in (value as List).Values.Zip(Values)) 
            {
                if (First != Second)
                    return false;

                if (First.GetType() != Second.GetType()) 
                    return false;

                if (First.Value is IEquatable equatable)
                    return equatable.Equals(Second);

                return false;
            }
            return true;
        }

        class ListEntry
        {
            public Type Type;
            public IProgrammable Owner;
            public UIPanel Panel;
            public UITextPanel<string> Index;
        }
        public record struct ListValueEntry(bool IsRef, VariableValue Value, Guid Id);
    }
}
