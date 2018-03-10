namespace Crisp.Eval
{
    interface IMemberSet
    {
        (IObj, MemberStatus) MemberSet(string name, IObj value);
    }
}
