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
                case ":q":
                    return true;
                case ":e":
                    environment.Write();
                    break;
                case ":st":
                    symbolTable.Write();
                    break;
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
            var program = parser.Program();
            var result = Evaluator.Run(program, environment);
            writer.WriteLine(result);
        }
    }
}
