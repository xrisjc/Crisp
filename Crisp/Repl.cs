using System.IO;

namespace Crisp
{
    class Repl
    {
        public static void Run(TextReader reader, TextWriter writer)
        {
            var environment = new Environment();
            environment.Create("writeLn", new ObjectFunctionWriteLn());

            {
                var sys = File.ReadAllText("Sys.crisp");
                var lexer = new Lexer(sys);
                var parser = new Parser(lexer);
                var expr = parser.Parse();
                expr.Evaluate(environment);
            }

            while (true)
            {
                try
                {
                    writer.Write("> ");
                    var code = reader.ReadLine();
                    var lexer = new Lexer(code);
                    var parser = new Parser(lexer);
                    var expr = parser.Parse();
                    writer.WriteLine(expr.ToString());
                    var obj = expr.Evaluate(environment);
                    writer.WriteLine(obj);
                }
                catch (SyntaxErrorException e)
                {
                    writer.WriteLine($"Syntax Error: {e.Message}");
                }
                catch (RuntimeErrorException e)
                {
                    writer.WriteLine($"Runtime Error: {e.Message}");
                }
            }
        }
    }
}
