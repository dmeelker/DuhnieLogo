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

        private Dictionary<string, ProcedureInfo> procedures = new Dictionary<string, ProcedureInfo>();
        private Stack<int> locationStack = new Stack<int>();

        public Interpreter(Token[] tokens)
        {
            this.tokens = new TokenStream(tokens);

            procedures.Add("Print", new BuiltInProcedure() { Name = "Print" });
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
            }

            return result;
        }

        private object Statement()
        {
            if (tokens.CurrentToken.Type == TokenType.Make)
                return VariableDefinition();
            else if (tokens.CurrentToken.Type == TokenType.Learn)
            {
                ProcedureDeclaration();
                return null;
            }
            else
                return Expression();
        }

        private void ProcedureDeclaration()
        {
            var procedureInfo = new CustomProcedureInfo();

            tokens.Eat(TokenType.Learn);
            
            procedureInfo.Name = tokens.Eat(TokenType.Word).Value;
            procedureInfo.Location = tokens.Location;

            while (tokens.CurrentToken.Type != TokenType.End)
                tokens.Eat();

            tokens.Eat(TokenType.End);

            procedures[procedureInfo.Name] = procedureInfo;
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
            if (node is VariableNode)
                return memory[((VariableNode)node).Name];
            if (node is ProcedureCallNode)
                return InterpretProcedureCall(node as ProcedureCallNode);
            if (node is BinaryOperatorNode)
                return InterpretBinaryOperator(node as BinaryOperatorNode);

            throw new Exception($"Unknown node: {node}");
        }

        private object InterpretProcedureCall(ProcedureCallNode node)
        {
            var procedure = procedures[node.Name];

            if (procedure is CustomProcedureInfo)
            {
                var customProcedure = procedure as CustomProcedureInfo;
                locationStack.Push(tokens.Location);

                tokens.Seek(customProcedure.Location);
                object result = null;

                while (tokens.CurrentToken.Type != TokenType.End)
                    result = Statement();

                tokens.Eat(TokenType.End);

                tokens.Seek(locationStack.Pop());
                return result;
            }
            else if(procedure is BuiltInProcedure)
            {
                var builtInProcedure = procedure as BuiltInProcedure;

                return null;
            }

            throw new Exception($"Unknown procedure type {procedure}");
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
            else if (tokens.CurrentToken.Type == TokenType.Colon)
            {
                tokens.Eat(TokenType.Colon);
                var variableName = tokens.Eat(TokenType.Word).Value;
                
                return new VariableNode { Name = variableName };
            }
            else if (tokens.CurrentToken.Type == TokenType.Word)
            {
                var name = tokens.Eat(TokenType.Word).Value;

                return new ProcedureCallNode { Name = name };
            }

            throw new Exception($"Unexpected token {tokens.CurrentToken}");
        }
    }
}
