using Crisp.Runtime;

namespace Crisp.Ast
{
    class AttributeAssignment : IExpression
    {
        public IExpression Entity { get; }

        public string Name { get; }

        public IExpression Value { get; }

        public AttributeAssignment(IExpression entity, string name, IExpression value)
        {
            Entity = entity;
            Name = name;
            Value = value;
        }

        public object Evaluate(Environment environment)
        {
            var entity = Entity.Evaluate(environment) as Entity;
            if (entity == null)
            {
                throw new RuntimeErrorException("attribute assignment on non entity object");
            }

            var value = Value.Evaluate(environment);

            if (entity.SetAttribute(Name, value) == false)
            {
                throw new RuntimeErrorException($"attritue {Name} not found.");
            }

            return value;
        }
    }
}
