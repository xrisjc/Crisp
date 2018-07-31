namespace Crisp.IR
{
    class Push
    {
        public object Value { get; set; }

        public Push(object value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"push <{Value}>";
        }
    }
}
