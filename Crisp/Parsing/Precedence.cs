namespace Crisp.Parsing
{
    enum Precedence
    {
        Lowest,
        Sequence,
        Assignment,
        LogicalOr,
        LogicalAnd,
        Equality,
        Relational,
        Subtract,
        Additive,
        Multiplicitive,
        Expression,
    }
}
