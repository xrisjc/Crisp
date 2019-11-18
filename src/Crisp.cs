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
                Repl.Run(Console.In, Console.Out);
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

        static void RunFile(string filename)
        {
            try
            {
                var code = File.ReadAllText(filename);
                var parser = new Parser();
                var program = parser.Parse(code);
                Evaluator.Run(program);
            }
            catch (CrispException e)
            {
                Console.WriteLine(e.FormattedMessage());
            }
        }
    }
}
