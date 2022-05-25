using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                writer.Write((int)error.Type);
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
                ErrorType errorType = (ErrorType)reader.ReadInt32();
                string[] args = new string[reader.ReadInt32()];

                for (int j = 0; j < args.Length; j++)
                    args[j] = reader.ReadString();

                errors[i] = new Error(errorType, args);
            }
            return new ErrorDisplay(errors);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Type, Errors);
        }

        public override bool Equals(object obj)
        {
            return obj is ErrorDisplay display &&
                   base.Equals(obj) &&
                   Type == display.Type &&
                   EqualityComparer<Error[]>.Default.Equals(Errors, display.Errors);
        }
    }
}
