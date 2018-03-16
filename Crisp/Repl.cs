using Crisp.Eval;
using Crisp.Fn;
using Crisp.Parsing;
using System;
using System.IO;

namespace Crisp
{
    class Repl
    {
        public static void Run(TextReader reader, TextWriter writer)
        {
            var environment = new Eval.Environment();
            environment.Create("len", new Len());
            environment.Create("push", new Push());
            environment.Create("writeLn", new WriteLn());
            environment.Create("readLn", new ReadLn());

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
            Eval.Environment environment,
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
            Eval.Environment environment, TextWriter writer)
        {
            var scanner = new Scanner(code);
            var parser = new Parser(scanner);
            var expr = parser.ParseExpression();
            var obj = expr.Evaluate(environment);
            writer.WriteLine(obj);
        }

        public static void Load(string filename, Eval.Environment environment)
        {
            try
            {
                var sys = File.ReadAllText(filename);
                var scanner = new Scanner(sys);
                var parser = new Parser(scanner);
                while (!parser.IsFinished)
                {
                    var expr = parser.ParseExpression();
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
