﻿namespace Crisp
{
    class ExpressionLiteral<T> : IExpression
    {
        T value;

        public ExpressionLiteral(T value)
        {
            this.value = value;
        }

        public IObj Evaluate(Environment environoment)
        {
            return Obj.Create(value);
        }
    }
}
