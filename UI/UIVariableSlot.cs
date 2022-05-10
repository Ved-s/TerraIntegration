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
                VariableChanged?.Invoke();
            }
        }
        public Point WorldPos { get; set; }

        public event Action VariableChanged;

        public override void OnDeactivate()
        {
            if (Var is null || DisplayOnly) return;

            Util.DropItemInWorld(Var.Item, WorldPos.X, WorldPos.Y);
            Var = null;
        }
    }
}
