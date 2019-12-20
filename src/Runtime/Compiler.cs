using Crisp.Ast;

namespace Crisp.Runtime
{
    class Compiler
    {
        public Library System { get; } = new Library();

        Labels labels = new Labels();
        Resolver resolver;
        FunctionLookup functions;

        public Chunk Chunk { get; } = new Chunk();

        public void Compile(Program program)
        {
            resolver = new Resolver(program);
            functions = new FunctionLookup(program);
            foreach (var expr in program.Expressions)
            {
                Compile(expr);
                Chunk.Emit(Op.Discard);
            }
            Chunk.Emit(Op.Halt);
            foreach (var fn in functions.Functions)
            {
                functions.SetOffset(fn, Chunk.NextOffset);
                Compile(fn);
            }

            // Ajust jumps and function creation to where they actually are in
            // the chunk.
            int i = 0;
            while (i < Chunk.Code.Count)
            {
                var code = (Op)Chunk.Code[i];
                switch (code)
                {
                    case Op.Fn:
                        Chunk.Code[i + 1] = functions.Offset(Chunk.Code[i + 1]);
                        break;
                    case Op.Jump:
                    case Op.JumpFalsy:
                    case Op.JumpTruthy:
                        Chunk.Code[i + 1] = labels.Offset(Chunk.Code[i + 1]);
                        break;
                }
                i += code.Count();
            }
        }

        void Compile(Function fn)
        {
            var lStartDiscard = labels.New();
            var lEndDiscard = labels.New();

            // Test if we have arguments to discard if A > P.
            labels.Set(lStartDiscard, Chunk.NextOffset);
            Chunk.Emit(Op.Dup);
            Chunk.Emit(Op.Const, System.Create(fn.Parameters.Count));
            Chunk.Emit(Op.LtEq);
            Chunk.Emit(Op.JumpTruthy, lEndDiscard);

            // Discard an argument and test again.
            Chunk.Emit(Op.Switch);
            Chunk.Emit(Op.Discard);
            Chunk.Emit(Op.Const, System.Create(1));
            Chunk.Emit(Op.Sub);
            Chunk.Emit(Op.Jump, lStartDiscard);

            labels.Set(lEndDiscard, Chunk.NextOffset);

            if (fn.Parameters.Count > 0)
            {
                // A <= P.  If A < P, then we need to make some fake null
                // arguments so A = P.

                var lStartFake = labels.New();
                var lEndFake = labels.New();

                labels.Set(lStartFake, Chunk.NextOffset);
                Chunk.Emit(Op.Dup);
                Chunk.Emit(Op.Const, System.Create(fn.Parameters.Count));
                Chunk.Emit(Op.GtEq);
                Chunk.Emit(Op.JumpTruthy, lEndFake);

                // We don't have enough parameters, so push a null and loop.
                Chunk.Emit(Op.Null);
                Chunk.Emit(Op.Switch);
                Chunk.Emit(Op.Const, System.Create(1));
                Chunk.Emit(Op.Add);
                Chunk.Emit(Op.Jump, lStartFake);

                labels.Set(lEndFake, Chunk.NextOffset);

                // At this point A = P, so bind parameters.
                Chunk.Emit(Op.Discard); // Discard A
                for (int i = fn.Parameters.Count - 1; i >= 0; i--)
                {
                    Chunk.Emit(Op.Const, System.Create(fn.Parameters[i]));
                    Chunk.Emit(Op.Switch);
                    Chunk.Emit(Op.CreateVar);
                }
            }

            // Compile the body and return.
            Compile(fn.Body);
            Chunk.Emit(Op.Return);
        }

