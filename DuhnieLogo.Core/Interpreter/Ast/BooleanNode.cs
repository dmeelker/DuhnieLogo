﻿using DuhnieLogo.Core.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Interpreter.Ast
{
    class BooleanNode : Node
    {
        public bool Value { get; set; }

        public BooleanNode(TokenPosition position) : base(position)
        { }
    }
}
