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
                leer Add :a :b
                    0 + :a + :b
                eind

                Add 10*5 5";

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
