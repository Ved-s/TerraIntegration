﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraIntegration.Interfaces
{
    public interface INamed : IValueInterface
    {
        string Name { get; }
    }
}
