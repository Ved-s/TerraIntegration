using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Templates;
using Terraria.ModLoader;

namespace TerraIntegration.ValueProperties
{
    [Autoload(false)]
    public class AutoProperty<TValue, TReturn> : ValueProperty<TValue>
        where TValue : VariableValue
        where TReturn : VariableValue
    {
        public delegate TReturn PropGetterDelegate(ComponentSystem system, TValue value, List<Error> errors);

        public override string PropertyName { get; }
        public override string PropertyDisplay { get; }

        public override ReturnType? VariableReturnType { get; set; }

        public PropGetterDelegate PropGetter { get; }

        public override TReturn GetProperty(ComponentSystem system, TValue value, List<Error> errors) => PropGetter(system, value, errors);

        public AutoProperty(string name, string display, PropGetterDelegate getter)
        {
            PropertyName = name;
            PropertyDisplay = display;
            PropGetter = getter;
            VariableReturnType = typeof(TReturn);
        }

        public static void Register(AutoProperty<TValue, TReturn> prop) => Register(prop as ValueProperty);

        public override Variable NewInstance()
        {
            return new AutoProperty<TValue, TReturn>(PropertyName, PropertyDisplay, PropGetter)
            {
                PropertyDescription = PropertyDescription,
                SpriteSheetPos = SpriteSheetPos,
            };
        }
    }
}
