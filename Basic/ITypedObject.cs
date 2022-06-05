using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraIntegration.Basic
{
    public interface ITypedObject
    {
        string TypeName { get; }
        string TypeDisplayName { get; }
        string TypeDescription { get; }

        string TypeDefaultDisplayName { get; }
        string TypeDefaultDescription { get; }

        string DescriptionLocalizationKey { get; }
        string DisplayNameLocalizationKey { get; }
    }
}
