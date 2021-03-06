﻿using System;
using System.Collections.Generic;

namespace YetAnotherScriptingLanguage
{
    public class Interpreter
    {
        public class Block
        {
            public Block(string name = null)
            {
                Name = name;
                variables = new Dictionary<string, Function>();
            }
            public Dictionary<string, Function> variables;
            public Dictionary<string, Function> Variables => variables;
            public string Name { get; set; }
        }
        public enum mode
        {
            console,
            Uwp
        }
        public static mode Mode = mode.Uwp;
        private string _script;
        public static Dictionary<string, Function> Functions = new Dictionary<string, Function>();
        public static Dictionary<string, Action> Actions = new Dictionary<string, Action>();
        public static Queue<variables.Variable> ReturnValue = new Queue<variables.Variable>();

        private static KeyWords keywords;
        public static KeyWords Keywords {
            get {
                if (keywords is null) return keywords =new KeyWords();
                return keywords;
            }
        }

        public TokensList tokens { get; set; }
        public int index = 0;

        private static Stack<Block> levels = new Stack<Block>();
        public static Stack<Block> ExecutionStack => levels;
        public static Block CurrentBlock => levels.Peek();

        public static COMMANDS.GET  Get  = new COMMANDS.GET() ;
        public static COMMANDS.SET  Set  = new COMMANDS.SET() ;
        public static COMMANDS.POP  Pop  = new COMMANDS.POP() ; 
        public static COMMANDS.PEEK Peek = new COMMANDS.PEEK(); 
        public static COMMANDS.POST Post = new COMMANDS.POST(); 

        public Interpreter()
        {
            Initialize();
        }
        public void Initialize()
        {
            setUpExternals();
            SetUp();
        }
        
        void setUpExternals()
        {
            MathProcess.SetupFunctions();
            ConstantsMap.SetUpCanstants();
        }

        public void SetUp()
        {
            foreach (var word in Interpreter.Keywords)
            {
                switch (word.Value)
                {
                    case ("IF"): 
                        Interpreter.Functions.Add("IF", new IfProcess());
                        break;
                    case ("WHILE"):
                        Interpreter.Functions.Add("WHILE", new WhileProcess());
                        break;
                    case ("PRINT"):
                        Interpreter.Functions.Add("PRINT", new PrintProcess());
                        break;
                    case ("READ"):
                        Interpreter.Functions.Add("READ", new ReadProcess());
                        break;
                    case ("RETURN"):
                        Interpreter.Functions.Add("RETURN", new ReturnProcess());
                        break;
                    case ("VARIABLE"):
                        Interpreter.Functions.Add("VARIABLE", new VariableProcess());
                        break;
                    case ("FOR"):
                        Interpreter.Functions.Add("FOR", new ForProcess());
                        break;
                    case ("IMPORT"):
                        Interpreter.Functions.Add("IMPORT", new ImportProcess());
                        break;
                    case ("TYPE"):
                        Interpreter.Functions.Add("TYPE", new ClassProcess());
                        break;
                    case ("FUNCTION"):
                        Interpreter.Functions.Add("FUNCTION", new FunctionProcess());
                        break;
                    case ("SET"):
                        Interpreter.Functions.Add("SET", new SetProcess());
                        break;
                    case ("OPEN"):
                        Interpreter.Functions.Add("OPEN", new OpenProcess());
                        break;
                    case ("ARRAY"):
                        Interpreter.Functions.Add("ARRAY", new ArrayProcess());
                        break;
                    case ("DELETE"):
                        Interpreter.Functions.Add("DELETE", new DeleteProcess());
                        break;
                    case ("EVALUATE"):
                        Interpreter.Functions.Add("EVALUATE", new EvalProcess());
                        break;
                    case ("MEMBER"):
                        Interpreter.Functions.Add("MEMBER", new MemberProcess());
                        break;
                    case ("METHOD"):
                        Interpreter.Functions.Add("METHOD", new MethodProcess());
                        break;
                    case ("CREATE"):
                        Interpreter.Functions.Add("CREATE", new CreateProcess());
                        break;
                    case ("HOLDER"):
                        Interpreter.Functions.Add("HOLDER", new ThisHolderProcess());
                        break;
                    case ("LOOP"):
                        Interpreter.Functions.Add("LOOP", new RepeatProcess());
                        break;
                }
            }
            foreach (var maps in MathProcess.DualFunctions.Keys)
            {
                Interpreter.Functions.Add(maps, new MathProcess(maps));
            }
            foreach (var maps in MathProcess.UnaryFunctions.Keys)
            {
                Interpreter.Functions.Add(maps, new MathProcess(maps));
            }
            foreach (var csts in ConstantsMap.Canstants.Keys)
            {
                Interpreter.Functions.Add(csts, new ConstantsMap(csts));
            }
            Interpreter.Keywords.UpdateColorMap();
        }

