namespace Crisp.Parsing
{
    enum Precedence
    {
        Lowest,
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
