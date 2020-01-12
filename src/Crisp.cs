using Crisp.Ast;
using Crisp.Parsing;
using Crisp.Runtime;
using System;
using System.Collections.Generic;
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

        static void Load(string filename, Interpreter interpreter)
        {
            var code = File.ReadAllText(filename);
            var program = Parser.Parse(code);
            foreach (var expr in program.Expressions)
                interpreter.Evaluate(expr);
        }

        static Interpreter InitInterpreter()
        {
            var system = new Runtime.System();
            var globals = system.CreateGlobalEnvironment();
            var interpreter = new Interpreter(system, globals);

            Load("core.crisp", interpreter);

            return interpreter;
        }

        public static void Repl(TextReader reader, TextWriter writer)
        {
            string? Prompt()
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

            bool RunCommand((string, string) command, Interpreter interpreter)
            {
                switch (command)
                {
                    case ("eval", string code):
                        try
                        {
                            var program = Parser.Parse(code);
                            var result = interpreter.System.Null;
                            foreach (var expr in program.Expressions)
                                writer.WriteLine(interpreter.Evaluate(expr));
                        }
                        catch (CrispException e)
                        {
                            writer.WriteLine(e.FormattedMessage());
                        }
                        break;

                    case ("unknown", _):
                        writer.WriteLine("error: unknown command");
                        break;

                    case ("quit", _):
                        return true;
                }

                return false;
            }

            var interpreter = InitInterpreter();
            var done = false;
            while (!done)
            {
                var input = Prompt() ?? "";
                var command = ParseInput(input);
                done = RunCommand(command, interpreter);
            }
            writer.WriteLine("goodbye");
        }

        static void RunFile(string filename)
        {
            try
            {
                var interpreter = InitInterpreter();
                Load(filename, interpreter);
            }
            catch (CrispException e)
            {
                Console.WriteLine(e.FormattedMessage());
            }
        }
    }
}
