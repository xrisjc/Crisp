namespace Crisp.Ast
{
    interface IExpressionVisitor
    {
        void Visit(AssignmentIdentifier assignmentIdentifier);
        void Visit(AttributeAccess attributeAccess);
        void Visit(AttributeAssignment attributeAssignment);
        void Visit(Block block);
        void Visit(Call call);
        void Visit(Condition condition);
        void Visit(Identifier identifier);
        void Visit(Literal literal);
        void Visit(MessageSend messageSend);
        void Visit(OperatorBinary operatorBinary);
        void Visit(OperatorUnary operatorUnary);
        void Visit(RecordConstructor recordConstructor);
        void Visit(This @this);
        void Visit(Var var);
        void Visit(While @while);
        void Visit(Write command);
    }
}
