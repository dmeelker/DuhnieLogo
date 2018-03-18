using DuhnieLogo.Core.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Interpreter.Ast
{
    class IntegerNode : Node
    {
        public int Value { get; set; }

        public IntegerNode(TokenPosition position) : base(position)
        { }
    }
}
