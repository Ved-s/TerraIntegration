using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Variables;
using Terraria;
using Terraria.ModLoader;

namespace TerraIntegration
{
    public class DebugCommand : ModCommand
    {
        public override string Command => "tivar";
        public override CommandType Type => CommandType.Chat;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            List<string> arg = new(args);
            Guid? id = null;

            if (arg.Count < 1)
            {
                caller.Reply("Argument required: variable type or #id");
                return;
            }

            if (arg[0].StartsWith("#"))
            {
                string sid = arg[0][1..];
                if (sid.StartsWith('!'))
                    id = ModContent.GetInstance<ComponentWorld>().Guids.GetGuid(sid[1..]);
                else id = ModContent.GetInstance<ComponentWorld>().Guids.GetUniqueId(sid);
                arg.RemoveAt(0);

                if (arg.Count < 1)
                {
                    caller.Reply("Argument required: variable type");
                    return;
                }
            }

            if (!Variable.ByTypeName.TryGetValue(arg[0], out Variable var))
            {
                caller.Reply($"Argument required: unregistered variable type {arg[0]}");
                return;
            }
            arg.RemoveAt(0);

            Variable res = var.GetFromCommand(caller, arg);
            if (res is null)
            {
                caller.Reply($"No result");
                return;
            }
            if (id.HasValue) res.Id = id.Value;

            Item i = Items.Variable.CreateVarItem(res);

            Vector2 vec = Main.LocalPlayer.Center;

            Util.DropItemInWorld(i, (int)vec.X, (int)vec.Y);
        }
    }
}
