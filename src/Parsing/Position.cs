namespace Crisp.Parsing
{
    record Position(int Line, int Column)
    {
        public Position IncreaseColumn() => this with { Column = Column + 1 };

        public Position IncreaseLine() => new Position(Line + 1, 1);

        public override string ToString() => $"{Line}:{Column}";
    }
}
