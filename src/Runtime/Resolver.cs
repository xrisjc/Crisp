using Crisp.Ast;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Resolver
    {
        public enum Kind { Static, Local }
        public enum State { Declared, Defined }

        public struct Symbol
        {
            public State State { get; }
            public Kind Kind { get; }
            public int Index { get; }
            public bool IsDefined => State == State.Defined;
            public Symbol(State state, Kind kind, int index)
            {
                State = state;
                Kind = kind;
                Index = index;
            }
            public Symbol(Kind kind, int index)
                : this(State.Declared, kind, index)
            {
            }
            public Symbol Defined()
                => new Symbol(State.Defined, Kind, Index);
        }
        
        class Scope : Dictionary<string, Symbol>
        {
        }

        Stack<int> indexStack = new Stack<int>();
        Stack<List<Scope>> scopesStack = new Stack<List<Scope>>();
        int nextIndex = 0;
        Scope globals = new Scope();
        List<Scope> scopes;

        public Dictionary<Identifier, (Kind, int)> Indices =
            new Dictionary<Identifier, (Kind, int)>();

        public Dictionary<Function, int> LocalCount =
            new Dictionary<Function, int>();

        public Resolver(Program program)
        {
            scopes = new List<Scope> { globals };
            foreach (var e in program.Expressions)
                Resolve(e);
        }

        void Resolve(IExpression expression)
        {
            switch (expression)
            {
                case AssignmentIdentifier ai:
                    Resolve(ai.Value);
                    Resolve(ai.Target);
                    break;

                case AssignmentIndex ai:
                    Resolve(ai.Index);
                    Resolve(ai.Value);
                    break;

                case AssignmentRefinement ar:
                    Resolve(ar.Refinement);
                    Resolve(ar.Value);
                    break;

                case Block b:
                    BeginScope();
                    foreach (var e in b.Body)
                        Resolve(e);
                    EndScope();
                    break;

                case Call c:
                    Resolve(c.Target);
                    foreach (var e in c.Arguments)
                        Resolve(e);
                    break;

                case Function f:
                    BeginFrame();
                    BeginScope();
                    foreach (var p in f.Parameters)
                    {
                        Declare(p);
                        Define(p);
                    }
                    Resolve(f.Body);
                    LocalCount[f] = nextIndex;
                    EndScope();
                    EndFrame();
                    break;

                case Identifier i:
                    Resolve(i);
                    break;

                case If i:
                    Resolve(i.Condition);
                    BeginScope();
                    Resolve(i.Consequence);
                    EndScope();
                    BeginScope();
                    Resolve(i.Alternative);
                    EndScope();
                    break;

                case OperatorBinary ob:
                    Resolve(ob.Left);
                    Resolve(ob.Right);
                    break;

                case OperatorUnary ou:
                    Resolve(ou.Expression);
                    break;

                case Refinement r:
                    Resolve(r.Target);
                    break;

                case Var v:
                    Declare(v.Name);
                    Resolve(v.InitialValue);
                    Define(v.Name);
                    break;

                case While @while:
                    Resolve(@while.Guard);
                    BeginScope();
                    Resolve(@while.Body);
                    EndScope();
                    break;

                case Write write:
                    foreach (var expr in write.Arguments)
                        Resolve(expr);
                    break;
            }
        }

        void Resolve(Identifier identifier)
        {
            if (Lookup(identifier) is Symbol s && s.IsDefined)
                Indices[identifier] = (s.Kind, s.Index);
            else
                throw new RuntimeErrorException(
                    identifier.Position,
                    $"Undefined variable <{identifier.Name}>.");

        }

        Symbol? Lookup(Identifier identifier)
        {
            for (var i = scopes.Count - 1; i >= 0; i--)
                if (scopes[i].ContainsKey(identifier.Name))
                    return scopes[i][identifier.Name];
            
            return null;
        }

        void Declare(Identifier identifier)
        {
            var scope = PeekScope();

            if (scope.ContainsKey(identifier.Name))
                throw new RuntimeErrorException(
                    identifier.Position,
                    $"Variable `{identifier.Name}` is already declared");
            
            var kind = indexStack.Count > 0 ? Kind.Local : Kind.Static;
            var index = nextIndex++;
            var symbol = new Symbol(kind, index);

            scope[identifier.Name] = symbol;
            Indices[identifier] = (kind, index);
        }

        void Define(Identifier identifier)
        {
            var scope = PeekScope();
            var name = identifier.Name;

            scope[name] = scope[name].Defined();
        }

        Scope PeekScope()
        {
            return scopes[scopes.Count - 1];
        }

        void BeginScope()
        {
            scopes.Add(new Scope());
        }

        void EndScope()
        {
            scopes.RemoveAt(scopes.Count - 1);
        }

        void BeginFrame()
        {
            indexStack.Push(nextIndex);
            nextIndex = 0;

            scopesStack.Push(scopes);
            scopes = new List<Scope> { globals };
        }

        void EndFrame()
        {
            scopes = scopesStack.Pop();
            nextIndex = indexStack.Pop();
        }
    }
}