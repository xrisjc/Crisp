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
            var parser = new Parser();

            var quit = false;
            while (!quit)
            {
                try
                {
                    writer.Write("> ");
                    var code = reader.ReadLine();
                    if (code.Length > 0 && code[0] == ':')
                    {
                        quit = ExecuteCommand(code, parser, environment, writer);
                    }
                    else
                    {
                        EvalAndPrint(code, parser, environment, writer);
                    }
                }
                catch (CrispException e)
                {
                    writer.WriteLine(e.FormattedMessage());
                }
            }
        }

        private static bool ExecuteCommand(string code, Parser parser, Runtime.Environment environment, TextWriter writer)
        {
            var args = code.Split(new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);
            switch (args[0])
            {
                case ":q":
                    return true;
                case ":e":
                    environment.Write();
                    break;
                case ":st":
                    parser.SymbolTable.Write();
                    break;
                default:
                    writer.WriteLine($"Unknown command <{code}>");
                    break;
            }

            return false;
        }

        private static void EvalAndPrint(string code, Parser parser,
            Runtime.Environment environment, TextWriter writer)
        {
            var program = parser.Parse(code);
            var result = Evaluator.Run(program, environment);
            writer.WriteLine(result);
        }
    }
}
