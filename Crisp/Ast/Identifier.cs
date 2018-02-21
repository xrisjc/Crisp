﻿namespace Crisp.Ast
{
    class Identifier : IExpression
    {
        public string Name { get; }

        public Identifier(string name)
        {
            Name = name;
        }
    }
}
