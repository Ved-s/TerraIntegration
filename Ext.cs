using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraIntegration
{
    public static class Ext
    {
        public static bool IsNullEmptyOrWhitespace(this string str) => string.IsNullOrWhiteSpace(str) || str.Length == 0;
    }
}
