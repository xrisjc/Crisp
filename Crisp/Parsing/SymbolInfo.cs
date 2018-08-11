using Crisp.Ast;

namespace Crisp.Parsing
{
    class SymbolInfo
    {
        public SymbolTag Tag { get; }

        public IExpression Value { get; }

        private SymbolInfo(SymbolTag tag, IExpression value = null)
        {
            Tag = tag;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Tag}";
        }

        public static SymbolInfo Attribute()
        {
            return new SymbolInfo(SymbolTag.Attribute);
        }

        public static SymbolInfo Constant(IExpression value)
        {
            return new SymbolInfo(SymbolTag.Constant, value);
        }

        public static SymbolInfo Function(Function function)
        {
            return new SymbolInfo(SymbolTag.Function, function);
        }

        public static SymbolInfo MessageFunction()
        {
            return new SymbolInfo(SymbolTag.MessageFunction);
        }

        public static SymbolInfo Parameter()
        {
            return new SymbolInfo(SymbolTag.Parameter);
        }

        public static SymbolInfo Type()
        {
            return new SymbolInfo(SymbolTag.Type);
        }

        public static SymbolInfo Variable()
        {
            return new SymbolInfo(SymbolTag.Variable);
        }
    }
}
