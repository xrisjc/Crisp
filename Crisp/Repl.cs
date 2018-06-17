using Crisp.Ast;
using Crisp.Eval;
using Crisp.Parsing;
using System;
using System.Collections.Generic;
using System.IO;

namespace Crisp
{
    class Repl
    {
        static void PrintStack(Stack<dynamic> stack)
        {
            Console.WriteLine("###Stack");
            if (stack.Count == 0)
            {
                Console.WriteLine("<empty stack>");
            }
            foreach (var x in stack)
            {
                Console.WriteLine(x);
            }
        }

        public static void Run(TextReader reader, TextWriter writer)
        {
            var stack = new Stack<dynamic>();
            var environment = new Eval.Environment();

            Load("Sys.crisp", stack, environment);
            Load("Test.crisp", stack, environment);

            var quit = false;
            while (!quit)
            {
                try
                {
                    writer.Write("> ");
                    var code = reader.ReadLine();
                    if (code.Length > 0 && code[0] == ':')
                    {
                        quit = ExecuteCommand(code, stack, environment, writer);
                    }
                    else
                    {
                        EvalAndPrint(code, stack, environment, writer);
                    }
                }
                catch (CrispException e)
                {
                    writer.WriteLine(e.FormattedMessage());
                }
            }
        }

        private static bool ExecuteCommand(
            string code,
            Stack<dynamic> stack,
            Eval.Environment environment,
            TextWriter writer)
        {
            var args = code.Split(new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);
            switch (args[0])
            {
                case ":l" when args.Length >= 2:
                    Load(args[1], stack, environment);
                    break;
                case ":q":
                    return true;
                default:
                    writer.WriteLine($"Unknown command <{code}>");
                    break;
            }

            return false;
        }

        private static void EvalAndPrint(string code, Stack<dynamic> stack,
            Eval.Environment environment, TextWriter writer)
        {
            var scanner = new Scanner(code);
            var parser = new Parser(scanner);
            var expressions = parser.Program();
            foreach (var expr in expressions)
            {
                var value = Evaluate(expr, stack, environment);
                writer.WriteLine(value);
                PrintStack(stack);
            }
        }

        public static void Load(string filename, Stack<dynamic> stack, Eval.Environment environment)
        {
            try
            {
                var sys = File.ReadAllText(filename);
                var scanner = new Scanner(sys);
                var parser = new Parser(scanner);
                var program = parser.Program();
                foreach (var expr in program)
                {
                    Evaluate(expr, stack, environment);
                }
            }
            catch (CrispException e)
            {
                Console.WriteLine(e.FormattedMessage());
            }
        }

        static dynamic Evaluate(IExpression expr, Stack<dynamic> stack, Eval.Environment environment)
        {
            expr.Evaluate(stack, environment);
            return stack.Pop();
        }
    }
}
