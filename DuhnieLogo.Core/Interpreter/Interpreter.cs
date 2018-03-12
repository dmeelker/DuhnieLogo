using DuhnieLogo.Core.Interpreter.Ast;
using DuhnieLogo.Core.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Interpreter
{
    public class Interpreter
    {
        private readonly TokenStream tokens;
        private Dictionary<string, object> memory = new Dictionary<string, object>();

        public Interpreter(Token[] tokens)
        {
            this.tokens = new TokenStream(tokens);
        }

        public object Interpret()
        {
            return StatementList();
        }

        private object StatementList()
        {
            object result = null;
            while(tokens.CurrentToken.Type != TokenType.ProgramEnd)
            {
                result = Statement();
                tokens.Eat(TokenType.NewLine);
            }

            return result;
        }

        private object Statement()
        {
            if (tokens.CurrentToken.Type == TokenType.Make)
                return VariableDefinition();
            else
                return Expression();
        }

        private object VariableDefinition()
        {
            tokens.Eat(TokenType.Make);
            tokens.Eat(TokenType.DoubleQuote);
            var name = tokens.Eat(TokenType.Word);
            var value = Expression();

            memory[name.Value] = value;
            return value;
        }

        private object Expression()
        {
            var expressionNode = ParseExpression();

            return InterpretNode(expressionNode);
        }

        private object InterpretNode(Node node)
        {
            if (node is IntegerNode)
                return ((IntegerNode)node).Value;
            if (node is BinaryOperatorNode)
                return InterpretBinaryOperator(node as BinaryOperatorNode);

            throw new Exception($"Unknown node: {node}");
        }

        private object InterpretBinaryOperator(BinaryOperatorNode binaryOperatorNode)
        {
            var left = InterpretNode(binaryOperatorNode.Left);
            var right = InterpretNode(binaryOperatorNode.Right);

            switch (binaryOperatorNode.Operator.Type)
            {
                case TokenType.Plus:
                    return (int)left + (int)right;
                case TokenType.Minus:
                    return (int)left - (int)right;
                case TokenType.Multiply:
                    return (int)left * (int)right;
                case TokenType.Divide:
                    return (int)left / (int)right;
            }

            throw new Exception($"Unknown operator {binaryOperatorNode.Operator}");
        }

        private Node ParseExpression()
        {
            return ParseAdditiveExpression();
        }

        private Node ParseAdditiveExpression()
        {
            var node = ParseMultiplicativeExpression();

            while(tokens.CurrentToken.Type == TokenType.Plus || tokens.CurrentToken.Type == TokenType.Minus)
            {
                var op = tokens.Eat(tokens.CurrentToken.Type);

                node = new BinaryOperatorNode { Left = node, Right = ParseMultiplicativeExpression(), Operator = op };
            }

            return node;
        }

        private Node ParseMultiplicativeExpression()
        {
            var node = ParseFactor();

            while (tokens.CurrentToken.Type == TokenType.Multiply || tokens.CurrentToken.Type == TokenType.Divide)
            {
                var op = tokens.Eat(tokens.CurrentToken.Type);

                node = new BinaryOperatorNode { Left = node, Right = ParseFactor(), Operator = op };
            }

            return node;
        }

        private Node ParseFactor()
        {
            if(tokens.CurrentToken.Type == TokenType.ParenthesisLeft)
            {
                tokens.Eat(TokenType.ParenthesisLeft);
                var node = ParseExpression();
                tokens.Eat(TokenType.ParenthesisRight);

                return node;
            }
            else if (tokens.CurrentToken.Type == TokenType.Integer)
            {
                var token = tokens.Eat(TokenType.Integer);
                return new IntegerNode { Value = Convert.ToInt32(token.Value) };
            }

            throw new Exception($"Unexpected token {tokens.CurrentToken}");
        }
    }
}
