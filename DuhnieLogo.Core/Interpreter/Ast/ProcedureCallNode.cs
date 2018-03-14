using DuhnieLogo.Core.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Interpreter.Ast
{
    class ProcedureCallNode : Node
    {
        public Token Name { get; set; }
        public Node[] ArgumentExpressions { get; set; }
    }
}
