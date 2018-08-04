namespace Crisp.Ast
{
    interface IExpressionVisitor
    {
        void Visit(AssignmentIdentifier assignmentIdentifier);
        void Visit(AttributeAccess attributeAccess);
        void Visit(AttributeAssignment attributeAssignment);
        void Visit(Block block);
        void Visit(Branch branch);
        void Visit(Call call);
        void Visit(Command command);
        void Visit(For @for);
        void Visit(Function function);
        void Visit(Identifier identifier);
        void Visit<T>(Literal<T> literal);
        void Visit(LiteralNull literalNull);
        void Visit(MessageSend messageSend);
        void Visit(OperatorBinary operatorBinary);
        void Visit(OperatorUnary operatorUnary);
        void Visit(Record record);
        void Visit(RecordConstructor recordConstructor);
        void Visit(This @this);
        void Visit(Var var);
        void Visit(While @while);
    }
}
