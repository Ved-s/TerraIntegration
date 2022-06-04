using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Values;
using TerraIntegration.Variables;
using Terraria;
using Terraria.ModLoader;

namespace TerraIntegration
{
    public class DebugCommand : ModCommand
    {
        public override string Command => "ti";
        public override CommandType Type => CommandType.Chat;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            List<string> arg = new(args);

            if (arg.Count == 0)
            {
                caller.Reply("Expected subcommand: var, gen, notex, stats");
                return;
            }
            string sub = arg[0];
            arg.RemoveAt(0);
            switch (sub)
            {
                case "var":
                    VarCommand(caller, arg);
                    return;
                case "gen":
                    ModContent.GetInstance<ComponentWorld>().PostWorldGen();
                    return;
                case "notex":
                    MissingTexCommand(caller);
                    return;
                case "stats":
                    Statistics.Visible = !Statistics.Visible;
                    return;
            }

            caller.Reply("Unknown subcommand");
            return;
        }

        private void VarCommand(CommandCaller caller, List<string> arg) 
        {
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
        private void MissingTexCommand(CommandCaller caller)
        {
            List<string> values = new();
            List<string> variables = new();

            int allValues = 0, allVariables = 0;

            foreach (VariableValue val in VariableValue.ByTypeName.Values)
                if (val.Type != "any" && val.Type != "unloaded")
                {
                    allValues++;
                    if (val.Texture is null && !val.SpriteSheetPos.HasValue)
                        values.Add(val.Type);
                }

            foreach (Variable var in Variable.ByTypeName.Values)
                if (var.Type != "any" && var.Type != "unloaded")
                {
                    allVariables++;
                    if (var.Texture is null && !var.SpriteSheetPos.HasValue)
                        variables.Add(var.Type);
                }

            string valstr = values.Count == 0 ?
                    $"All values are textured ({allValues})" :
                    $"Not textured values: ({values.Count} of {allValues})\n    {string.Join(", ", values)}";

            string varstr = variables.Count == 0 ?
                    $"All variables are textured ({allVariables})" :
                    $"Not textured variables: ({variables.Count} of {allVariables})\n    {string.Join(", ", variables)}";

            caller.Reply($"{valstr}\n{varstr}");
            
        }
    }
}
