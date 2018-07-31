using Crisp.Ast;
using System;

namespace Crisp.IR
{
    class Translator : IExpressionVisitor
    {
        void Emit(object o)
        {
            Console.WriteLine(o);
        }

        void Emit<T>() where T : new()
        {
            Console.WriteLine(new T());
        }

        public void Visit(AssignmentIdentifier assignmentIdentifier)
        {
            throw new NotImplementedException();
        }

        public void Visit(AssignmentIndexing assignmentIndexing)
        {
            throw new NotImplementedException();
        }

        public void Visit(AttributeAccess attributeAccess)
        {
            throw new NotImplementedException();
        }

        public void Visit(AttributeAssignment attributeAssignment)
        {
            throw new NotImplementedException();
        }

        public void Visit(Block block)
        {
            if (block.Body.Count == 0)
            {
                Emit<PushNull>();
                return;
            }

            Emit<PushEnv>();
            block.Body[0].Accept(this);
            for (int i = 1; i < block.Body.Count; i++)
            {
                Emit<Pop>();
                block.Body[i].Accept(this);
            }
            Emit<PopEnv>();
        }

        public void Visit(Branch branch)
        {
            throw new NotImplementedException();
        }

        public void Visit(Call call)
        {
            throw new NotImplementedException();
        }

        public void Visit(Command command)
        {
            throw new NotImplementedException();
        }

        public void Visit(For @for)
        {
            throw new NotImplementedException();
        }

        public void Visit(ForIn forIn)
        {
            throw new NotImplementedException();
        }

        public void Visit(Ast.Function function)
        {
            throw new NotImplementedException();
        }

        public void Visit(Identifier identifier)
        {
            Emit(new GetEnv(identifier.Name));
        }

        public void Visit(Indexing indexing)
        {
            throw new NotImplementedException();
        }

        public void Visit(Let let)
        {
            let.Value.Accept(this);
            Emit(new SetEnv(let.Identifier.Name));
        }

        public void Visit(Ast.List list)
        {
            throw new NotImplementedException();
        }

        public void Visit<T>(Literal<T> literal)
        {
            Emit(new Push(literal.Value));
        }

        public void Visit(LiteralNull literalNull)
        {
            Emit<PushNull>();
        }

        public void Visit(Ast.Map map)
        {
            throw new NotImplementedException();
        }

        public void Visit(MessageSend messageSend)
        {
            throw new NotImplementedException();
        }

        public void Visit(OperatorBinary operatorBinary)
        {
            if (operatorBinary.Op == OperatorInfix.And)
            {
                return;
            }

            if (operatorBinary.Op == OperatorInfix.Or)
            {
                return;
            }

            operatorBinary.Left.Accept(this);
            operatorBinary.Right.Accept(this);
            switch (operatorBinary.Op)
            {
                case OperatorInfix.Add:
                    break;
                case OperatorInfix.Sub:
                    break;
                case OperatorInfix.Mul:
                    break;
                case OperatorInfix.Div:
                    break;
                case OperatorInfix.Mod:
                    break;
                case OperatorInfix.Lt:
                    break;
                case OperatorInfix.LtEq:
                    break;
                case OperatorInfix.Gt:
                    break;
                case OperatorInfix.GtEq:
                    break;
                case OperatorInfix.Eq:
                    break;
                case OperatorInfix.Neq:
                    break;
            }
        }

        public void Visit(OperatorUnary operatorUnary)
        {
            operatorUnary.Expression.Accept(this);
            switch (operatorUnary.Op)
            {
                case OperatorPrefix.Neg:
                    Emit<Neg>();
                    break;
                case OperatorPrefix.Not:
                    Emit<Not>();
                    break;
            }
        }

        public void Visit(Ast.Record record)
        {
            throw new NotImplementedException();
        }

        public void Visit(RecordConstructor recordConstructor)
        {
            throw new NotImplementedException();
        }

        public void Visit(While @while)
        {
            throw new NotImplementedException();
        }
    }
}
