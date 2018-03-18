using DuhnieLogo.Core.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Interpreter
{
    public struct ProcedureCallContext
    {
        public MemorySpace GlobalMemory;
        public Token CallToken;
    }
}
