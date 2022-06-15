using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI.Chat;

namespace TerraIntegration.DataStructures
{
    public class TransparentText : TextSnippet
    {
        public TransparentText(TextSnippet copyFrom)
        {
            Color = copyFrom.Color;
            Text = copyFrom.Text;
            TextOriginal = copyFrom.TextOriginal;
            CheckForHover = copyFrom.CheckForHover;
            DeleteWhole = copyFrom.DeleteWhole;
            Scale = copyFrom.Scale;
        }

        public override Color GetVisibleColor() => Color;
    }
}
