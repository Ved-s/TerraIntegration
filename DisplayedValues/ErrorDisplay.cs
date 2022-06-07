using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;

namespace TerraIntegration.DisplayedValues
{
    public class ErrorDisplay : ColorTextDisplay
    {
        public override string Type => "error";

        public override string Text => $"Errors:\n{string.Join('\n', (object[])Errors)}";
        public override Color Color => Color.OrangeRed;

        Error[] Errors;

        public ErrorDisplay() { }
        public ErrorDisplay(Error[] errors) 
        {
            Errors = errors;
        }

        protected override void SendCustomData(BinaryWriter writer)
        {
            writer.Write(Errors.Length);
            foreach (Error error in Errors)
            {
                writer.Write(error.Type);
                writer.Write(error.Args.Length);
                foreach (string arg in error.Args)
                    writer.Write(arg);
            }
        }

        protected override DisplayedValue ReceiveCustomData(BinaryReader reader)
        {
            Error[] errors = new Error[reader.ReadInt32()];
            for (int i = 0; i < errors.Length; i++)
            {
                string errorType = reader.ReadString();
                string[] args = new string[reader.ReadInt32()];

                for (int j = 0; j < args.Length; j++)
                    args[j] = reader.ReadString();

                errors[i] = new Error(errorType, args);
            }
            return new ErrorDisplay(errors);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Errors);
        }

        public override bool Equals(DisplayedValue value)
        {
            return value is ErrorDisplay display &&
                   Errors.SequenceEqual(display.Errors);
        }
    }
}
