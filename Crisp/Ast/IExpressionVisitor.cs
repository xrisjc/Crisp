namespace Crisp.Ast
{
    interface IExpressionVisitor
    {
        void Visit(AssignmentIdentifier assignmentIdentifier);
        void Visit(AssignmentIndexing assignmentIndexing);
        void Visit(AttributeAccess attributeAccess);
        void Visit(AttributeAssignment attributeAssignment);
        void Visit(Block block);
        void Visit(Branch branch);
        void Visit(Call call);
        void Visit(Command command);
        void Visit(For @for);
        void Visit(Function function);
        void Visit(Identifier identifier);
        void Visit(Indexing indexing);
        void Visit(Let let);
        void Visit<T>(Literal<T> literal);
        void Visit(LiteralNull literalNull);
        void Visit(MessageSend messageSend);
        void Visit(OperatorBinary operatorBinary);
        void Visit(OperatorUnary operatorUnary);
        void Visit(Record record);
        void Visit(RecordConstructor recordConstructor);
        void Visit(While @while);
    }
}
