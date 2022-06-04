using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.UI;
using TerraIntegration.Values;

namespace TerraIntegration.Variables
{
    public class StringFormat : DoubleReferenceVariable
    {
        public override string Type => "strFormat";
        public override string TypeDisplay => "Format";

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 2, 2);

        public override Type[] LeftSlotValueTypes => new[] { typeof(Values.String) };
        List<string> Strings = new();

        public override string LeftSlotDescription => "Format string\nExample: The value is {0}";
        public override string RightSlotDescription => $"{VariableValue.TypeToName(typeof(IToString), true)}s format with";
        public override UIDrawing CenterDrawing => null;

        public override Type VariableReturnType => typeof(Values.String);

        public override Type[] GetValidRightSlotTypes(Type leftSlotType) => new[] { typeof(ICollection) };

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            string format = ((Values.String)left).Value;

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

            return new Values.String(string.Format(format, Strings.ToArray()));
        }
    }
}
