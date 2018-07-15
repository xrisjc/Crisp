using System;
using System.Collections.Generic;
using System.Linq;
using Crisp.Runtime;

namespace Crisp.Ast
{
    class Command : IExpression
    {
        public CommandType Type { get; }

        public IEnumerable<IExpression> ArgumentExpressions { get; }

        public Command(CommandType type, IEnumerable<IExpression> argumentExpressions)
        {
            Type = type;
            ArgumentExpressions = argumentExpressions;
        }

        public object Evaluate(Runtime.Environment environment)
        {
            List<dynamic> args = ArgumentExpressions.Evaluate(environment).ToList();

            switch (Type)
            {
                case CommandType.ReadLn:
                    {
                        if (args.Count > 0)
                        {
                            var prompt = string.Join("", args);
                            Console.Write(prompt);
                        }
                        var line = Console.ReadLine();
                        return line;
                    }

                case CommandType.WriteLn:
                    {
                        var line = string.Join("", args);
                        Console.WriteLine(line);
                        return Null.Instance;
                    }

                default:
                    throw new RuntimeErrorException($"unknown command {Type}");
            }
        }
    }
}
