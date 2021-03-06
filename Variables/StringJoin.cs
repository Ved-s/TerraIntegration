using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces.Value;
using TerraIntegration.Templates;
using TerraIntegration.UI;
using TerraIntegration.Values;

namespace TerraIntegration.Variables
{
    public class StringJoin : DoubleReferenceVariable
    {
        public override string TypeName => "strJoin";
        public override string TypeDefaultDisplayName => "Join";
        public override string TypeDefaultDescription => "Joins collection of {0}s using\ndelimeter into one string";

        public override object[] DescriptionFormatters => new[] { VariableValue.TypeToName<IToString>() };

        public override ReturnType[] LeftSlotValueTypes => new ReturnType[] { typeof(Values.String) };
        List<string> Strings = new();

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 2, 1);

        public override string LeftSlotDescription => "Delimeter";
        public override string RightSlotDescription => $"{VariableValue.TypeToName(typeof(IToString), true)}s to join";
        public override UIDrawing CenterDrawing => null;

        public override ReturnType? VariableReturnType => typeof(Values.String);

        public override ReturnType[] GetValidRightSlotTypes(ReturnType leftSlotType) => new ReturnType[] { new(typeof(ICollection), typeof(IToString)) };

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
                    errors.Add(Errors.ExpectedValue(typeof(IToString), TypeIdentity));
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
