using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Tokens
{
    static class Keywords
    {
        public static readonly string[] All = new string[] { "print", "vooruit", "achteruit", "links", "rechts", "herhaal"};

        public static bool IsKeyword(string value)
        {
            return All.Contains(value);
        }
    }
}