        public void Reset()
        {
            keywords.Update();
            Functions.Clear();
            variables.Variable.CustomTypes.Clear();
            Interpreter.ExecutionStack.Clear();
            Parser.ParserState = Parser.state.Normal;
            SetUp();
        }

        public static void logStacks()
        {
            foreach(var stack in Interpreter.ExecutionStack)
            {
                foreach(var variable in stack.Variables)
                    Console.WriteLine(variable.Key + " : " + ((variables.Variable)variable.Value).Type.ToString());
            }
        } 
    }
    namespace COMMANDS
    {
        public class GET
        {
            internal GET() { }
            public Function this[string token]
            {
                get
                {
                    if (Interpreter.ExecutionStack.Count > 0)
                    {
                        if (Interpreter.CurrentBlock.Variables.ContainsKey(token))
                            return Interpreter.CurrentBlock.Variables[token];
                    }
                    if (Interpreter.Functions.ContainsKey(token)) return Interpreter.Functions[token];
                    throw new Exception(token + " Method undefined");
                }
            }

            public Tuple<Function,int> this[TokensList expression,int i]
            {
                get
                {
                    variables.Variable v = null;
                    if (expression[i].IsKeyword == "HOLDER")
                    {
                        v = ThisHolderProcess.Referenced;
                    }
                    else
                    {
                        v = (variables.Variable)Interpreter.Get[expression[i].Word];
                    }
                    if (v.Type == variables.Variable.type.Array && (i + 1 < expression.Count && expression[i + 1].Type == Token.type.array))
                    {
                        v = variables.Array.getElement(v as variables.Array, expression[i + 1]);
                        i++;
                    }
                    while (v.Type == variables.Variable.type.Record && (i + 1 < expression.Count && expression[i + 1].IsKeyword == "ACCESS"))
                    {
                        i += 2;
                        var member = (v as variables.Record)[expression[i].Word];
                        if (member.Type == Function.type.variable)
                        {
                            v = (variables.Variable)member;
                            if (v.Type == variables.Variable.type.Array && (i + 1 < expression.Count && expression[i + 1].Type == Token.type.array))
                            {
                                v = variables.Array.getElement(member as variables.Array, expression[i + 1]);
                                i++;
                            }
                            continue;
                        }
                        else if (member.Type == Function.type.function || member.Type == Function.type.procedure)
                        {
                            Function foo = member as MethodProcess.CustomFunction;
                            var Body = expression[i, foo.Limiter];
                            i += Body.Count;
                            if(i<expression.Count)
                                i -= expression[i].Type == Token.type.function ? 2 : 0;
                            ThisHolderProcess.Referenced = v as variables.Record;
                            v = foo[Body];
                            ThisHolderProcess.Referenced = null;
                            if(foo.Type == Function.type.procedure)
                                break;
                        }
                        else
                        {
                            throw new Exception("Inexpected Token : " + expression[i].Word);
                        }
                        i++;
                    }
                    return new Tuple<Function, int>(v,i);
                }
            }
        }

        public class SET
        {
            internal SET() { }
            public Function  this[string token]
            {
                set
                {
                    value.Name = token;
                    if (Interpreter.ExecutionStack.Count == 0)
                    {
                        Interpreter.Set.Insert(new Interpreter.Block());
                    }
                    Interpreter.CurrentBlock.Variables.Add(token, value);
                }
            }

            public void Insert(Interpreter.Block block)
            {
                Interpreter.ExecutionStack.Push(block);
            }
        }

        public class POP
        {
            internal POP() { }
            public bool this[string token]
            {
                get
                {
                    bool found = Interpreter.CurrentBlock.Variables.ContainsKey(token);
                    if (found)
                    {
                        Interpreter.CurrentBlock.Variables.Remove(token);
                    }
                    return found;
                }
            }
        }

        public class PEEK
        {
            internal PEEK() { }
            public bool this[string token]
            {
                get
                {
                    return (Interpreter.ExecutionStack.Count>0 && Interpreter.CurrentBlock.Variables.ContainsKey(token)) || Interpreter.Functions.ContainsKey(token);
                }
            }
        }

        public class POST
        {
            internal POST() { }
            public Function this[string token]
            {
                set
                {
                    Interpreter.Functions.Add(token, value);
                }
            }
        }
    }
}
