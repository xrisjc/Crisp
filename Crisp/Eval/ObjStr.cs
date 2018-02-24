﻿using System;
using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjStr : IObj, IIndexGet, IEquatable<ObjStr>
    {
        public string Value { get; }

        public ObjStr(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }

        public string Print()
        {
            return $"'{Value}'";
        }

        public IObj Get(IObj index)
        {
            return ObjNull.Instance;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ObjStr);
        }

        public bool Equals(ObjStr other)
        {
            return other != null &&
                   Value == other.Value;
        }

        public override int GetHashCode()
        {
            return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
        }
    }
}
