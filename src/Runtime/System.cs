using Crisp.Ast;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    class System
    {
        public Obj PrototypeObject { get; }
        public Obj PrototypeBool { get; }
        public Obj PrototypeFunction { get; }
        public Obj PrototypeList { get; }
        public Obj PrototypeNull { get; }
        public Obj PrototypeNumber { get; }
        public Obj PrototypeString { get; }
        public Obj Null { get; }
        public Obj True { get; }
        public Obj False { get; }        

        public System()
        {
            PrototypeObject   = new Obj(null);
            PrototypeBool     = new Obj(PrototypeObject);
            PrototypeFunction = new Obj(PrototypeObject);
            PrototypeList     = new Obj(PrototypeObject);
            PrototypeNull     = new Obj(PrototypeObject);
            PrototypeNumber   = new Obj(PrototypeObject);
            PrototypeString   = new Obj(PrototypeObject);

            Null = new Obj(PrototypeNull, new Null());
            True = new Obj(PrototypeBool, true);
            False = new Obj(PrototypeBool, false);

            Method(PrototypeObject, "beget", (i, s, a) => s?.Beget() ?? i.System.Null);

            SetupPrototypeList();
        }

        public Obj Create(bool value) => value ? True : False;
        
        public Obj Create(string value) => new Obj(PrototypeString, value);
        
        public Obj Create(Identifier identifier) => Create(identifier.Name);
        
        public Obj Create(double value) => new Obj(PrototypeNumber, value);

        public Obj Create(List<Obj> items) => new Obj(PrototypeList, items);
        
        public Obj Create(Callable callable) => new Obj(PrototypeFunction, callable);

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
            Obj Add(Interpreter i, Obj? s, Obj[] a)
            {
                switch (s?.Value)
                {
                    case List<Obj> list:
                        list.AddRange(a);
                        return s;
                    default:
                        return i.System.Null;
                }
            }

            Obj Length(Interpreter i, Obj? s, Obj[] a)
                => s?.Value switch
                {
                    List<Obj> list => i.System.Create(list.Count),
                    _ => i.System.Null,
                };

            Obj GetIterator(Interpreter i, Obj? self, Obj[] a)
            {
                switch (self?.Value)
                {
                    case List<Obj> list:
                        var itr = PrototypeObject.Beget();
                        var index = -1;

                        Method(
                            itr,
                            "next",
                            (i, s, a) => i.System.Create(++index < list.Count));

                        Method(
                            itr,
                            "current",
                            (i, s, a) =>
                                (0 <= index && index < list.Count)
                                    ? list[index]
                                    : i.System.Null);

                        return itr;
                    
                    default:
                        return i.System.Null;
                }
            }


            Method(PrototypeList, "add", Add);
            Method(PrototypeList, "length", Length);
            Method(PrototypeList, "getIterator", GetIterator);
        }

        void Method(Obj obj, string name, Callable callable)
        {
            obj.SetProperty(Create(name), Create(callable));
        }
    }
}