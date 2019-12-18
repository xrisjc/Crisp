using Crisp.Ast;
using System;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    // FunctionLook does a pass of a program to track all functions within.
    //
    // A function offset is where the function's instructions begin.  A
    // function's index is where the function is in the list of functions.
    // Function calls are initially compiled by index because it isn't
    // known where the instructions will start.  After compilation a pass
    // will update all indexes with offsets.

    class FunctionLookup
    {
        List<int> offsets = new List<int>();
        Dictionary<Function, int> indices = new Dictionary<Function, int>();

        public List<Function> Functions { get; } = new List<Function>();

        public int Index(Function function) => indices[function];

        public int Offset(int index) => offsets[index];

        public void SetOffset(Function function, int offset)
        {
            offsets[Index(function)] = offset;
        }

        public FunctionLookup(Program program)
        {
            foreach (var e in program.Expressions)
                Add(e);
        }

        void Add(Function function)
        {
            if (!indices.TryGetValue(function, out int index))
            {
                Functions.Add(function);
                offsets.Add(-1);
                index = Functions.Count - 1;
                indices[function] = index;
            }
        }

        void Add(IExpression expr)
        {
            switch (expr)
            {
                case AssignmentIdentifier ai:
                    Add(ai.Value);
                    break;

                case AssignmentIndex ai:
                    Add(ai.Index);
                    Add(ai.Value);
                    break;

                case AssignmentRefinement ar:
                    Add(ar.Refinement);
                    Add(ar.Value);
                    break;

                case Block b:
                    foreach (var e in b.Body)
                        Add(e);
                    break;

                case Call c:
                    Add(c.Target);
                    foreach (var e in c.Arguments)
                        Add(e);
                    break;

                case Function f:
                    Add(f);
                    Add(f.Body);
                    break;

                case If i:
                    Add(i.Condition);
                    Add(i.Consequence);
                    Add(i.Alternative);
                    break;

                case Ast.Index i:
                    Add(i.Target);
                    Add(i.Key);
                    break;

                case LiteralObject lo:
                    foreach (var (_, e) in lo.Properties)
                        Add(e);
                    break;

                case OperatorBinary ob:
                    Add(ob.Left);
                    Add(ob.Right);
                    break;

                case OperatorUnary ou:
                    Add(ou.Expression);
                    break;

                case Refinement r:
                    Add(r.Target);
                    break;

                case Var v:
                    Add(v.InitialValue);
                    break;

                case While w:
                    Add(w.Guard);
                    Add(w.Body);
                    break;

                case Write w:
                    foreach (var e in w.Arguments)
                        Add(e);
                    break;
            }
        }
    }
}