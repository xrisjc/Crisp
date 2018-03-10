namespace Crisp.Eval
{
    interface IMemberGet
    {
        (IObj, MemberStatus) MemberGet(string name);
    }
}
