﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Interpreter
{
    class BuiltInProcedure : ProcedureInfo
    {
        public Func<MemorySpace, object[], object> Implementation { get; set; }
    }
}
