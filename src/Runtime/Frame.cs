namespace Crisp.Runtime
{
    class Frame
    {
        public int Offset { get; set; }
        public CrispObject? Self { get; }
        public Environment2 Environment { get; private set; }

        public Frame(
            int offset,
            Environment2 environment,
            CrispObject? self)
        {
            Offset = offset;
            Environment = environment;
            Self = self;
        }

        public void StartBlock()
        {
            Environment = new Environment2(Environment);
        }

        public void EndBlock()
        {
            Environment = Environment.Outer;
        }
    }
}