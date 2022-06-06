using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Basic.Actions;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TerraIntegration.Components
{
    public class ActionBox : Component
    {
        public override string TypeName => "action";
        public override string TypeDefaultDisplayName => "Action Box";
        public override string TypeDefaultDescription => "Action Box lets you execute actions\nwhen its event is triggered";

        public override bool CanHaveVariables => true;
        public override bool HasCustomInterface => true;

        public string SlotIds = "0123";
        public UIComponentVariable[] Slots;

        List<Error> Errors = new();

        public ActionBox() 
        {
            VariableInfo = new ComponentVariableInfo[]
            {
                new()
                {
                    AcceptVariableTypes = new[] { "eventsub" },
                    VariableName = "Run actions",
                    VariableSlot = "evt"
                }
            };
        }

        public override UIPanel SetupInterface()
        {
            UIPanel panel = new UIPanel()
            {
                Width = new(0, 1),
                MinWidth = new(214, 0),
                Height = new(58, 0),

                PaddingTop = 8
            };

            Slots = new UIComponentVariable[SlotIds.Length];

            float totWidth = Slots.Length * 52 - 10;
            float x = 0;
            for (int i = 0; i < SlotIds.Length; i++)
            {
                panel.Append(Slots[i] = new()
                {
                    Left = new(totWidth / 2 - x - 42, .5f),

                    VariableValidator = var => var is ActionVariable,
                    VariableSlot = SlotIds[i].ToString()
                });
                x += 52;
            }

            return panel;
        }
        public override void UpdateInterface(Point16 pos)
        {
            foreach (UIComponentVariable slot in Slots)
                slot.Component = new(pos, this);
        }

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
            SetupNewTile();
            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<Items.ComponentItems.ActionBox>();
        }

        public override void OnEvent(Point16 pos, string variableSlot)
        {
            if (variableSlot == "evt")
            { 
                ComponentData data = GetData(pos);
                foreach (char id in SlotIds)
                {
                    string sid = id.ToString();
                    if (data.TryGetVariable(sid, out Variable var) && var is ActionVariable action)
                    {
                        Errors.Clear();
                        action.Execute(data.System, Errors);
                    }
                }
            }
        }
    }
}
