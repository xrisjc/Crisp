﻿namespace Crisp.Eval
{
    class ObjBool : IObj
    {
        public bool Value { get; }

        public Type ObjType => Type.Bool;

        private ObjBool(bool value)
        {
            Value = value;
        }

        public static ObjBool True = new ObjBool(true);

        public static ObjBool False = new ObjBool(false);

        public override string ToString()
        {
            return Value ? "true" : "false";
        }
    }
}
