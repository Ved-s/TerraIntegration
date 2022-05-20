using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.UI;
using TerraIntegration.Variables;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI;

namespace TerraIntegration.Components
{
    public class WireIO : Component
    {
        public override string ComponentType => "wire";
        public override string ComponentDisplayName => "Wire IO";

        public override bool CanHaveVariables => true;

        public override bool HasRightClickInterface => true;

        public override Vector2 InterfaceOffset => new(24, 0);

        List<UIComponentNamedVariable> Variables = new();

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
            SetupNewTile();
            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<Items.VarStore>();
        }

        public override void OnEvent(Point16 pos, int variableIndex)
        {
            base.OnEvent(pos, variableIndex);
            if (variableIndex == 1)
                World.WireTrips.Add(new(pos.X, pos.Y, 1, 1, 0));
        }

        public override UIPanel SetupInterface()
        {
            UIPanel p = new()
            {
                Width = new(250, 0),
                Height = new(120, 0),

                PaddingTop = 0,
                PaddingLeft = 0,
                PaddingRight = 0,
                PaddingBottom = 0,
                MarginTop = 0,
                MarginLeft = 0,
                MarginRight = 0,
                MarginBottom = 0,
                BackgroundColor = Color.Transparent,
            };

            Variables.Clear();

            Variables.Add(new()
            {
                MarginTop = 0,
                MarginLeft = 0,
                MarginRight = 0,
                MarginBottom = 0,

                VariableType = "event",
                VariableName = "On Signal",
                VariableReturnType = null,
                VariableSlot = 0,
            });

            Variables.Add(new()
            {
                Top = new(62, 0),

                MarginTop = 0,
                MarginLeft = 0,
                MarginRight = 0,
                MarginBottom = 0,

                VariableType = "eventsub",
                VariableName = "Do Signal",
                VariableReturnType = null,
                VariableSlot = 1,
            });

            foreach (UIElement e in Variables)
                p.Append(e);

            return p;
        }

        public override void UpdateInterface(Point16 pos)
        {
            foreach (UIComponentNamedVariable var in Variables)
                var.Component = new(pos, this);
        }

        public override void HitWire(int i, int j)
        {
            Point16 pos = new(i, j);

            ComponentData data = GetData(pos);
            if (data.GetVariable(0) is Event ev)
                ev.Trigger(pos, data.System);
        }
    }
}
