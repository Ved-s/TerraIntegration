using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TerraIntegration.Basic;
using TerraIntegration.Components;
using Terraria;
using Terraria.Localization;
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

            if (!TerraIntegration.DebugMode)
            {
                NonDebugAction(caller, arg);
                return;
            }

            if (arg.Count == 0)
            {
                caller.Reply("Expected subcommand: var, gen, todo, stats, comdbg");
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
                    ComponentWorld.Instance.PostWorldGen();
                    return;
                case "todo":
                    ToDoCommand(caller);
                    return;
                case "stats":
                    Statistics.Visible = !Statistics.Visible;
                    return;
                case "comdbg":
                    ComponentWorld.Instance.ComponentDebug = !ComponentWorld.Instance.ComponentDebug;
                    return;
            }

            caller.Reply("Unknown subcommand");
            return;
        }

        public void NonDebugAction(CommandCaller caller, List<string> arg)
        {
            if (arg.Count == 0)
            {
                caller.Reply("Expected subcommand: stats, comdbg");
                return;
            }
            string sub = arg[0];
            arg.RemoveAt(0);
            switch (sub)
            {
                case "stats":
                    Statistics.Visible = !Statistics.Visible;
                    return;
                case "comdbg":
                    ComponentWorld.Instance.ComponentDebug = !ComponentWorld.Instance.ComponentDebug;
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
                    id = ComponentWorld.Instance.Guids.GetGuid(sid[1..]);
                else id = ComponentWorld.Instance.Guids.GetUniqueId(sid);
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
        private void ToDoCommand(CommandCaller caller)
        {
            List<string> valueTextures = new();
            List<string> variableTextures = new();
            List<string> valueDescriptions = new();
            List<string> variableDescriptions = new();
            List<string> componentDescriptions = new();

            int allValues = 0,
                allVariables = 0,
                allComponents = 0;

            foreach (VariableValue val in VariableValue.ByTypeName.Values)
                if (val.TypeName != "unloaded")
                {
                    allValues++;
                    if (val.Texture is null && !val.SpriteSheetPos.HasValue)
                        valueTextures.Add(val.TypeName);
                    if (val.TypeDescription is null)
                        valueDescriptions.Add(val.TypeName);
                }

            foreach (Variable var in Variable.ByTypeName.Values)
                if (var.TypeName != "unloaded")
                {
                    allVariables++;
                    if (var.Texture is null && !var.SpriteSheetPos.HasValue)
                        variableTextures.Add(var.TypeName);
                    if (var.TypeDescription is null)
                        variableDescriptions.Add(var.TypeName);
                }

            foreach (Component com in Component.ByType.Values)
                if (com.TypeName != "unloaded")
                {
                    allComponents++;
                    if (com.TypeDescription is null)
                        componentDescriptions.Add(com.TypeName);
                }

            caller.Reply(valueTextures.Count == 0 ?
                    $"All values are textured ({allValues})" :
                    $"Not textured values: ({valueTextures.Count} of {allValues})\n    {string.Join(", ", valueTextures)}");

            caller.Reply(variableTextures.Count == 0 ?
                    $"All variables are textured ({allVariables})" :
                    $"Not textured variables: ({variableTextures.Count} of {allVariables})\n    {string.Join(", ", variableTextures)}");

            caller.Reply(valueDescriptions.Count == 0 ?
                    $"All values have descriptions ({allValues})" :
                    $"Values without description: ({valueDescriptions.Count} of {allValues})\n    {string.Join(", ", valueDescriptions)}");

            caller.Reply(variableDescriptions.Count == 0 ?
                    $"All variables have descriptions ({allVariables})" :
                    $"Variables without description: ({variableDescriptions.Count} of {allVariables})\n    {string.Join(", ", variableDescriptions)}");

            caller.Reply(componentDescriptions.Count == 0 ?
                    $"All components have descriptions ({allComponents})" :
                    $"Components without description: ({componentDescriptions.Count} of {allComponents})\n    {string.Join(", ", componentDescriptions)}");
        }
    }
}
