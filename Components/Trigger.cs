using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace TerraIntegration.Components
{
    public class Trigger : Component<Trigger.TriggerData>
    {
        public override string TypeName => "trigger";
        public override string TypeDefaultDisplayName => "Trigger";
        public override string TypeDefaultDescription => "Triggers an event by condition";

        public override bool HasCustomInterface => true;
        public override bool CanHaveVariables => true;

        public override bool ConfigurableFrequency => true;
        public override ushort DefaultUpdateFrequency => 60;

        UIComponentVariable TriggerVar, ActionVar;
        UISwitch ModeSwitch;

        public override void OnUpdate(Point16 pos)
        {
            base.OnUpdate(pos);

            if (Networking.Client) return;
            TriggerData td = GetData(pos);

            Variable trig = td.GetVariable("trig");

            if (trig is null)
            {
                td.Value = null;
                return;
            }

            td.LastErrors.Clear();
            VariableValue value = trig.GetValue(td.System, td.LastErrors);
            trig.SetLastValue(value, td.System);
            if (value is not Values.Boolean @bool || td.LastErrors.Count > 0)
            {
                td.Value = null;
                return;
            }

            bool newValue = @bool.Value;

            Variable act = td.GetVariable("act");

            if (td.Value is not null && act is Variables.Event evt)
            {
                bool oldValue = td.Value.Value;
                switch (td.Mode)
                {
                    case 0 when newValue && !oldValue:
                    case 1 when !newValue && oldValue:
                    case 2 when newValue != oldValue:
                        evt.Trigger(pos, td.System);
                        break;
                }
            }
            td.Value = newValue;
        }

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;

            SetupNewTile();
            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<Items.ComponentItems.Trigger>();
        }

        public override UIPanel SetupInterface()
        {
            UIPanel panel = new()
            {
                Width = new(0, 1),
                MinWidth = new(180, 0),
                MinHeight = new(60, 0),
            };
            panel.SetPadding(0);

            Texture2D switchTex = ModContent.Request<Texture2D>("TerraIntegration/Assets/UI/TriggerMode", AssetRequestMode.ImmediateLoad).Value;
            panel.Append(ModeSwitch = new()
            {
                Width = new(32, 0),
                Height = new(32, 0),

                Top = new(-16, .5f),
                Left = new(-16, .5f),

                States = new UISwitch.SwitchState[] 
                {
                    new(switchTex, new(0,0,32,32),  "Trigger on became True"),
                    new(switchTex, new(32,0,32,32), "Trigger on became False"),
                    new(switchTex, new(64,0,32,32), "Trigger on value change"),
                },
                StateChanged = (state, ind) =>
                {
                    GetData(InterfacePos).Mode = (byte)ind;
                }
            });

            panel.Append(TriggerVar = new()
            {
                VariableSlot = "trig",
                Top = new(-21, .5f),
                Left = new(-66, .5f),

                VariableValidator = (var) => var.VariableReturnType == typeof(Values.Boolean),
                HoverText = $"{VariableValue.TypeToName<Values.Boolean>()} value to check"
            });

            panel.Append(ActionVar = new()
            {
                VariableSlot = "act",
                Top = new(-21, .5f),
                Left = new(24, .5f),

                VariableValidator = (var) => var is Variables.Event,
                HoverText = "Event to trigger"
            });

            return panel;
        }
        public override void UpdateInterface(Point16 pos)
        {
            TriggerData td = GetData(pos);

            TriggerVar.Component = new(pos, this);
            ActionVar.Component = new(pos, this);
            ModeSwitch.StateIndex = td.Mode;
        }

        public override void SendCustomData(TriggerData data, BinaryWriter writer)
        {
            writer.Write(data.Mode);
        }
        public override TriggerData ReceiveCustomData(BinaryReader reader, Point16 pos)
        {
            return new() { Mode = reader.ReadByte() };
        }

        public override TagCompound SaveCustomDataTag(TriggerData data)
        {
            return new() { ["mode"] = data.Mode };
        }
        public override TriggerData LoadCustomDataTag(TagCompound data, Point16 pos)
        {
            TriggerData td = new();
            if (data.TryGet("mode", out byte mode))
                td.Mode = mode;

            return td;
        }

        public class TriggerData : ComponentData
        {
            public byte Mode;
            public bool? Value = null;
        }
    }
}
