using Crisp.Eval;
using Crisp.Parsing;
using System;
using System.IO;

namespace Crisp
{
    class Repl
    {
        public static void Run(TextReader reader, TextWriter writer)
        {
            var environment = new Eval.Environment();
            environment.Create("writeLn", new ObjFnWriteLn());
            environment.Create("readLn", new ObjFnReadLn());

            //Load("Sys.crisp", environment);
            Load("Test.crisp", environment);

            while (true)
            {
                try
                {
                    writer.Write("> ");
                    var code = reader.ReadLine();
                    var lexer = new Lexer(code);
                    var parser = new Parser(lexer);
                    var expr = parser.ParseExpression();
                    var obj = expr.Evaluate(environment);
                    writer.WriteLine(obj.Print());
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

        public static void Load(string filename, Eval.Environment environment)
        {
            try
            {
                var sys = File.ReadAllText(filename);
                var lexer = new Lexer(sys);
                var parser = new Parser(lexer);
                while (!parser.IsFinished)
                {
                    var expr = parser.ParseExpression();
                    expr.Evaluate(environment);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
