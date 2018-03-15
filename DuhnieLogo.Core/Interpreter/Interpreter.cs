﻿using DuhnieLogo.Core.Interpreter.Ast;
using DuhnieLogo.Core.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Interpreter
{
    public class Interpreter
    {
        private Stack<TokenStream> tokenStack = new Stack<TokenStream>();
        private TokenStream tokens;

        private Stack<MemorySpace> memoryStack = new Stack<MemorySpace>();
        private MemorySpace globalMemory;
        private MemorySpace memory;

        private Dictionary<string, ProcedureInfo> procedures = new Dictionary<string, ProcedureInfo>();
        private readonly Random random = new Random();

        public Interpreter()
        {
            RegisterFunction("herhaal",  new string[] { "count", "commands" }, (_globalMemory, _arguments) => {
                var memorySpace = new MemorySpace(memory);
                PushMemorySpace(memorySpace);

                var tokens = Lexer.Tokenize(string.Join(" ", (List<string>)_arguments[1])).ToArray();

                for(int i=0; i<(int) _arguments[0]; i++)
                {
                    memorySpace.Set("iteratie", i+1);
                    Interpret(tokens);
                }

                PopMemorySpace();

                return null;
            });

            RegisterFunction("telherhaal", new string[] {}, (_globalMemory, _arguments) => {
                if (!memory.Contains("iteratie"))
                    throw new ScriptException("Telherhaal kan alleen binnen een herhaal opdracht gebruikt worden");

                return memory.Get("iteratie");
            });

            RegisterFunction("lijst", new string[] { "arg1", "arg2" }, (_globalMemory, _arguments) => {
                var result = new List<string>();

                foreach (var arg in _arguments)
                    result.Add(arg.ToString());

                return result;
            });

            RegisterFunction("woord", new string[] { "arg1", "arg2" }, (_globalMemory, _arguments) => {
                var result = new StringBuilder();

                foreach (var arg in _arguments)
                    result.Append(arg.ToString());

                return result.ToString();
            });

            RegisterFunction("gok", new string[] { "maximum" }, (_globalMemory, _arguments) => {
                var max = (int)_arguments[0];
                return random.Next(max - 1);
            });

            RegisterFunction("wacht", new string[] { "duration" }, (_globalMemory, _arguments) => {
                Thread.Sleep((int)_arguments[0]);
                return null;
            });

            globalMemory = new MemorySpace();
            memory = globalMemory;
        }

        public void RegisterFunction(string name, string[] arguments, Func<MemorySpace, object[], object> implementation)
        {
            procedures.Add(name.ToLower(), new BuiltInProcedure { Name = name, Arguments = arguments, Implementation = implementation });
        }

        private void PushTokenStream(TokenStream tokenStream)
        {
            tokenStack.Push(tokens);
            tokens = tokenStream;
        }

        private void PopTokenStream()
        {
            tokens = tokenStack.Pop();
        }

        private void PushMemorySpace(MemorySpace memory)
        {
            memoryStack.Push(this.memory);
            this.memory = memory;
        }

        private void PopMemorySpace()
        {
            this.memory = memoryStack.Pop();
        }

        public object Interpret(Token[] tokens)
        {
            PushTokenStream(new TokenStream(tokens));
            object result = null;
            try
            {
                StatementList();
            }catch(ReturnException ex)
            {
                result = ex.Value;
            }

            PopTokenStream();
            return result;
        }

        private object StatementList()
        {
            while(tokens.CurrentToken.Type != TokenType.ProgramEnd)
                Statement();

            return null;
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
            else if (tokens.CurrentToken.Type == TokenType.Return)
                return ProcessReturn();
            else if (tokens.CurrentToken.Type == TokenType.Stop)
                return ProcessStop();
            else
                return Expression();
        }

        private void ProcedureDeclaration()
        {
            var procedureInfo = new CustomProcedureInfo();

            tokens.Eat(TokenType.Learn);
            
            procedureInfo.Name = tokens.Eat(TokenType.Identifier).Value;

            var arguments = new List<string>();
            while(tokens.CurrentToken.Type == TokenType.Colon)
            {
                tokens.Eat(TokenType.Colon);
                arguments.Add(tokens.Eat(TokenType.Identifier).Value);
            }
            procedureInfo.Arguments = arguments.ToArray();

            var procTokens = new List<Token>();

            while (tokens.CurrentToken.Type != TokenType.End)
                procTokens.Add(tokens.Eat());

            procTokens.Add(new Token(TokenType.ProgramEnd, "", new TokenPosition(0, 0)));

            tokens.Eat(TokenType.End);
            procedureInfo.Tokens = procTokens.ToArray();

            procedures[procedureInfo.Name.ToLower()] = procedureInfo;
        }

        private object ProcessReturn()
        {
            tokens.Eat(TokenType.Return);
            var result = Expression();

            throw new ReturnException { Value = result };
        }

        private object ProcessStop()
        {
            tokens.Eat(TokenType.Stop);
            throw new ReturnException();
        }

        private object VariableDefinition()
        {
            tokens.Eat(TokenType.Make);
            var name = tokens.Eat(TokenType.StringLiteral); ;
            var value = Expression();

            memory.Set(name.Value, value);
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
            if (node is StringLiteralNode)
                return ((StringLiteralNode)node).Value.Value;
            if (node is VariableNode)
            {
                var variableNode = node as VariableNode;
                if (memory.Contains(variableNode.Name.Value))
                    return memory.Get(variableNode.Name.Value);
                else
                    throw new ScriptException($"Onbekende variabele '{variableNode.Name.Value}'", variableNode.Name);
            }
            if (node is ListNode)
                return InterpretListNode(node as ListNode);
            if (node is ProcedureCallNode)
                return InterpretProcedureCall(node as ProcedureCallNode);
            if (node is BinaryOperatorNode)
                return InterpretBinaryOperator(node as BinaryOperatorNode);

            throw new Exception($"Unknown node: {node}");
        }

        private object InterpretListNode(ListNode listNode)
        {
            return listNode.Values;
        }

        private object InterpretProcedureCall(ProcedureCallNode node)
        {
            var procedure = ResolveProcedure(node.Name);

            if (procedure is CustomProcedureInfo)
            {
                var customProcedure = procedure as CustomProcedureInfo;

                var memorySpace = new MemorySpace(memory);
                for (int i = 0; i < customProcedure.Arguments.Length; i++)
                    memorySpace.Set(customProcedure.Arguments[i], InterpretNode(node.ArgumentExpressions[i]));

                PushMemorySpace(memorySpace);
                var result = Interpret(customProcedure.Tokens);
                PopMemorySpace();

                return result;
            }
            else if(procedure is BuiltInProcedure)
            {
                var builtInProcedure = procedure as BuiltInProcedure;

                var arguments = node.ArgumentExpressions.Select(arg => InterpretNode(arg)).ToArray();
                return builtInProcedure.Implementation(globalMemory, arguments);
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

                Node node;

                if(tokens.CurrentToken.Type == TokenType.Identifier)
                {
                    // Vararg procedure call
                    var name = tokens.Eat(TokenType.Identifier);
                    var procedure = ResolveProcedure(name);
                    var argumentExpressions = new List<Node>();
                    while(tokens.CurrentToken.Type != TokenType.ParenthesisRight)
                        argumentExpressions.Add(ParseExpression());

                    node = new ProcedureCallNode { Name = name, ArgumentExpressions = argumentExpressions.ToArray() };
                }
                else
                {
                    node = ParseExpression();
                }

                tokens.Eat(TokenType.ParenthesisRight);

                return node;
            }
            else if (tokens.CurrentToken.Type == TokenType.BracketLeft)
            {
                // List
                tokens.Eat(TokenType.BracketLeft);

                var values = new List<string>();
                while(tokens.CurrentToken.Type != TokenType.BracketRight)
                    values.Add(tokens.Eat().LiteralValue);

                tokens.Eat(TokenType.BracketRight);

                return new ListNode { Values = values };
            }
            else if (tokens.CurrentToken.Type == TokenType.Integer)
            {
                var token = tokens.Eat(TokenType.Integer);
                return new IntegerNode { Value = Convert.ToInt32(token.Value) };
            }
            else if (tokens.CurrentToken.Type == TokenType.Colon)
            {
                tokens.Eat(TokenType.Colon);
                var variableName = tokens.Eat(TokenType.Identifier);
                
                return new VariableNode { Name = variableName };
            }
            else if(tokens.CurrentToken.Type == TokenType.StringLiteral)
            {
                var value = tokens.Eat(TokenType.StringLiteral);
                return new StringLiteralNode() { Value = value };
            }
            else if (tokens.CurrentToken.Type == TokenType.Identifier)
            {
                var name = tokens.Eat(TokenType.Identifier);
                var procedure = ResolveProcedure(name);
                var argumentExpressions = new List<Node>();
                for(int i=0; i<procedure.Arguments.Length; i++)
                    argumentExpressions.Add(ParseExpression());

                return new ProcedureCallNode { Name = name, ArgumentExpressions = argumentExpressions.ToArray() };
            }

            throw new ScriptException($"Onverwachte invoer: {tokens.CurrentToken}", tokens.CurrentToken);
        }

        private ProcedureInfo ResolveProcedure(Token token)
        {
            var name = token.Value.ToLower();

            if (!procedures.ContainsKey(name))
                throw new ScriptException($"Onbekende procedure '{name}'", token);

            return procedures[name];
        }
    }
}
