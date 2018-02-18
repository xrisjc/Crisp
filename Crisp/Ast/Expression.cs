using Crisp.Eval;
using System.Collections.Generic;
using System.Linq;

namespace Crisp.Ast
{
    interface IExpression
    {
        IObj Evaluate(Environment environment);
    }

    class ExpressionAssignmentVariable : IExpression
    {
        ExpressionIdentifier identifier;
        IExpression value;

        public ExpressionAssignmentVariable(ExpressionIdentifier identifier, IExpression value)
        {
            this.identifier = identifier;
            this.value = value;
        }

        public IObj Evaluate(Environment environoment)
        {
            var objValue = value.Evaluate(environoment);
            environoment.Set(identifier.Name, objValue);
            return objValue;
        }
    }

    class ExpressionAssignmentIndex : IExpression
    {
        ExpressionIndex index;
        IExpression value;

        public ExpressionAssignmentIndex(ExpressionIndex index,
            IExpression value)
        {
            this.index = index;
            this.value = value;
        }

        public IObj Evaluate(Environment environment)
        {
            var objectRight = index.Object.Evaluate(environment);
            if (objectRight is IIndexable objIndexable)
            {
                var objIndex = index.Index.Evaluate(environment);
                var objValue = value.Evaluate(environment);
                objIndexable.Set(objIndex, objValue);
                return objValue;
            }
            else
            {
                throw new RuntimeErrorException(
                    "Non-indexable object indexed.");
            }
        }
    }

    class ExpressionBlock : IExpression
    {
        IEnumerable<IExpression> body;

        public ExpressionBlock(IEnumerable<IExpression> body)
        {
            this.body = body;
        }

        public IObj Evaluate(Environment environment)
        {
            var localEnvironment = new Environment(environment);
            IObj result = Obj.Null;
            foreach (var expression in body)
            {
                result = expression.Evaluate(localEnvironment);
            }
            return result;
        }
    }

    class ExpressionBranch : IExpression
    {
        IExpression condition, consequence, alternative;

        public ExpressionBranch(IExpression condition, IExpression consequence,
            IExpression alternative)
        {
            this.condition = condition;
            this.consequence = consequence;
            this.alternative = alternative;
        }

        public IObj Evaluate(Environment environment)
        {
            var objResult = condition.Evaluate(environment);
            if (objResult is Obj<bool> boolResult)
            {
                var expression = boolResult.Value
                    ? consequence
                    : alternative;
                return expression.Evaluate(environment);
            }

            throw new RuntimeErrorException(
                "an if condition must be a bool value");
        }
    }

    class ExpressionCall : IExpression
    {
        IExpression functionExpression;
        List<IExpression> argumentExpressions;

        public ExpressionCall(IExpression functionExpression, List<IExpression> argumentExpressions)
        {
            this.functionExpression = functionExpression;
            this.argumentExpressions = argumentExpressions;
        }

        public ExpressionCall(IExpression functionExpression)
            : this(functionExpression, new List<IExpression>())
        {

        }

        public IObj Evaluate(Environment environment)
        {
            if (functionExpression.Evaluate(environment) is IObjFn function)
            {
                var arguments = argumentExpressions.Select(arg => arg.Evaluate(environment))
                                                   .ToList();
                return function.Call(arguments);
            }
            else
            {
                throw new RuntimeErrorException("function call attempted on non function value");
            }
        }
    }

    class ExpressionFunction : IExpression
    {
        string name;
        IExpression body;
        List<string> parameters;

        public ExpressionFunction(string name, List<string> parameters, IExpression body)
        {
            this.name = name;
            this.body = body;
            this.parameters = parameters;
        }

        public IObj Evaluate(Environment environoment)
        {
            var function = new ObjFnNative(body, parameters, environoment);
            if (name != null)
            {
                environoment.Create(name, function);
            }
            return function;
        }
    }

    class ExpressionIdentifier : IExpression
    {
        public string Name { get; }

        public ExpressionIdentifier(string name)
        {
            Name = name;
        }

        public IObj Evaluate(Environment env) => env.Get(Name);
    }

    class ExpressionLet : IExpression
    {
        string name;
        IExpression value;

        public ExpressionLet(string name, IExpression value)
        {
            this.name = name;
            this.value = value;
        }

        public IObj Evaluate(Environment environoment)
        {
            var objValue = value.Evaluate(environoment);
            environoment.Create(name, objValue);
            return objValue;
        }
    }

