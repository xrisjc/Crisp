﻿using Crisp.Parsing;
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

        static void Load(string filename, IEnvironment environment)
        {
            var code = File.ReadAllText(filename);
            var program = Parser.Parse(code);
            InterpreterCps.Evaluate(program, environment);
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

            bool RunCommand((string, string) command, IEnvironment environment)
            {
                switch (command)
                {
                    case ("eval", string code):
                        try
                        {
                            var program = Parser.Parse(code);
                            var result = InterpreterCps.Evaluate(program, environment);
                            writer.WriteLine(result);
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

            IEnvironment environment = new EnvironmentEmpty();
            var done = false;
            while (!done)
            {
                var input = Prompt() ?? "";
                var command = ParseInput(input);
                done = RunCommand(command, environment);
            }
            writer.WriteLine("goodbye");
        }

        static void RunFile(string filename)
        {
            try
            {
                Load(filename, new EnvironmentEmpty());
            }
            catch (CrispException e)
            {
                Console.WriteLine(e.FormattedMessage());
            }
        }
    }
}
