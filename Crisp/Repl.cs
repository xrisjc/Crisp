using Crisp.Runtime;
using Crisp.Parsing;
using System;
using System.IO;

namespace Crisp
{
    class Repl
    {
        public static void Run(TextReader reader, TextWriter writer)
        {
            var environment = new Runtime.Environment();
            var symbolTable = new SymbolTable(outer: null);

            Load("Sys.crisp", environment, symbolTable);
            Load("Test.crisp", environment, symbolTable);

            var quit = false;
            while (!quit)
            {
                try
                {
                    writer.Write("> ");
                    var code = reader.ReadLine();
                    if (code.Length > 0 && code[0] == ':')
                    {
                        quit = ExecuteCommand(code, environment, symbolTable, writer);
                    }
                    else
                    {
                        EvalAndPrint(code, environment, symbolTable, writer);
                    }
                }
                catch (CrispException e)
                {
                    writer.WriteLine(e.FormattedMessage());
                }
            }
        }

        private static bool ExecuteCommand(string code, Runtime.Environment environment, SymbolTable symbolTable, TextWriter writer)
        {
            var args = code.Split(new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);
            switch (args[0])
            {
                case ":l" when args.Length >= 2:
                    Load(args[1], environment, symbolTable);
                    break;
                case ":q":
                    return true;
                default:
                    writer.WriteLine($"Unknown command <{code}>");
                    break;
            }

            return false;
        }

        private static void EvalAndPrint(string code,
            Runtime.Environment environment, SymbolTable symbolTable, TextWriter writer)
        {
            var scanner = new Scanner(code);
            var parser = new Parser(scanner, symbolTable);
            var expressions = parser.Program();
            var evaluator = new Evaluator(environment);
            foreach (var expression in expressions)
            {
                evaluator.Evaluate(expression);
                var value = evaluator.Pop();
                writer.WriteLine(value);
            }
        }

        public static void Load(string filename, Runtime.Environment environment, SymbolTable symbolTable)
        {
            try
            {
                var sys = File.ReadAllText(filename);
                var scanner = new Scanner(sys);
                var parser = new Parser(scanner, symbolTable);
                var program = parser.Program();
                var evaluator = new Evaluator(environment);
                foreach (var expr in program)
                {
                    evaluator.Evaluate(expr);
                    evaluator.Pop();
                }
            }
            catch (CrispException e)
            {
                Console.WriteLine(e.FormattedMessage());
            }
        }
    }
}
