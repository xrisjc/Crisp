using Crisp.Ast;
using System;

namespace Crisp.Eval
{
    class Obj<T> : IObj, IEquatable<Obj<T>>
    {
        public T Value { get; }

        public Obj(T value)
        {
            Value = value;
        }

        public Obj(Literal<T> literal)
            : this(literal.Value)
        {
        }

        public string Print()
        {
            if (Value is string strValue)
            {
                return $"'{Value.ToString()}'";
            }
            if (Value is bool boolValue)
            {
                return boolValue ? "true" : "false";
            }
            else
            {
                return Value.ToString();
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public bool Equals(Obj<T> other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is Obj<T> objT)
            {
                return Equals(objT);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    static class Obj
    {
        public static Obj<bool> Create(bool value)
        {
            return value ? True : False;
        }

        public static Obj<T> Create<T>(T value)
        {
            return new Obj<T>(value);
        }

        public static Obj<bool> True { get; } = new Obj<bool>(true);

        public static Obj<bool> False { get; } = new Obj<bool>(false);
    }
}
