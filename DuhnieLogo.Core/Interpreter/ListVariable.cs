using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Interpreter
{
    public class ListVariable : List<string>
    {
        public ListVariable() { }

        public ListVariable(IEnumerable<string> collection) : base(collection)
        {}
    }
}
