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
            var input = @"
                leer Geef10
                    Print
                    10
                eind

                leer Plus5
                    Geef10 + 5
                eind

                10 + Plus5";

            var tokens = Lexer.Tokenize(input);

            var interpreter = new Interpreter(tokens.ToArray());
            var result = interpreter.Interpret();

            Console.WriteLine($"Result: {result}");
            Console.ReadLine();
        }
    }
}
