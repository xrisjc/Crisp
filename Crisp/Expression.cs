using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crisp
{
    class ExpressionAdd : IExpression
    {
        public IExpression Left { get; }

        public IExpression Right { get; }

        public ExpressionAdd(IExpression left, IExpression right)
        {
            Left = left;
            Right = right;
        }

        public IObj Evaluate(Environment environoment)
        {
            var objLeft = Left.Evaluate(environoment);
            var objRight = Right.Evaluate(environoment);

            if ((objLeft is Obj<double> numRight) && (objRight is Obj<double> numLeft))
            {
                return Obj.Create(numRight.Value + numLeft.Value);
            }
            else
            {
                throw new RuntimeErrorException("can only add integer values");
            }
        }
    }

    class ExpressionMultiply : IExpression
    {
        public IExpression Left { get; }

        public IExpression Right { get; }

        public ExpressionMultiply(IExpression left, IExpression right)
        {
            Left = left;
            Right = right;
        }

        public IObj Evaluate(Environment environoment)
        {
            var objLeft = Left.Evaluate(environoment);
            var objRight = Right.Evaluate(environoment);

            if ((objLeft is Obj<double> intLeft) && (objRight is Obj<double> intRight))
            {
                return Obj.Create(intLeft.Value * intRight.Value);
            }
            else
            {
                throw new RuntimeErrorException("can only multiply numbers");
            }
        }
    }
}
