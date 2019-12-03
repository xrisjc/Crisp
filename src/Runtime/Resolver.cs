using Crisp.Ast;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    /// <summary>
    /// Resolves local variables.
    /// </summary>
    class Resolver
    {
        enum VarState { Declared, Defined }
        
        class Scope : Dictionary<string, VarState> { }

        readonly List<Scope> scopes = new List<Scope>();

        public void Resolve(Program program)
        {
            foreach (var pi in program.ProgramItems)
                Resolve(pi);
        }

        void Resolve(IProgramItem programItem)
        {
            switch (programItem)
            {
                case Function function:
                    BeginScope();
                    foreach (var p in function.Parameters)
                        Resolve(p);
                    Resolve(function.Body);
                    EndScope();
                    break;

                case Record record:
                    foreach (var fn in record.Functions.Values)
                        Resolve(fn);
                    break;

                case IExpression expr:
                    Resolve(expr);
                    break;
            }
        }

        void Resolve(IExpression expression)
        {
            switch (expression)
            {
                case AssignmentIdentifier ai:
                    Resolve(ai.Value);
                    ResolveLocal(ai, ai.Target.Name);
                    break;

                case AttributeAccess aa:
                    Resolve(aa.Entity);
                    break;

                case AttributeAssignment aa:
                    Resolve(aa.Entity);
                    Resolve(aa.Value);
                    break;

                case Block block:
                    foreach (var e in block.Body)
                        Resolve(e);
                    break;

                case Call call:
                    foreach (var e in call.Arguments)
                        Resolve(e);
                    break;

                case Condition condition:
                    foreach (var branch in condition.Branches)
                    {
                        Resolve(branch.Condition);
                        Resolve(branch.Consequence);
                    }
                    break;

                case Function fn:
                    BeginScope();
                    foreach (var p in fn.Parameters)
                    {
                        Declare(p.Name);
                        Define(p.Name);
                    }
                    EndScope();
                    break;

                case Identifier identifier
                when PeekScope()?[identifier.Name] == VarState.Declared:
                    throw new RuntimeErrorException(
                        identifier.Position,
                        "Cannot read a local variable in its own initializer.");

                case Identifier identifier:
                    ResolveLocal(identifier, identifier.Name);
                    break;

                case MessageSend ms:
                    Resolve(ms.EntityExpr);
                    foreach (var e in ms.ArgumentExprs)
                        Resolve(e);
                    break;

                case OperatorBinary op:
                    Resolve(op.Left);
                    Resolve(op.Right);
                    break;

                case OperatorUnary op:
                    Resolve(op.Expression);
                    break;

                case Var var:
                    Declare(var.Name.Name);
                    Resolve(var.InitialValue);
                    Define(var.Name.Name);
                    break;

                case While @while:
                    Resolve(@while.Guard);
                    Resolve(@while.Body);
                    break;

                case Write write:
                    foreach (var expr in write.Arguments)
                        Resolve(expr);
                    break;
            }
        }

        void ResolveLocal(IExpression expression, string name)
        {
            for (var i = scopes.Count - 1; i >= 0; i--)
                if (scopes[i].ContainsKey(name))
                {

                    return;
                }
        }

        void Declare(string name)
        {
            if (PeekScope() is Scope scope)
                scope[name] = VarState.Declared;
        }

        void Define(string name)
        {
            if (PeekScope() is Scope scope)
                scope[name] = VarState.Defined;
        }

        Scope? PeekScope()
        {
            if (scopes.Count == 0)
                return null;

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
    }
}
