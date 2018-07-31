namespace Crisp.IR
{
    class SetEnv
    {
        public string Name { get; }

        public SetEnv(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"setenv {Name}";
        }
    }
}
