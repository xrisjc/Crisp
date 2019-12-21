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
                var system = new Runtime.Library();
                var compiler = new Compiler(program, system);
                // compiler.Chunk.Dissassemble();
                Vm.Run(system, compiler.Chunk);
            }
            catch (CrispException e)
            {
                Console.WriteLine(e.FormattedMessage());
            }
        }
    }
}
