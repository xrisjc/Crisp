using Crisp.Runtime;

namespace Crisp.Ast
{
    class AttributeAccess : IExpression
    {
        public IExpression Entity { get; }

        public string Name { get; }

        public AttributeAccess(IExpression entity, string name)
        {
            Entity = entity;
            Name = name;
        }

        public object Evaluate(Environment environment)
        {
            var entity = Entity.Evaluate(environment) as IEntity;
            if (entity == null)
            {
                throw new RuntimeErrorException("attribute access on non entity object");
            }

            if (entity.GetAttribute(Name, out var value))
            {
                return value;
            }
            else
            {
                throw new RuntimeErrorException($"attribute {Name} not found");
            }
        }
    }
}
