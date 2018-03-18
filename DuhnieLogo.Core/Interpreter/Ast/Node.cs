﻿using DuhnieLogo.Core.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Interpreter.Ast
{
    class Node
    {
        public TokenPosition Position { get; private set; }

        public Node(TokenPosition position)
        {
            Position = position;
        }
    }
}
