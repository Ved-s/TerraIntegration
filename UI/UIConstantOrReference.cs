using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Variables;
using Terraria.GameContent.UI.Elements;

namespace TerraIntegration.UI
{
    public class UIConstantOrReference : UIPanel
    {
        public Type[] ValidRefTypes
        {
            get => validRefTypes;
            set
            {
                validRefTypes = value;

                SetType(null);
                Switch.Index = 0;
                List<ValueVariablePair> switches = new();
                if (value is not null)
                {
                    switches.Add(new(null, "ref"));
                }
                if (validConstTypes is not null)
                {
                    switches.AddRange(validConstTypes
                    .SelectMany(t =>
                        t.IsInterface ?
                        VariableValue.ByType
                            .Where(kvp => kvp.Key.IsAssignableTo(t))
                            .Select(kvp => kvp.Key)
                        : new[] { t })
                    .Select(t => new ValueVariablePair(t, null, "Constant")));
                }
                Switch.SwitchValues = switches.ToArray();

            }
        }
        public Type[] ValidConstTypes
        {
            get => validConstTypes;
            set
            {
                validConstTypes = value;

                SetType(null);
                Switch.Index = 0;

                List<ValueVariablePair> switches = new();

                if (validRefTypes?.Length is not null and > 0)
                    switches.Add(new(null, "ref"));

                if (value is not null)
                {
                    switches.AddRange(value
                        .SelectMany(t =>
                            t.IsInterface ?
                            VariableValue.ByType
                                .Where(kvp => kvp.Key.IsAssignableTo(t))
                                .Select(kvp => kvp.Key)
                            : new[] { t })
                        .Select(t => new ValueVariablePair(t, null, "Constant")));
                }
                Switch.SwitchValues = switches.ToArray();
            }
        }

        UIVariableSlot RefSlot;
        IOwnProgrammerInterface Owner;
        UIVariableSwitch Switch;

        private Type[] validRefTypes;
        private Type[] validConstTypes;

        public UIConstantOrReference()
        {
            PaddingTop = 0;
            PaddingBottom = 0;
            PaddingRight = 0;
            PaddingLeft = 0;

            MinHeight = new(84, 0);

            Append(Switch = new()
            {
                Width = new(32, 0),
                Height = new(32, 0),
                Top = new(4, 0),
                Left = new(-16, .5f),
                CurrentValueChanged = (val) => SetType(val?.ValueType)
            });
            SetType(null);
        }

        public void SetType(Type valueType)
        {
            if (Owner is not null && Owner.Interface is not null)
            {
                if (valueType is not null && valueType == Owner.GetType())
                    return;
                RemoveChild(Owner.Interface);
                Owner = null;
            }

            if (RefSlot is not null)
            {
                if (valueType is null) return;
                RemoveChild(RefSlot);
                RefSlot = null;
            }

            if (valueType is null)
            {
                MinWidth = new(50, 0);
                Append(RefSlot = new()
                {
                    Top = new(-4, .5f),
                    Left = new(-21, .5f),
                    DisplayOnly = true,

                    VariableValidator = (var) => ValidRefTypes is not null && ValidRefTypes.Any(t => t.IsAssignableFrom(var.VariableReturnType)),
                    HoverText = ValidRefTypes is null ? null : string.Join(", ", ValidRefTypes.Select(t => VariableValue.TypeToName(t, true)))
                });
                return;
            }

            if (!VariableValue.ByType.TryGetValue(valueType, out var value)
                || value is not IOwnProgrammerInterface owner
                || owner.HasComplexInterface)
                return;

            Owner = (IOwnProgrammerInterface)value.Clone();
            Owner.Interface = null;
            Owner.SetupInterfaceIfNeeded();
            if (Owner.Interface is null)
            {
                Owner = null;
                return;
            }

            MinWidth = new(120, 0);

            Owner.Interface.BackgroundColor = Color.Transparent;
            Owner.Interface.BorderColor = Color.Transparent;

            Owner.Interface.MarginTop = 0;
            Owner.Interface.Width = new(0, 1);
            Owner.Interface.Height = new(-38, 1);
            Owner.Interface.Left = new(0, 0);
            Owner.Interface.Top = new(36, 0);

            Append(Owner.Interface);
        }

        public ValueOrRef GetValue()
        {
            ValueVariablePair vvp = Switch.Current.Value;
            if (vvp.ValueType is null && RefSlot?.Var?.Var is not null)
            {
                return new(RefSlot.Var.Var.Id);
            }
            if (vvp.ValueType is not null)
            {
                Variable var = Owner?.WriteVariable();
                if (var is not Constant @const) return null;

                return new(@const.Value);
            }
            return null;
        }
    }
}
