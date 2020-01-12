using Crisp.Parsing;
using Crisp.Runtime;
using System;
using System.IO;

namespace Crisp
{
    class Crisp
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Repl(Console.In, Console.Out);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                Console.WriteLine("usage: crisp [filename]");
            }
        }

        public static void Repl(TextReader reader, TextWriter writer)
        {
            static string? Prompt(TextReader reader, TextWriter writer)
            {
                writer.Write("> ");
                return reader.ReadLine();
            }

            static (string, string) ParseInput(string input)
            {
                if (!input.StartsWith(":"))
                    return ("eval", input);

                var args = input.Substring(1).Split(
                    new[] { ' ' },
                    StringSplitOptions.RemoveEmptyEntries);

                if (args.Length == 0)
                    return ("unknown", "");

                return args[0] switch
                {
                    "q" => ("quit", ""),
                    _ => ("unknown", ""),
                };
            }

            static void Evaluate(
                string code,
                Runtime.System system,
                Runtime.Environment globals,
                TextWriter writer)
            {
                try
                {
                    var program = Parser.Parse(code);
                    var result = system.Null;
                    var interpreter = new Interpreter(system, globals);
                    foreach (var expr in program.Expressions)
                        writer.WriteLine(interpreter.Evaluate(expr));
                }
                catch (CrispException e)
                {
                    writer.WriteLine(e.FormattedMessage());
                }
            }

            var system = new Runtime.System();
            var globals = system.CreateGlobalEnvironment();

            while (true)
            {
                var input = Prompt(reader, writer) ?? "";
                var command = ParseInput(input);
                switch (command)
                {
                    case ("eval", string code):
                        Evaluate(code, system, globals, writer);
                        break;

                    case ("unknown", _):
                        writer.WriteLine("error: unknown command");
                        break;

                    case ("quit", _):
                        goto QuitRepl;
                }
            }

            QuitRepl: writer.WriteLine("goodbye");
        }

        static void RunFile(string filename)
        {
            try
            {
                var code = File.ReadAllText(filename);
                var program = Parser.Parse(code);
                var system = new Runtime.System();
                var globals = system.CreateGlobalEnvironment();
                var interpreter = new Interpreter(system, globals);
                foreach (var expr in program.Expressions)
                    interpreter.Evaluate(expr);
            }
            catch (CrispException e)
            {
                Console.WriteLine(e.FormattedMessage());
            }
        }
    }
}
