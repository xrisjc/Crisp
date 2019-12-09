using Crisp.Runtime;
using Crisp.Parsing;
using System;
using System.IO;
using System.Collections.Generic;

namespace Crisp
{
    class Repl
    {
        enum Commands
        {
            EvalCode,
            Unknown,
            Quit,
        }

        static Dictionary<string, Commands> commands =
            new Dictionary<string, Commands>
            {
                [":q"] = Commands.Quit,            };

        public static void Run(TextReader reader, TextWriter writer)
        {
            var globals = new Runtime.ObjectObject();

            while (true)
            {
                var input = Prompt(reader, writer) ?? "";
                var command = ParseInput(input);
                switch (command)
                {
                    case Commands.EvalCode:
                        Evaluate(input, globals, writer);
                        break;

                    case Commands.Unknown:
                        writer.WriteLine("error: unknown command");
                        break;

                    case Commands.Quit:
                        goto QuitRepl;
                }
            }

            QuitRepl: writer.WriteLine("goodbye");
        }

        static string? Prompt(TextReader reader, TextWriter writer)
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

            if (!commands.TryGetValue(args[0], out var command))
                command = Commands.Unknown;
            
            return command;
        }

        static void Evaluate(string code, Runtime.ObjectObject globals, TextWriter writer)
        {
            try
            {
                var program = Parser.Parse(code);
                var result = new ObjectNull();
                var interpreter = new Interpreter(globals);
                foreach (var expr in program.Expressions)
                    writer.WriteLine(interpreter.Evaluate(expr));
            }
            catch (CrispException e)
            {
                writer.WriteLine(e.FormattedMessage());
            }
        }
    }
}
