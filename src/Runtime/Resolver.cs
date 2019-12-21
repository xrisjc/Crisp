using Crisp.Ast;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Resolver
    {
        enum State { Declared, Defined }

        struct Symbol
        {
            public State State { get; }
            public (int, int) Position { get; }
            public bool IsDefined => State == State.Defined;
            public Symbol(State state, (int, int) position)
            {
                State = state;
                Position = position;
            }
            public Symbol(int level, int index)
                : this(State.Declared, (level, index))
            {
            }
            public Symbol Defined()
                => new Symbol(State.Defined, Position);
        }
        
        class Scope : Dictionary<string, Symbol>
        {
        }

        Stack<int> indexStack = new Stack<int>();
        int nextIndex = 0;
        List<Scope> scopes = new List<Scope>();

        public Dictionary<Identifier, (int, int)> Positions =
            new Dictionary<Identifier, (int, int)>();

        public Resolver(Program program)
        {
            BeginScope();
            foreach (var e in program.Expressions)
                Resolve(e);
            EndScope();
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
                    BeginScope();
                    foreach (var p in f.Parameters)
                    {
                        Declare(p);
                        Define(p);
                    }
                    Resolve(f.Body);
                    EndScope();
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
                Positions[identifier] = s.Position;
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
            
            var level = scopes.Count - 1;
            var index = nextIndex++;
            var symbol = new Symbol(level, index);

            scope[identifier.Name] = symbol;
            Positions[identifier] = symbol.Position;
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
            indexStack.Push(nextIndex);
            nextIndex = 0;
            scopes.Add(new Scope());
        }

        void EndScope()
        {
            nextIndex = indexStack.Pop();
            scopes.RemoveAt(scopes.Count - 1);
        }
    }
}