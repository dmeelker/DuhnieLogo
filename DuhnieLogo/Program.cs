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
                maak ""teller 0
                (Print 1 :teller 2)
                ";

            var tokens = Lexer.Tokenize(input);

            var interpreter = new Interpreter();
            var result = interpreter.Interpret(tokens.ToArray());

            Console.WriteLine($"Result: {result}");

            //while (true)
            //{
            //    input = Console.ReadLine();
            //    tokens = Lexer.Tokenize(input);

            //    result = interpreter.Interpret(tokens.ToArray());
            //    Console.WriteLine($"Result: {result}");
            //}

            
            Console.ReadLine();
        }
    }
}
