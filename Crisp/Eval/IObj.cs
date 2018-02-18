namespace Crisp.Eval
{
    interface IObj
    {
        string Print();
    }

    interface IIndexable
    {
        IObj Get(IObj index);
        void Set(IObj index, IObj value);
    }
}
