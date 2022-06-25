using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using TerraIntegration.Values;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerraIntegration.Variables
{
    public class Function : Variable, IProgrammable
    {
        public override string TypeName => "func";
        public override string TypeDefaultDisplayName => "{0}Function{1}";
        public override object[] DisplayNameFormatters => GetFormat(ReturnType, ArgTypes, true);

        public override ReturnType? VariableReturnType => ReturnTypeOf(ReturnType, ArgTypes);
        public override bool ShowReturnType => false;

        public ReturnType[] ArgTypes { get; set; }
        public ReturnType? ReturnType { get; set; }

        public Guid[] ArgIDs { get; set; }
        public Guid ReturnValueID { get; set; }

        List<Argument> Args = new();
        UIList ArgList;
        UIScrollbar Scroll;
        UITextPanel<string> NewValueAdd;
        UITextPanel Arguments;
        UIVariableSlot ReturnValueSlot;

        public UIPanel Interface { get; set; }
        public bool HasComplexInterface => true;

        readonly Dictionary<Guid, VariableValue> SetValues = new();

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            return new SpecialValue(this,
                ReturnType is null ? null :
                ArgTypes?.Length is null or 0 ? new[] { ReturnType.Value } :
                Util.Enum(ReturnType.Value).Concat(ArgTypes).ToArray());
        }

        public VariableValue Execute(ComponentSystem system, List<Error> errors)
        {
            return Execute(system, errors, 0, Enumerable.Empty<VariableValue>());
        }
        public VariableValue Execute(ComponentSystem system, List<Error> errors, VariableValue arg0)
        {
            return Execute(system, errors, 1, Util.Enum(arg0));
        }
        public VariableValue Execute(ComponentSystem system, List<Error> errors, VariableValue arg0, VariableValue arg1)
        {
            return Execute(system, errors, 2, Util.Enum(arg0, arg1));
        }
        public VariableValue Execute(ComponentSystem system, List<Error> errors, VariableValue arg0, VariableValue arg1, VariableValue arg2)
        {
            return Execute(system, errors, 3, Util.Enum(arg0, arg1, arg2));
        }
        public VariableValue Execute(ComponentSystem system, List<Error> errors, params VariableValue[] args)
        {
            return Execute(system, errors, args.Length, args);
        }

        VariableValue Execute(ComponentSystem system, List<Error> errors, int argc, IEnumerable<VariableValue> argv)
        {
            int exp = ArgIDs?.Length ?? 0;
            if (argc != exp)
            {
                errors.Add(Errors.FunctionExpectedArgsAmount(Id, exp, argc));
                return null;
            }

            foreach (var ((value, type), id) in argv.Zip(ArgTypes).Zip(ArgIDs))
            {
                ReturnType valueType = value.GetReturnType();
                if (!type.Match(valueType))
                {
                    errors.Add(Errors.ExpectedValue(type, TypeIdentity));
                    SetValues.Clear();
                    return null;
                }
                SetValues[id] = value;
            }

            foreach (var kvp in SetValues)
            {
                system.FunctionArguments[kvp.Key] = kvp.Value;
            }

            VariableValue result = system.GetVariableValue(ReturnValueID, errors);

            foreach (var kvp in SetValues)
            {
                system.FunctionArguments.Remove(kvp.Key);
            }

            return result;
        }

        public static ReturnType ReturnTypeOf(ReturnType? returnType, params ReturnType[] args)
        {
            if (returnType is null)
                return SpecialValue.ReturnTypeOf<Function>();

            if (args?.Length is null or 0)
                return SpecialValue.ReturnTypeOf<Function>(new[] { returnType.Value });

            return SpecialValue.ReturnTypeOf<Function>(Util.Enum(returnType.Value).Concat(args).ToArray());
        }

        public void SetupInterface()
        {
            Interface.PaddingTop = 6;
            Interface.PaddingBottom = 6;
            Interface.PaddingRight = 6;
            Interface.PaddingLeft = 6;

            Interface.Append(Arguments = new()
            {
                Left = new(0, .4f),
                Width = new(-55, .6f),
                Height = new(32, 0),
                Text = "Arguments:",

                PaddingTop = 0,
                PaddingBottom = 0,
                BackgroundColor = Color.Transparent,
                BorderColor = Color.Transparent,
            });

            Args.Clear();

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
            Interface.Append(ArgList = new()
            {
                Top = new(34, 0),
                Left = new(0, .4f),
                Width = new(-22, .6f),
                Height = new(-34, 1),

            });
            ArgList.SetScrollbar(Scroll);
            NewValueAdd.OnClick += AddArg;

            Interface.Append(ReturnValueSlot = new()
            {
                Top = new(-21, .5f),
                Left = new(-21, .2f),

                DisplayOnly = true,

                HoverText = "Function return value, when function is executed,\nit will set argument values and return this value",
                VariableValidator = (var) => var?.VariableReturnType is not null
            });
        }
        public Variable WriteVariable()
        {
            if (ReturnValueSlot?.Var is null)
            {
                ReturnValueSlot?.NewFloatingText("No variable here", Color.Red);
                return null;
            }

            foreach (Argument arg in Args)
                if (arg.Slot.Var?.Var?.VariableReturnType is null)
                {
                    arg.Slot?.NewFloatingText("No argument", Color.Red);
                    return null;
                }

            return new Function 
            {
                ReturnType = ReturnValueSlot.Var.Var.VariableReturnType,
                ArgTypes = Args.Select(a => a.Slot.Var.Var.VariableReturnType.Value).ToArray(),

                ReturnValueID = ReturnValueSlot.Var.Var.Id,
                ArgIDs = Args.Select(a => a.Slot.Var.Var.Id).ToArray(),
            };
        }
        public void AddArg(UIMouseEvent ev, UIElement e)
        {
            Argument arg = new();

            arg.Panel = new()
            {
                Width = new(0, 1),
                Height = new(60, 0)
            };

            arg.Panel.Append(arg.Slot = new()
            {
                Top = new(-21, .5f),
                Left = new(-21, .5f),

                DisplayOnly = true,
                HoverText = "Function argument",
                VariableValidator = (var) => var is FunctionArgument
            });

            arg.Panel.Append(arg.Index = new(Args.Count.ToString())
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
            arg.Panel.Append(remove);

            remove.OnClick += (_, _) => RemoveArg(arg);

            Args.Add(arg);
            ArgList.Add(arg.Panel);

            SoundEngine.PlaySound(SoundID.MenuTick);
        }
        void RemoveArg(Argument arg)
        {
            int index = Args.IndexOf(arg);
            if (index == -1) return;

            Args.RemoveAt(index);
            ArgList.Remove(arg.Panel);

            for (int i = index; i < Args.Count; i++)
            {
                Args[i].Index.SetText(i.ToString());
            }

            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(ReturnType?.ToTypeString() ?? "");
            writer.Write(ReturnValueID.ToByteArray());

            writer.Write(ArgTypes is null ? -1 : ArgTypes.Length);
            if (ArgTypes is not null)
                foreach (var type in ArgTypes)
                    writer.Write(type.ToTypeString());

            writer.Write(ArgIDs is null ? -1 : ArgIDs.Length);
            if (ArgIDs is not null)
                foreach (var id in ArgIDs)
                    writer.Write(id.ToByteArray());
        }
        protected override Variable LoadCustomData(BinaryReader reader)
        {
            ReturnType? returnType = DataStructures.ReturnType.FromTypeString(reader.ReadString());
            Guid returnId = new(reader.ReadBytes(16));

            ReturnType[] argTypes = null;
            Guid[] argIds = null;

            int argTypeCount = reader.ReadInt32();
            if (argTypeCount >= 0)
            {
                argTypes = new ReturnType[argTypeCount];
                for (int i = 0; i < argTypeCount; i++)
                    argTypes[i] = DataStructures.ReturnType.FromTypeString(reader.ReadString()).Value;
            }

            int argIdsCount = reader.ReadInt32();
            if (argIdsCount >= 0)
            {
                argIds = new Guid[argIdsCount];
                for (int i = 0; i < argIdsCount; i++)
                    argIds[i] = new(reader.ReadBytes(16));
            }

            return new Function
            {
                ReturnType = returnType,
                ReturnValueID = returnId,

                ArgTypes = argTypes,
                ArgIDs = argIds
            };
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ReturnValueID != default)
                tooltips.Add(new(Mod, "TIFuncRetId", $"[c/aaaa00:Return id:] {World.Guids.GetShortGuid(ReturnValueID)}"));
            if (ArgIDs?.Length is not null and > 0)
                tooltips.Add(new(Mod, "TIFuncArgId", $"[c/aaaa00:Argument ids:] {string.Join(", ", ArgIDs.Select(arg => World.Guids.GetShortGuid(arg)))}"));
        }

        public override string FormatSpecialType(ReturnType[] types, bool colored)
        {
            ReturnType? retType = types?.Length is null or 0 ? null : types[0];

            return Util.GetLangText(DisplayNameLocalizationKey, TypeDefaultDisplayName, GetFormat(retType, types?.Skip(1), colored));
        }

        string[] GetFormat(ReturnType? returnValueType, IEnumerable<ReturnType> args, bool colored)
        {
            if (returnValueType is null)
                return new[] { "", "" };

            string argStr = args is null ? "" : string.Join(", ", args.Select(t => t.ToStringName(colored)));
            string @return = returnValueType.Value.ToStringName(colored) + " ";

            return new[] { @return, $"({argStr})" };
        }

        class Argument
        {
            public UIPanel Panel;
            public UIVariableSlot Slot;
            public UITextPanel<string> Index;
        }
    }

    public class FunctionArgument : Variable, IProgrammable
    {
        public override string TypeName => "funcArg";
        public override string TypeDefaultDisplayName => "Function argument";

        UIVariableSwitch TypeSwitch;
        public UIPanel Interface { get; set; }
        public bool HasComplexInterface => false;

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            if (!system.FunctionArguments.TryGetValue(Id, out VariableValue value))
                value = null;

            if (value is null)
                errors.Add(Errors.FunctionArgumentNotSet(Id));

            return value;
        }

        public void SetupInterface()
        {
            Interface.Append(TypeSwitch = new()
            {
                Top = new(-16, .5f),
                Left = new(-16, .5f),

                SwitchValues = VariableValue.ByType.Values
                    .Where(v => !v.HideInProgrammer)
                    .Select(v => new ValueVariablePair(v.GetReturnType(), null))
                    .Append(new(null, null, null, true))
                    .ToArray()
            });

            Interface.Append(new UITextPanel()
            {
                Top = new(-48, .5f),
                Left = new(0, 0),
                Width = new(0, 1),
                Height = new(32, 0),

                PaddingTop = 0,
                PaddingBottom = 0,

                Text = "Argument type:",

                BackgroundColor = Color.Transparent,
                BorderColor = Color.Transparent,

            });
        }

        public Variable WriteVariable()
        {
            ReturnType? type = TypeSwitch?.Current?.ValueType;

            if (type is null)
                return null;

            return new FunctionArgument
            {
                VariableReturnType = type.Value
            };
        }
    }
}
