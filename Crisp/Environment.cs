namespace Crisp
{
    class Environment
    {
        public Environment(Environment outer)
        {
        }

        public IObject this[string name]
        {
            get { return ObjectNull.Instance; }
            set { }
        }
    }
}
