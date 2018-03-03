namespace Crisp.Eval
{
    interface IMemberSet
    {
        (IObj, SetStatus) MemberSet(string name, IObj value);
    }
}
