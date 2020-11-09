using Crisp.Parsing;
using Crisp.Runtime;
using System;
using System.Collections.Immutable;
using System.IO;

namespace Crisp
{
    class Crisp
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                Console.WriteLine("usage: crisp [filename]");
            }
        }

        static void RunFile(string filename)
        {
            try
            {
                var code = File.ReadAllText(filename);
                var program = Parser.Parse(code);
                Interpreter.Evaluate(program);
                // InterpreterCps.Evaluate(program, ImmutableList<Cell>.Empty);
            }
            catch (CrispException e)
            {
                Console.WriteLine(e.FormattedMessage());
            }
        }
    }
}
