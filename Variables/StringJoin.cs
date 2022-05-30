using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;

namespace TerraIntegration.Variables
{
    public class StringJoin : DoubleReferenceVariable
    {
        public override string Type => "strJoin";
        public override string TypeDisplay => "Join";

        public override Type[] LeftSlotValueTypes => new[] { typeof(Values.String) };
        List<string> Strings = new();

        public override Type VariableReturnType => typeof(Values.String);

        public override Type[] GetValidRightSlotTypes(Type leftSlotType) => new[] { typeof(ICollection) };

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            string delim = ((Values.String)left).Value;

            Strings.Clear();
            bool success = true;

            foreach (VariableValue val in ((ICollection)right).Enumerate(system, errors))
            {
                if (val is null)
                {
                    success = false;
                    continue;
                }
                if (val is not IToString toString)
                {
                    errors.Add(new(
                        ErrorType.ExpectedValueWithId,
                        VariableValue.TypeToName(typeof(IToString), false),
                        World.Guids.GetShortGuid(Id)));
                    success = false;
                    continue;
                }

                Strings.Add(toString.ToString());
            }

            if (!success) return null;

            return new Values.String(string.Join(delim, Strings));
        }
    }
}
