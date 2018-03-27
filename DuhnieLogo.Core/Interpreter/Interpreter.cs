using DuhnieLogo.Core.Interpreter.Ast;
using DuhnieLogo.Core.Tokens;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        private volatile bool stopRequested = false;

        public Interpreter()
        {
            RegisterFunction("herhaal",  new string[] { "count", "commands" }, (_context, _arguments) => {
                if (!(_arguments[1] is ListVariable))
                    throw new ScriptException("Herhaal verwacht een lijst van opdrachten", _context.CallToken);
                //_context.GetArgument("count");
                var memorySpace = new MemorySpace(memory);
                PushMemorySpace(memorySpace);

                var tokens = Lexer.Tokenize(string.Join(" ", (ListVariable)_arguments[1])).ToArray();

                try
                {
                    for (int i = 0; i < (int)_arguments[0]; i++)
                    {
                        memorySpace.Set("iteratie", i + 1);
                        Interpret(tokens);
                    }
                }catch(Exception ex)
                {
                    throw new ScriptException($"Fout in herhaal", _context.CallToken, ex);
                }

                PopMemorySpace();

                return null;
            });

            RegisterFunction("zolang", new string[] { "uitdrukking", "opdrachten" }, (_context, _arguments) => {
                var expressionTokens = Lexer.Tokenize(string.Join(" ", (ListVariable)_arguments[0])).ToArray();
                var bodyTokens = Lexer.Tokenize(string.Join(" ", (ListVariable)_arguments[1])).ToArray();

                while(Convert.ToBoolean(InterpretExpression(expressionTokens)))
                {
                    Interpret(bodyTokens);
                }

                return null;
            });

            RegisterFunction("telherhaal", new string[] {}, (_context, _arguments) => {
                if (!memory.ContainsRecursive("iteratie"))
                    throw new ScriptException("Telherhaal kan alleen binnen een herhaal opdracht gebruikt worden");

                return memory.Get("iteratie");
            });

            RegisterFunction("en", new string[] {"aanduiding1", "aanduiding2" }, (_context, _arguments) => {
                return _arguments.All(arg => Convert.ToBoolean(arg));
            });

            RegisterFunction("of", new string[] { "aanduiding1", "aanduiding2" }, (_context, _arguments) => {
                return _arguments.Any(arg => Convert.ToBoolean(arg));
            });

            RegisterFunction("niet", new string[] { "aanduiding" }, (_context, _arguments) => {
                return !Convert.ToBoolean(_arguments[0]);
            });

            RegisterFunction("als", new string[] { "conditie", "alsWelwaar", "alsNietwaar" }, (_context, _arguments) => {
                var boolean = Convert.ToBoolean(_arguments[0]);

                if (_arguments[1] is ListVariable && _arguments[2] is ListVariable)
                {
                    var trueTokens = _arguments[1] as ListVariable;
                    var falseTokens = _arguments[2] as ListVariable;

                    var memorySpace = new MemorySpace(memory);
                    PushMemorySpace(memorySpace);

                    var tokens = Lexer.Tokenize(string.Join(" ", boolean ? trueTokens : falseTokens)).ToArray();
                    Interpret(tokens);

                    PopMemorySpace();
                    return null;
                }
                else
                {
                    return boolean ? _arguments[1] : _arguments[2];
                }
            });

            RegisterFunction("test", new string[] { "conditie" }, (_context, _arguments) => {
                var boolean = Convert.ToBoolean(_arguments[0]);

                memory.Set("testResultaat", boolean);
                return null;
            });

            RegisterFunction("alswaar", new string[] { "instructies" }, (_context, _arguments) => {
                if (!memory.ContainsRecursive("testResultaat"))
                    throw new ScriptException("Alswaar kan alleen na een aanroep van test gebruikt worden");

                if (Convert.ToBoolean(memory.Get("testResultaat")))
                {
                    var memorySpace = new MemorySpace(memory);
                    PushMemorySpace(memorySpace);

                    var tokens = Lexer.Tokenize(string.Join(" ", _arguments[0] as ListVariable)).ToArray();
                    Interpret(tokens);

                    PopMemorySpace();
                }

                return null;
            });

            RegisterFunction("alsnietwaar", new string[] { "instructies" }, (_context, _arguments) => {
                if (!memory.ContainsRecursive("testResultaat"))
                    throw new ScriptException("Alsnietwaar kan alleen na een aanroep van test gebruikt worden");

                if (!Convert.ToBoolean(memory.Get("testResultaat")))
                {
                    var memorySpace = new MemorySpace(memory);
                    PushMemorySpace(memorySpace);

                    var tokens = Lexer.Tokenize(string.Join(" ", _arguments[0] as ListVariable)).ToArray();
                    Interpret(tokens);

                    PopMemorySpace();
                }

                return null;
            });

            RegisterFunction("lijst", new string[] { "arg1", "arg2" }, (_context, _arguments) => {
                var result = new ListVariable();

                foreach (var arg in _arguments)
                    result.Add(arg.ToString());

                return result;
            });

            RegisterFunction("openafbeelding", new string[] { "naam" }, (_context, _arguments) => {
                var fileName = _arguments[0].ToString();

                return Image.FromFile(fileName);
            });

            RegisterFunction("aantal", new string[] { "aanduiding" }, (_context, _arguments) => {
                var target = _arguments[0];

                if (target is ListVariable)
                    return (target as ListVariable).Count;
                else
                    throw new ScriptException($"Aantal verwacht een lijst", _context.CallToken);
            });

            RegisterFunction("plaatservoor", new string[] { "aanduiding", "lijst" }, (_context, _arguments) => {
                var target = _arguments[1];
                var addition = _arguments[0];

                if (target is ListVariable)
                {
                    var newList = new ListVariable((target as ListVariable));

                    newList.Insert(0, addition.ToString());
                    return newList;
                }
                else
                    throw new ScriptException($"Plaatservoor verwacht een aanduiding en een lijst", _context.CallToken);
            });

            RegisterFunction("plaatserachter", new string[] { "aanduiding", "lijst" }, (_context, _arguments) => {
                var target = _arguments[1];
                var addition = _arguments[0];

                if(target is ListVariable)
                {
                    var newList = new ListVariable((target as ListVariable));

                    newList.Add(addition.ToString());
                    return newList;
                }
                else
                    throw new ScriptException($"Plaatserachter verwacht een aanduiding en een lijst", _context.CallToken);
            });

            RegisterFunction("eerste", new string[] { "aanduiding" }, (_context, _arguments) => {
                var target = _arguments[0];

                if (target is ListVariable)
                {
                    var list = target as ListVariable;
                    if (list.Count == 0)
                        throw new ScriptException($"Ik kan niet het eerste element van een lege lijst opvragen", _context.CallToken);
                    
                    return list[0];
                }
                else
                    throw new ScriptException($"Eerste verwacht een lijst", _context.CallToken);
            });

            RegisterFunction("laatste", new string[] { "aanduiding" }, (_context, _arguments) => {
                var target = _arguments[0];

                if (target is ListVariable)
                {
                    var list = target as ListVariable;
                    if (list.Count == 0)
                        throw new ScriptException($"Ik kan niet het laatste element van een lege lijst opvragen", _context.CallToken);

                    return list[list.Count - 1];
                }
                else
                    throw new ScriptException($"Laatste verwacht een lijst", _context.CallToken);
            });

            RegisterFunction("mineerste", new string[] { "lijst" }, (_context, _arguments) => {
                var list = _arguments[0];

                if (list is ListVariable)
                {
                    var newList = new ListVariable((list as ListVariable));

                    newList.RemoveAt(0);
                    return newList;
                }
                else
                    throw new ScriptException($"Mineerste verwacht een lijst", _context.CallToken);
            });

            RegisterFunction("minlaatste", new string[] { "lijst" }, (_context, _arguments) => {
                var list = _arguments[0];

                if (list is ListVariable)
                {
                    var newList = new ListVariable((list as ListVariable));

                    newList.RemoveAt(newList.Count - 1);
                    return newList;
                }
                else
                    throw new ScriptException($"Minlaatste verwacht een lijst", _context.CallToken);
            });

            RegisterFunction("element", new string[] { "welke", "aanduiding" }, (_context, _arguments) => {
                var index = _arguments[0];
                var source = _arguments[1];

                if (source is ListVariable)
                {
                    return ((ListVariable)source) [Convert.ToInt32(index)];
                }

                return null;
            });

            RegisterFunction("woord", new string[] { "arg1", "arg2" }, (_context, _arguments) => {
                var result = new StringBuilder();

                foreach (var arg in _arguments)
                    result.Append(arg.ToString());

                return result.ToString();
            });

            RegisterFunction("gok", new string[] { "maximum" }, (_context, _arguments) => {
                var max = (int)_arguments[0];
                return random.Next(max - 1);
            });

            RegisterFunction("wacht", new string[] { "duration" }, (_context, _arguments) => {
                Thread.Sleep((int)_arguments[0]);
                return null;
            });

            globalMemory = new MemorySpace();
            memory = globalMemory;
        }

        public void RegisterFunction(string name, string[] arguments, Func<ProcedureCallContext, object[], object> implementation)
        {
            procedures.Add(name.ToLower(), new BuiltInProcedure { Name = name, Arguments = arguments, Implementation = implementation });
        }

        public void RegisterFunction(string[] names, string[] arguments, Func<ProcedureCallContext, object[], object> implementation)
        {
            foreach(var name in names)
                RegisterFunction(name, arguments, implementation);
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
            memory = memoryStack.Pop();
        }

        public void Stop()
        {
            stopRequested = true;
        }

        public object Interpret(Token[] tokens)
        {
            stopRequested = false;

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

        public object InterpretExpression(Token[] tokens)
        {
            stopRequested = false;

            PushTokenStream(new TokenStream(tokens));
            object result = Expression();

            PopTokenStream();
            return result;
        }

        private object StatementList()
        {
            while (tokens.CurrentToken.Type != TokenType.ProgramEnd)
            {
                Statement();

                if (stopRequested)
                    throw new ExcecutionStoppedException();
            }

            return null;
        }

        private object Statement()
        {
            if (tokens.CurrentToken.Type == TokenType.Make)
                return VariableDefinition();
            else if (tokens.CurrentToken.Type == TokenType.Local)
                return LocalVariableDefinition();
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

            var nameToken = tokens.Eat(TokenType.Identifier);
            procedureInfo.Name = nameToken.Value;

            var arguments = new ListVariable();
            while(tokens.CurrentToken.Type == TokenType.Colon)
            {
                tokens.Eat(TokenType.Colon);
                arguments.Add(tokens.Eat(TokenType.Identifier).Value);
            }
            procedureInfo.Arguments = arguments.ToArray();

            var procTokens = new List<Token>();

            while (tokens.CurrentToken.Type != TokenType.End)
                procTokens.Add(tokens.Eat());

            procTokens.Add(new Token(TokenType.ProgramEnd, "", new TokenPosition(0, 0, 0)));

            tokens.Eat(TokenType.End);
            procedureInfo.Tokens = procTokens.ToArray();

            if (ProcedureExists(procedureInfo.Name))
                throw new ScriptException($"Er bestaat al een procedure '{procedureInfo.Name}'", nameToken);

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

        private object LocalVariableDefinition()
        {
            var token = tokens.Eat(TokenType.Local);
            var name = tokens.Eat(TokenType.StringLiteral);

            if (memory == globalMemory)
                throw new ScriptException("Lokale variabelen kunnen alleen binnnen procedures gemaakt worden", token);

            memory.Set(name.Value, null);

            return null;
        }

        private object VariableDefinition()
        {
            tokens.Eat(TokenType.Make);
            var name = tokens.Eat(TokenType.StringLiteral);
            var value = Expression();

            // If exists in local scope, update value, else create in global scope
            if(memory.ContainsLocal(name.Value))
                memory.Set(name.Value, value);
            else
                globalMemory.Set(name.Value, value);

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
            if (node is BooleanNode)
                return ((BooleanNode)node).Value;
            if (node is StringLiteralNode)
                return ((StringLiteralNode)node).Value.Value;
            if (node is VariableNode)
            {
                var variableNode = node as VariableNode;
                if (memory.ContainsRecursive(variableNode.Name.Value))
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
                var context = new ProcedureCallContext() {
                    GlobalMemory = globalMemory,
                    CallToken = node.Name
                };
                return builtInProcedure.Implementation(context, arguments);
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
                    return Convert.ToInt32(left) + Convert.ToInt32(right);
                case TokenType.Minus:
                    return Convert.ToInt32(left) - Convert.ToInt32(right);
                case TokenType.Multiply:
                    return Convert.ToInt32(left) * Convert.ToInt32(right);
                case TokenType.Divide:
                    return Convert.ToInt32(left) / Convert.ToInt32(right);

                case TokenType.Equals:
                    return (int)left == (int)right;
                case TokenType.NotEqual:
                    return (int)left != (int)right;
                case TokenType.GreaterThan:
                    return Convert.ToInt32(left) > Convert.ToInt32(right);
                case TokenType.GreaterOrEqualThan:
                    return Convert.ToInt32(left) >= Convert.ToInt32(right);
                case TokenType.SmallerThan:
                    return Convert.ToInt32(left) < Convert.ToInt32(right);
                case TokenType.SmallerOrEqualThan:
                    return Convert.ToInt32(left) <= Convert.ToInt32(right);
            }

            throw new Exception($"Unknown operator {binaryOperatorNode.Operator}");
        }

        private Node ParseExpression()
        {
            return ParseComparitiveExpression();
        }

        private Node ParseComparitiveExpression()
        {
            var node = ParseAdditiveExpression();

            while (tokens.CurrentToken.Type == TokenType.Equals ||
                tokens.CurrentToken.Type == TokenType.NotEqual ||
                tokens.CurrentToken.Type == TokenType.SmallerThan || 
                tokens.CurrentToken.Type == TokenType.SmallerOrEqualThan ||
                tokens.CurrentToken.Type == TokenType.GreaterThan ||
                tokens.CurrentToken.Type == TokenType.GreaterOrEqualThan)
            {
                var op = tokens.Eat(tokens.CurrentToken.Type);

                node = new BinaryOperatorNode(node.Position) { Left = node, Right = ParseAdditiveExpression(), Operator = op };
            }

            return node;
        }

        private Node ParseAdditiveExpression()
        {
            var node = ParseMultiplicativeExpression();

            while(tokens.CurrentToken.Type == TokenType.Plus || tokens.CurrentToken.Type == TokenType.Minus)
            {
                var op = tokens.Eat(tokens.CurrentToken.Type);

                node = new BinaryOperatorNode(node.Position) { Left = node, Right = ParseMultiplicativeExpression(), Operator = op };
            }

            return node;
        }

        private Node ParseMultiplicativeExpression()
        {
            var node = ParseFactor();

            while (tokens.CurrentToken.Type == TokenType.Multiply || tokens.CurrentToken.Type == TokenType.Divide)
            {
                var op = tokens.Eat(tokens.CurrentToken.Type);

                node = new BinaryOperatorNode(node.Position) { Left = node, Right = ParseFactor(), Operator = op };
            }

            return node;
        }

        private Node ParseFactor()
        {
            var position = tokens.CurrentToken.Location;

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

                    node = new ProcedureCallNode(position) { Name = name, ArgumentExpressions = argumentExpressions.ToArray() };
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

                var values = new ListVariable();
                while(tokens.CurrentToken.Type != TokenType.BracketRight)
                    values.Add(tokens.Eat().LiteralValue);

                tokens.Eat(TokenType.BracketRight);

                return new ListNode(position) { Values = values };
            }
            else if (tokens.CurrentToken.Type == TokenType.Integer)
            {
                var token = tokens.Eat(TokenType.Integer);
                return new IntegerNode(position) { Value = Convert.ToInt32(token.Value) };
            }
            else if (tokens.CurrentToken.Type == TokenType.True || tokens.CurrentToken.Type == TokenType.False)
            {
                var token = tokens.Eat();
                return new BooleanNode(position) { Value = token.Value.Equals("welwaar", StringComparison.CurrentCultureIgnoreCase) };
            }
            else if (tokens.CurrentToken.Type == TokenType.Colon)
            {
                tokens.Eat(TokenType.Colon);
                var variableName = tokens.Eat(TokenType.Identifier);
                
                return new VariableNode(position) { Name = variableName };
            }
            else if(tokens.CurrentToken.Type == TokenType.StringLiteral)
            {
                var value = tokens.Eat(TokenType.StringLiteral);
                return new StringLiteralNode(position) { Value = value };
            }
            else if (tokens.CurrentToken.Type == TokenType.Identifier)
            {
                var name = tokens.Eat(TokenType.Identifier);
                var procedure = ResolveProcedure(name);
                var argumentExpressions = new List<Node>();

                for (int i = 0; i < procedure.Arguments.Length; i++)
                {
                    argumentExpressions.Add(ParseExpression());
                }

                return new ProcedureCallNode(position) { Name = name, ArgumentExpressions = argumentExpressions.ToArray() };
            }

            throw new ScriptException($"Onverwachte invoer: {tokens.CurrentToken}", tokens.CurrentToken);
        }

        private bool ProcedureExists(string name)
        {
            return procedures.ContainsKey(name.ToLower());
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
