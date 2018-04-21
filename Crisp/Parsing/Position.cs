namespace Crisp.Parsing
{
    class Position
    {
        public int Line { get; }

        public int Column { get; }

        public Position(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public Position IncreaseColumn()
        {
            return new Position(Line, Column + 1);
        }

        public Position IncreaseLine()
        {
            return new Position(Line + 1, column: 1);
        }

        public override string ToString()
        {
            return $"{Line}:{Column}";
        }
    }
}
