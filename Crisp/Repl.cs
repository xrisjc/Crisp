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

            Load("Sys.crisp", environment);
            Load("Test.crisp", environment);

            var quit = false;
            while (!quit)
            {
                try
                {
                    writer.Write("> ");
                    var code = reader.ReadLine();
                    if (code.Length > 0 && code[0] == ':')
                    {
                        quit = ExecuteCommand(code, environment, writer);
                    }
                    else
                    {
                        EvalAndPrint(code, environment, writer);
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
            Runtime.Environment environment,
            TextWriter writer)
        {
            var args = code.Split(new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);
            switch (args[0])
            {
                case ":l" when args.Length >= 2:
                    Load(filename: args[1], environment: environment);
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
            Runtime.Environment environment, TextWriter writer)
        {
            var scanner = new Scanner(code);
            var parser = new Parser(scanner);
            var expressions = parser.Program();
            foreach (var expression in expressions)
            {
                var value = expression.Evaluate(environment);
                writer.WriteLine(value);
            }
        }

        public static void Load(string filename, Runtime.Environment environment)
        {
            try
            {
                var sys = File.ReadAllText(filename);
                var scanner = new Scanner(sys);
                var parser = new Parser(scanner);
                var program = parser.Program();
                foreach (var expr in program)
                {
                    expr.Evaluate(environment);
                }
            }
            catch (CrispException e)
            {
                Console.WriteLine(e.FormattedMessage());
            }
        }
    }
}
