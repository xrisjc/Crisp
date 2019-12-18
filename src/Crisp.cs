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
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            try
            {
                var code = File.ReadAllText(filename);
                var program = Parser.Parse(code);
                var system = new Runtime.Library();
                var globals = system.CreateGlobalEnvironment();
                var compiler = new Compiler();
                compiler.Compile(program);
                compiler.Chunk.Dissassemble();
                Vm.Run(compiler.System, compiler.Chunk);
            }
            catch (CrispException e)
            {
                Console.WriteLine(e.FormattedMessage());
            }
            finally
            {
                sw.Stop();
                Console.WriteLine($"{sw.ElapsedMilliseconds} ms");
            }
        }
    }
}