    class ExpressionLiteral<T> : IExpression
    {
        T value;

        public ExpressionLiteral(T value)
        {
            this.value = value;
        }

        public IObj Evaluate(Environment environoment) => Obj.Create(value);
    }

    class ExpressionLiteralNull : IExpression
    {
        private ExpressionLiteralNull() { }

        public IObj Evaluate(Environment environment) => Obj.Null;

        public static ExpressionLiteralNull Instance { get; } =
            new ExpressionLiteralNull();
    }

    class ExpressionLogicalAnd : IExpression
    {
        IExpression left, right;

        public ExpressionLogicalAnd(IExpression left, IExpression right)
        {
            this.left = left;
            this.right = right;
        }

        public IObj Evaluate(Environment environment)
        {
            var objLeft = left.Evaluate(environment);
            if (objLeft is Obj<bool> boolLeft)
            {
                if (boolLeft.Value == false)
                {
                    return boolLeft;
                }

                var objRight = right.Evaluate(environment);
                if (objRight is Obj<bool> boolRight)
                {
                    return boolRight;
                }
                else
                {
                    throw new RuntimeErrorException(
                        "Right hand side of 'and' must be Boolean.");
                }
            }
            else
            {
                throw new RuntimeErrorException(
                    "Left hand side of 'and' must be Boolean.");
            }
        }
    }

    class ExpressionLogicalOr : IExpression
    {
        IExpression left, right;

        public ExpressionLogicalOr(IExpression left, IExpression right)
        {
            this.left = left;
            this.right = right;
        }

        public IObj Evaluate(Environment environment)
        {
            var objLeft = left.Evaluate(environment);
            if (objLeft is Obj<bool> boolLeft)
            {
                if (boolLeft.Value)
                {
                    return boolLeft;
                }

                var objRight = right.Evaluate(environment);
                if (objRight is Obj<bool> boolRight)
                {
                    return boolRight;
                }
                else
                {
                    throw new RuntimeErrorException(
                        "Right hand side of 'and' must be Boolean.");
                }
            }
            else
            {
                throw new RuntimeErrorException(
                    "Left hand side of 'or' must be Boolean.");
            }
        }
    }

    class ExpressionMap : IExpression
    {
        List<IndexValuePair> initializers;

        public ExpressionMap(List<IndexValuePair> initializers)
        {
            this.initializers = initializers;
        }

        public ExpressionMap()
            : this(new List<IndexValuePair>())
        {
        }

        public IObj Evaluate(Environment environment)
        {
            var map = new ObjMap();
            foreach (var initializer in initializers)
            {
                map.Set(initializer, environment);
            }
            return map;
        }
    }

    class ExpressionIndex : IExpression
    {
        public IExpression Object { get; }

        public IExpression Index { get; }

        public ExpressionIndex(IExpression @object, IExpression index)
        {
            Object = @object;
            Index = index;
        }

        public IObj Evaluate(Environment environment)
        {
            var objectRight = Object.Evaluate(environment);
            if (objectRight is IIndexable objIndexable)
            {
                var objIndex = Index.Evaluate(environment);
                var objValue = objIndexable.Get(objIndex);
                return objValue;
            }
            else
            {
                throw new RuntimeErrorException(
                    "Non-indexable object indexed.");
            }
        }
    }

    class ExpressionOperatorBinary : IExpression
    {
        IOperatorBinary op;
        IExpression left, right;

        public ExpressionOperatorBinary(IOperatorBinary op, IExpression left,
            IExpression right)
        {
            this.op = op;
            this.left = left;
            this.right = right;
        }

        public IObj Evaluate(Environment environment)
        {
            var objLeft = left.Evaluate(environment);
            var objRight = right.Evaluate(environment);
            var result = op.Evaluate(objLeft, objRight);
            return result;
        }
    }

    class ExpressionWhile : IExpression
    {
        IExpression guard;
        IExpression body;

        public ExpressionWhile(IExpression guard, IExpression body)
        {
            this.guard = guard;
            this.body = body;
        }

        public IObj Evaluate(Environment environment)
        {
            while (true)
            {
                var predicate = guard.Evaluate(environment);
                if (predicate is Obj<bool> boolPredicate)
                {
                    if (boolPredicate.Value == false)
                    {
                        return Obj.Null;
                    }
                }
                else
                {
                    throw new RuntimeErrorException("a while guard must be a bool value");
                }
                body.Evaluate(environment);
            }
        }
    }
}
