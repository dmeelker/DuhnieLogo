﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Interpreter.Ast
{
    class ListNode : Node
    {
        public List<string> Values { get; set; }
    }
}