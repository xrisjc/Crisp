using Crisp.Runtime;
using Crisp.Parsing;
using System;
using System.IO;

namespace Crisp
{
    class Repl
    {
        enum Commands
        {
            EvalCode,
            Unknown,
            Quit,
            WriteEnvironment,
            WriteSymbolTable,
        }

        public static void Run(TextReader reader, TextWriter writer)
        {
            var environment = new Runtime.Environment();
            var parser = new Parser();

            while (true)
            {
                var input = Prompt(reader, writer);
                var command = ParseInput(input);
                switch (command)
                {
                    case Commands.EvalCode:
                        Evaluate(input, parser, environment, writer);
                        break;

                    case Commands.Unknown:
                        writer.WriteLine("error: unknown command");
                        break;

                    case Commands.Quit:
                        goto QuitRepl;

                    case Commands.WriteEnvironment:
                        environment.Write();
                        break;

                    case Commands.WriteSymbolTable:
                        parser.SymbolTable.Write();
                        break;
                }
            }

            QuitRepl: writer.WriteLine("goodbye");
        }

        static string Prompt(TextReader reader, TextWriter writer)
        {
            writer.Write("> ");
            return reader.ReadLine();
        }

        static Commands ParseInput(string input)
        {
            if (!input.StartsWith(":"))
            {
                return Commands.EvalCode;
            }

            var args = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            switch (args[0])
            {
                case ":q":
                    return Commands.Quit;
                case ":e":
                    return Commands.WriteEnvironment;
                case ":st":
                    return Commands.WriteSymbolTable;
                default:
                    return Commands.Unknown;
            }
        }

        static void Evaluate(string code, Parser parser, Runtime.Environment environment, TextWriter writer)
        {
            try
            {
                var program = parser.Parse(code);
                var result = Evaluator.Run(program, environment);
                writer.WriteLine(result);
            }
            catch (CrispException e)
            {
                writer.WriteLine(e.FormattedMessage());
            }
        }
    }
}
