using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crisp
{
    class Repl
    {
        public static void Run(TextReader reader, TextWriter writer)
        {
            while (true)
            {
                try
                {
                    writer.Write("> ");
                    var code = reader.ReadLine();
                    var lexer = new Lexer(code);
                    var parser = new Parser(lexer);
                    var expr = parser.ParseExpression();
                    writer.WriteLine(expr.ToString());
                    writer.WriteLine();
                }
                catch (Exception e)
                {
                    writer.WriteLine("Error:");
                    writer.WriteLine(e.Message);
                }
            }
        }
    }
}
