using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Items;
using Terraria;

namespace TerraIntegration.UI
{
    public class UIVariableSlot : UIVariable
    {
        private Variable var = null;

        public override Variable Var 
        {
            get => var;
            set
            {
                var = value;
                VariableChanged?.Invoke(var);
            }
        }
        public virtual Point WorldPos { get; set; }

        public Action<Variable> VariableChanged;

        public override void OnDeactivate()
        {
            if (Var is null || DisplayOnly) return;

            Util.DropItemInWorld(Var.Item, WorldPos.X, WorldPos.Y);
            Var = null;
        }
    }

    public class UIPlayerVariableSlot : UIVariableSlot
    {
        public override Point WorldPos => (Main.LocalPlayer.Center / 16).ToPoint();
    }
}
