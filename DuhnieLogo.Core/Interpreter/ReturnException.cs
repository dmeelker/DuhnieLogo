﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Interpreter
{
    class ReturnException : Exception
    {
        public object Value { get; set; }
    }
}
