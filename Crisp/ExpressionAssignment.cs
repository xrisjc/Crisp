﻿namespace Crisp
{
    class ExpressionAssignment : IExpression
    {
        public string Name { get; }

        public IExpression Value { get; }

        public ExpressionAssignment(string name, IExpression value)
        {
            Name = name;
            Value = value;
        }

        public IObj Evaluate(Environment environoment)
        {
            var objValue = Value.Evaluate(environoment);
            environoment.Set(Name, objValue);
            return objValue;
        }
    }
}
