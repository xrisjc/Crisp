namespace Crisp.Eval
{
    interface IIndexable
    {
        IObj Get(IObj index);
        IObj Set(IObj index, IObj value);
    }
}
