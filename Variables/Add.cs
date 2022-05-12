using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;
using Terraria.ModLoader;

namespace TerraIntegration.Variables
{
    public class Add : Variable
    {
        public override string Type => "add";
        public override string TypeDisplay => "Add";

        public Guid First { get; set; }
        public Guid Second { get; set; }

        public override string TypeDescription => 
            (First == default || Second == default) ? null :
            $"[c/aaaa00:Referenced IDs:] {World.Guids.GetShortGuid(First)}, {World.Guids.GetShortGuid(Second)}";

        private Type ReturnTypeCache;
        public override Type VariableReturnType => ReturnTypeCache;

        public Add() { }
        public Add(Guid first, Guid second) 
        {
            First = first;
            Second = second;
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(First.ToByteArray());
            writer.Write(Second.ToByteArray());
        }

        protected override Variable LoadCustomData(BinaryReader reader)
        {
            return new Add(new Guid(reader.ReadBytes(16)), new Guid(reader.ReadBytes(16)));
        }

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            VariableValue first = system.GetVariableValue(First, errors);
            VariableValue second = system.GetVariableValue(Second, errors);

            if (first is null || second is null) return null;

            if (first is not IAddable addable)
            {
                errors.Add(new(ErrorType.ExpectedValueWithId, "Addable", World.Guids.GetShortGuid(Id)));
                return null;
            }
            bool anyMatch = false;
            Type secondType = second.GetType();
            foreach (Type t in addable.ValidAddTypes) 
                if (secondType.IsAssignableTo(t))
                {
                    anyMatch = true;
                    break;
                }
            if (!anyMatch) 
            {
                IEnumerable<string> strings = addable.ValidAddTypes.Select(t => VariableValue.TypeToName(t, out _));
                errors.Add(new(ErrorType.ExpectedValuesWithId, string.Join(", ", strings), World.Guids.GetShortGuid(Id)));
                return null;
            }

            VariableValue result = addable.Add(second, errors);
            ReturnTypeCache = result.GetType();
            return result;
        }

        public override Variable GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: first variable id");
                return null;
            }
            if (args.Count < 2)
            {
                caller.Reply("Argument required: second variable id");
                return null;
            }

            Guid? first = World.Guids.GetGuid(args[0]);
            Guid? second = World.Guids.GetGuid(args[1]);

            if (first is null)
            {
                caller.Reply($"First id not found: {args[0]}");
                return null;
            }
            if (second is null)
            {
                caller.Reply($"Second id not found: {args[1]}");
                return null;
            }
            args.RemoveAt(0);
            args.RemoveAt(0);

            return new Add(first.Value, second.Value);
        }
    }
}
