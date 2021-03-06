using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.UI;
using TerraIntegration.Values;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.IO;

namespace TerraIntegration.Basic
{
    public abstract class ActionVariable : Variable, IProgrammable
    {
        internal readonly static SpriteSheet ActionsSheet = new("TerraIntegration/Assets/Types/actions", new(32, 32));

        public abstract VariableMatch Variables { get; }
        public abstract bool NeedsSaveTag { get; }

        public sealed override ReturnType? VariableReturnType => SpecialValue.ReturnTypeOf(GetType());
        public override string DefaultItemName => "Action";
        public override string ItemNameLocalizationKey => "Mods.TerraIntegration.ItemNames.Action";

        public UIPanel Interface { get; set; }
        public abstract bool HasComplexInterface { get; }

        public UIVariableSlot ActionVarSlot { get; set; }
        public Guid ActionVarId { get; set; }

        public override IEnumerable<Type> RelatedTypes => Variables.ToTypeArray();
        public override bool VisibleInProgrammerVariables => false;

        public void Execute(Point16 pos, ComponentSystem system, List<Error> errors)
        {
            Variable var = system.GetVariable(ActionVarId, errors);
            if (var is null) return;

            VariableMatch match = Variables;

            if (!match.Match(var))
            {
                errors.Add(Errors.ExpectedVariables(match.ToTypeNameString(), TypeIdentity));
                return;
            }
            Execute(pos, var, system, errors);
        }
        public abstract void Execute(Point16 pos, Variable var, ComponentSystem system, List<Error> errors);
        public virtual void SetupActionInterface() { }
        public virtual ActionVariable WriteActionvariable() => (ActionVariable)NewInstance();

        public virtual void SaveActionData(BinaryWriter writer) { }
        public virtual ActionVariable LoadActionData(BinaryReader reader) => (ActionVariable)NewInstance();

        public virtual TagCompound SaveActionTag() => null;
        public virtual ActionVariable LoadActionTag(TagCompound data) => (ActionVariable)NewInstance();

        public sealed override VariableValue GetValue(ComponentSystem system, List<Error> errors) => new SpecialValue(this);

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

                VariableValidator = var => Variables.Match(var),
                HoverText = Variables.ToTypeNameString(),
            });
            SetupActionInterface();
        }
        public Variable WriteVariable()
        {
            if (ActionVarSlot?.Var?.Var is null)
            {
                ActionVarSlot?.NewFloatingText(TerraIntegration.Localize("ProgrammingErrors.NoVariable"), Color.Red);
                return null;
            }

            ActionVariable var = WriteActionvariable();
            if (var is null)
                return null;

            var.ActionVarId = ActionVarSlot.Var.Var.Id;
            return var;
        }
    }
}
