using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.IO;

namespace TerraIntegration.Basic
{
    public abstract class ActionVariable : Variable, IProgrammable
    {
        internal readonly static SpriteSheet ActionsSheet = new("TerraIntegration/Assets/Types/actions", new(32, 32));

        private VariableMatch VariablesCache;
        protected VariableMatch Variables => VariablesCache ??= InitVariables;
        protected abstract VariableMatch InitVariables { get; }

        public abstract bool NeedsSaveTag { get; }

        public sealed override Type VariableReturnType => null;
        public override string DefaultItemName => "Action";
        public override string ItemNameLocalizationKey => "Mods.TerraIntegration.ItemNames.Action";

        public UIPanel Interface { get; set; }
        public abstract bool HasComplexInterface { get; }

        public UIVariableSlot ActionVarSlot { get; set; }
        public Guid ActionVarId { get; set; }

        protected override VariableMatch InitRelated => Variables;
        public override bool VisibleInProgrammerVariables => false;

        public void Execute(Point16 pos, ComponentSystem system, List<Error> errors)
        {
            Variable var = system.GetVariable(ActionVarId, errors);
            if (var is null) return;

            if (!Variables.MatchVariable(var))
            {
                errors.Add(Errors.ExpectedVariables(Variables.GetMatchDescription(), TypeIdentity));
                return;
            }
            Execute(pos, var, system, errors);
        }
        public abstract void Execute(Point16 pos, Variable var, ComponentSystem system, List<Error> errors);
        public virtual void SetupActionInterface() { }
        public virtual ActionVariable WriteActionvariable() => this.NewInstance();

        public virtual void SaveActionData(BinaryWriter writer) { }
        public virtual ActionVariable LoadActionData(BinaryReader reader) => this.NewInstance();

        public virtual TagCompound SaveActionTag() => null;
        public virtual ActionVariable LoadActionTag(TagCompound data) => this.NewInstance();

        public sealed override VariableValue GetValue(ComponentSystem system, List<Error> errors) => null;

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(ActionVarId.ToByteArray());
            SaveActionData(writer);
        }
        protected override Variable LoadCustomData(BinaryReader reader)
        {
            Guid avId = new(reader.ReadBytes(16));
            ActionVariable var = LoadActionData(reader);
            if (var is null) return null;
            var.ActionVarId = avId;
            return var;
        }

        protected override TagCompound SaveCustomTag()
        {
            if (!NeedsSaveTag) return null;
            TagCompound tag = SaveActionTag() ?? new();
            tag["acvid"] = ActionVarId.ToByteArray();
            return tag;
        }
        protected override Variable LoadCustomTag(TagCompound data)
        {
            ActionVariable var = LoadActionTag(data);
            if (var is null) return null;

            if (data.ContainsKey("acvid"))
                var.ActionVarId = new(data.GetByteArray("acvid"));

            return var;
        }

        public void SetupInterface()
        {
            Interface.Append(ActionVarSlot = new()
            {
                Left = new(-21, .5f),
                Top = new(-21, .5f),
                DisplayOnly = true,

                VariableValidator = var => Variables.MatchVariable(var),
                HoverText = Variables.GetMatchDescription(),
            });
            SetupActionInterface();
        }
        public Variable WriteVariable()
        {
            if (ActionVarSlot?.Var?.Var is null)
                return null;

            ActionVariable var = WriteActionvariable();
            if (var is null)
                return null;

            var.ActionVarId = ActionVarSlot.Var.Var.Id;
            return var;
        }
    }
}
