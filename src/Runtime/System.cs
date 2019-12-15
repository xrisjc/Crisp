﻿using Crisp.Ast;
using System;

namespace Crisp.Runtime
{
    class System
    {
        public CrispObject PrototypeObject { get; }
        public CrispObject PrototypeBool { get; }
        public CrispObject PrototypeFunction { get; }
        public CrispObject PrototypeNull { get; }
        public CrispObject PrototypeNumber { get; }
        public CrispObject PrototypeString { get; }
        public CrispObject Null { get; }

        public System()
        {
            PrototypeObject   = new CrispObject(null);
            PrototypeBool     = new CrispObject(PrototypeObject);
            PrototypeFunction = new CrispObject(PrototypeObject);
            PrototypeNull     = new CrispObject(PrototypeObject);
            PrototypeNumber   = new CrispObject(PrototypeObject);
            PrototypeString   = new CrispObject(PrototypeObject);

            Null = new ObjectNull(PrototypeNull);
        }

        public CrispObject Beget(CrispObject obj)
            => new CrispObject(obj);

        public CrispObject Create()
            => new CrispObject(PrototypeObject);

        public CrispObject Create(bool value)
            => new ObjectBool(PrototypeBool, value);
        
        public CrispObject Create(string value)
            => new ObjectString(PrototypeString, value);
        
        public CrispObject Create(Identifier identifier)
            => Create(identifier.Name);
        
        public CrispObject Create(double value)
            => new ObjectNumber(PrototypeNumber, value);

        public CrispObject Create(Function definition)
            => new ObjectFunction(PrototypeFunction, definition);

        public Environment CreateGlobalEnvironment()
        {
            var env = new Environment();
            env.Create("Object"  , PrototypeObject);
            env.Create("Bool"    , PrototypeBool);
            env.Create("Function", PrototypeFunction);
            env.Create("Null"    , PrototypeNull);
            env.Create("Number"  , PrototypeNumber);
            env.Create("String"  , PrototypeString);
            return env;
        }
    }
}