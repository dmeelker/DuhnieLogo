using DuhnieLogo.Core.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Interpreter
{
    class CustomProcedureInfo : ProcedureInfo
    {
        public Token[] Tokens { get; set; }
    }
}
