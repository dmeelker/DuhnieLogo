using DuhnieLogo.Core.Interpreter;
using DuhnieLogo.Core.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = @"maak ""test (10 + 5) * 2
maak ""aap 1
10 + 1
";

            var tokens = Lexer.Tokenize(input);

            var interpreter = new Interpreter(tokens.ToArray());
            var result = interpreter.Interpret();

            Console.WriteLine($"Result: {result}");
            Console.ReadLine();
        }
    }
}
