namespace Crisp.IR
{
    class GetEnv
    {
        public string Name { get; }

        public GetEnv(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"getenv {Name}";
        }
    }
}