        void Compile(IExpression expr)
        {
            switch (expr)
            {
                case AssignmentIdentifier ai:
                    Chunk.Emit(Op.Const, System.Create(ai.Target.Name));
                    Compile(ai.Value);
                    Chunk.Emit(Op.SetVar);
                    break;

                case AssignmentIndex ai:
                    Compile(ai.Index.Target);
                    Compile(ai.Index.Key);
                    Compile(ai.Value);
                    Chunk.Emit(Op.SetProp);
                    break;

                case AssignmentRefinement ar:
                    Compile(ar.Refinement.Target);
                    Chunk.Emit(Op.Const, System.Create(ar.Refinement.Name));
                    Compile(ar.Value);
                    Chunk.Emit(Op.SetProp);
                    break;

                case Block b:
                    Chunk.Emit(Op.StartBlock);
                    if (b.Body.Count > 0)
                        for (int i = 0; i < b.Body.Count; i++)
                        {
                            Compile(b.Body[i]);
                            if (i < b.Body.Count - 1)
                                Chunk.Emit(Op.Discard);
                        }
                    else
                        Chunk.Emit(Op.Null);
                    Chunk.Emit(Op.EndBlock);
                    break;

                case Call c:

                    // Call stack is:
                    // A_1, .., A_n, n, [self,] fn

                    // A_1, .., A_n
                    foreach (var e in c.Arguments)
                        Compile(e);

                    // n
                    Chunk.Emit(Op.Const, System.Create(c.Arguments.Count));

                    switch (c.Target)
                    {
                        case Ast.Index index:
                            // self
                            Compile(index.Target);
                            // fn
                            Chunk.Emit(Op.Dup);
                            Compile(index.Key);
                            Chunk.Emit(Op.GetProp);
                            
                            Chunk.Emit(Op.CallMthd);
                            break;

                        case Refinement rfnt:
                            // self
                            Compile(rfnt.Target);
                            // fn
                            Chunk.Emit(Op.Dup);
                            Chunk.Emit(Op.Const, System.Create(rfnt.Name));
                            Chunk.Emit(Op.GetProp);
                            
                            Chunk.Emit(Op.CallMthd);
                            break;

                        default:
                            // fn
                            Compile(c.Target);

                            Chunk.Emit(Op.Call);
                            break;
                    }

                    break;

                case Function f:
                    Chunk.Emit(Op.Fn, functions.Index(f));
                    break;

                case Identifier i:
                    Chunk.Emit(Op.Const, System.Create(i.Name));
                    Chunk.Emit(Op.GetVar);
                    break;

                case If i:
                    {
                        var lAlt = labels.New();
                        var lEnd = labels.New();
                        Compile(i.Condition);
                        Chunk.Emit(Op.JumpFalsy, lAlt);
                        Chunk.Emit(Op.StartBlock);
                        Compile(i.Consequence);
                        Chunk.Emit(Op.EndBlock);
                        Chunk.Emit(Op.Jump, lEnd);
                        labels.Set(lAlt, Chunk.NextOffset);
                        Chunk.Emit(Op.StartBlock);
                        Compile(i.Alternative);
                        Chunk.Emit(Op.EndBlock);
                        labels.Set(lEnd, Chunk.NextOffset);
                    }
                    break;

                case Ast.Index i:
                    Compile(i.Target);
                    Compile(i.Key);
                    Chunk.Emit(Op.GetProp);
                    break;

                case LiteralBool lb:
                    Chunk.Emit(lb.Value ? Op.True : Op.False);
                    break;

                case LiteralNull _:
                    Chunk.Emit(Op.Null);
                    break;

                case LiteralObject lo:
                    Chunk.Emit(Op.New);
                    foreach (var (n, e) in lo.Properties)
                    {
                        Chunk.Emit(Op.Dup);
                        Chunk.Emit(Op.Const, System.Create(n));
                        Compile(e);
                        Chunk.Emit(Op.SetProp);
                        Chunk.Emit(Op.Discard);
                    }
                    break;

                case LiteralNumber ln:
                    Chunk.Emit(Op.Const, System.Create(ln.Value));
                    break;

                case LiteralString ls:
                    Chunk.Emit(Op.Const, System.Create(ls.Value));
                    break;

                case OperatorBinary ob when ob.Tag == OperatorBinaryTag.And:
                    {
                        var lShortCiruit = labels.New();
                        Compile(ob.Left);
                        Chunk.Emit(Op.Truthy);
                        Chunk.Emit(Op.Dup);
                        Chunk.Emit(Op.JumpFalsy, lShortCiruit);
                        Chunk.Emit(Op.Discard);
                        Compile(ob.Right);
                        Chunk.Emit(Op.Truthy);
                        labels.Set(lShortCiruit, Chunk.NextOffset);
                    }
                    break;

                case OperatorBinary ob when ob.Tag == OperatorBinaryTag.Or:
                    {
                        var lShortCiruit = labels.New();
                        Compile(ob.Left);
                        Chunk.Emit(Op.Truthy);
                        Chunk.Emit(Op.Dup);
                        Chunk.Emit(Op.JumpTruthy, lShortCiruit);
                        Chunk.Emit(Op.Discard);
                        Compile(ob.Right);
                        Chunk.Emit(Op.Truthy);
                        labels.Set(lShortCiruit, Chunk.NextOffset);
                    }
                    break;

                case OperatorBinary ob:
                    Compile(ob.Left);
                    Compile(ob.Right);
                    switch (ob.Tag)
                    {
                        case OperatorBinaryTag.Add:
                            Chunk.Emit(Op.Add);
                            break;
                        case OperatorBinaryTag.Sub:
                            Chunk.Emit(Op.Sub);
                            break;
                        case OperatorBinaryTag.Mul:
                            Chunk.Emit(Op.Mul);
                            break;
                        case OperatorBinaryTag.Div:
                            Chunk.Emit(Op.Div);
                            break;
                        case OperatorBinaryTag.Mod:
                            Chunk.Emit(Op.Mod);
                            break;
                        case OperatorBinaryTag.Lt:
                            Chunk.Emit(Op.Lt);
                            break;
                        case OperatorBinaryTag.LtEq:
                            Chunk.Emit(Op.LtEq);
                            break;
                        case OperatorBinaryTag.Gt:
                            Chunk.Emit(Op.Gt);
                            break;
                        case OperatorBinaryTag.GtEq:
                            Chunk.Emit(Op.GtEq);
                            break;
                        case OperatorBinaryTag.Eq:
                            Chunk.Emit(Op.Eq);
                            break;
                        case OperatorBinaryTag.Neq:
                            Chunk.Emit(Op.NotEq);
                            break;
                    }
                    break;

                case OperatorUnary ou:
                    Compile(ou.Expression);
                    switch (ou.Op)
                    {
                        case OperatorUnaryTag.Beget:
                            Chunk.Emit(Op.Beget);
                            break;
                        case OperatorUnaryTag.Neg:
                            Chunk.Emit(Op.Neg);
                            break;
                        case OperatorUnaryTag.Not:
                            Chunk.Emit(Op.Not);
                            break;
                    }
                    break;

                case Refinement r:
                    Compile(r.Target);
                    Chunk.Emit(Op.Const, System.Create(r.Name));
                    Chunk.Emit(Op.GetProp);
                    break;

                case Self _:
                    Chunk.Emit(Op.Self);
                    break;

                case Var v:
                    Chunk.Emit(Op.Const, System.Create(v.Name));
                    Compile(v.InitialValue);
                    Chunk.Emit(Op.CreateVar);
                    break;

                case While w:
                    {
                        var lStart = labels.New();
                        var lEnd = labels.New();
                        labels.Set(lStart, Chunk.NextOffset);
                        Compile(w.Guard);
                        Chunk.Emit(Op.JumpFalsy, lEnd);
                        Chunk.Emit(Op.StartBlock);
                        Compile(w.Body);
                        Chunk.Emit(Op.EndBlock);
                        Chunk.Emit(Op.Discard);
                        Chunk.Emit(Op.Jump, lStart);
                        labels.Set(lEnd, Chunk.NextOffset);
                        Chunk.Emit(Op.Null);
                    }
                    break;

                case Write w:
                    foreach (var e in w.Arguments)
                    {
                        Compile(e);
                        Chunk.Emit(Op.Write);
                    }
                    Chunk.Emit(Op.Null);
                    break;
            }
        }
    }
}