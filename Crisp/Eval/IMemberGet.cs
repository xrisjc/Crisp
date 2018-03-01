namespace Crisp.Eval
{
    interface IMemberGet
    {
        (IObj, GetStatus) MemberGet(string name);
    }
}
