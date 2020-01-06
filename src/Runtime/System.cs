using Crisp.Ast;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    class System
    {
        delegate CrispObject? Invokeable(
            Interpreter interpreter,
            CrispObject? self,
            CrispObject[] arguments);

        class SystemFunction : CrispObject, ICallable
        {
            Invokeable definition;
            public SystemFunction(CrispObject prototype, Invokeable definition)
                : base(prototype)
            {
                this.definition = definition;
            }
            public CrispObject Invoke(
                Interpreter interpreter,
                CrispObject? self,
                CrispObject[] arguments)
            {
                return definition(interpreter, self, arguments) ?? interpreter.System.Null;
            }
        }

        class Beget : CrispObject, ICallable
        {
            public Beget(CrispObject prototype) : base(prototype) { }
            public CrispObject Invoke(Interpreter interpreter, CrispObject? self, CrispObject[] arguments)
            {
                return self?.Beget() ?? interpreter.System.Null;
            }
        }

        public CrispObject PrototypeObject { get; }
        public CrispObject PrototypeBool { get; }
        public CrispObject PrototypeFunction { get; }
        public CrispObject PrototypeList { get; }
        public CrispObject PrototypeNull { get; }
        public CrispObject PrototypeNumber { get; }
        public CrispObject PrototypeString { get; }
        public CrispObject Null { get; }
        public CrispObject True { get; }
        public CrispObject False { get; }        

        public System()
        {
            PrototypeObject   = new CrispObject(null);
            PrototypeBool     = new CrispObject(PrototypeObject);
            PrototypeFunction = new CrispObject(PrototypeObject);
            PrototypeList     = new CrispObject(PrototypeObject);
            PrototypeNull     = new CrispObject(PrototypeObject);
            PrototypeNumber   = new CrispObject(PrototypeObject);
            PrototypeString   = new CrispObject(PrototypeObject);

            Null = new ObjectNull(PrototypeNull);
            True = new ObjectBool(PrototypeBool, true);
            False = new ObjectBool(PrototypeBool, false);

            Method(PrototypeObject, "beget", (i, s, a) => s?.Beget());

            SetupPrototypeList();
        }

        public CrispObject Create()
            => new CrispObject(PrototypeObject);

        public CrispObject Create(bool value)
            => value ? True : False;
        
        public CrispObject Create(string value)
            => new ObjectString(PrototypeString, value);
        
        public CrispObject Create(Identifier identifier)
            => Create(identifier.Name);
        
        public CrispObject Create(double value)
            => new ObjectNumber(PrototypeNumber, value);

        public CrispObject Create(List<CrispObject> items)
            => new ObjectList(PrototypeList, items);

        public CrispObject Create(Function definition, Environment closure)
            => new ObjectFunction(PrototypeFunction, definition, closure);

        public Environment CreateGlobalEnvironment()
        {
            var env = new Environment();
            env.Create("Object"  , PrototypeObject);
            env.Create("Bool"    , PrototypeBool);
            env.Create("Function", PrototypeFunction);
            env.Create("List"    , PrototypeList);
            env.Create("Null"    , PrototypeNull);
            env.Create("Number"  , PrototypeNumber);
            env.Create("String"  , PrototypeString);
            return env;
        }

        void SetupPrototypeList()
        {
            CrispObject? Add(Interpreter i, CrispObject? s, CrispObject[] a)
                => (s as ObjectList)?.Add(a);

            CrispObject? Length(Interpreter i, CrispObject? s, CrispObject[] a)
                => s switch
                   {
                       ObjectList l => i.System.Create(l.Items.Count),
                       _ => i.System.Null,
                   };

            CrispObject? GetIterator(
                Interpreter i,
                CrispObject? self,
                CrispObject[] a)
            {
                switch (self)
                {
                    case ObjectList list:
                        var items = list.Items;
                        var index = -1;

                        var itr = new CrispObject(PrototypeObject);

                        Method(
                            itr,
                            "next",
                            (interpreter, self, arguments) =>
                            {
                                // TODO... do we need to test if self is
                                // set, i.e. is it called as a method?
                                index++;
                                var hasMore = index < items.Count;
                                return interpreter.System.Create(hasMore);
                            });

                        Method(
                            itr,
                            "current",
                            (interpreter, self, arguments) =>
                            {
                                // TODO... do we need to test if self is
                                // set, i.e. is it called as a method?

                                if (0 <= index && index < items.Count)
                                    return items[index];
                                else
                                    return interpreter.System.Null;
                            });

                        return itr;
                    
                    // Somehow this function wasn't called as a method on
                    // a ObjectList.
                    default:
                        return i.System.Null;
                }
            }


            Method(PrototypeList, "add", Add);
            Method(PrototypeList, "length", Length);
            Method(PrototypeList, "getIterator", GetIterator);
        }

        void Method(CrispObject obj, string name, Invokeable definition)
        {
            var key = Create(name);
            var value = new SystemFunction(PrototypeFunction, definition);
            obj.SetProperty(key, value);
        }
    }
}