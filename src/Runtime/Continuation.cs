using Crisp.Ast;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crisp.Runtime
{
    interface IContinuation
    {
    }

    record ContinuationDone : IContinuation;

    record ContinuationAssignment(
        ImmutableList<Cell> Environment,
        IContinuation Continuation,
        int Depth)
        : IContinuation;

    record ContinuationConditional(
        IExpression Consequence,
        IExpression Alternative,
        ImmutableList<Cell> Environment,
        IContinuation Continuation)
        : IContinuation;

    record ContinuationEval(
        IExpression Expression,
        ImmutableList<Cell> Environment,
        IContinuation Continuation)
        : IContinuation;

    record ContinuationFnArg(
        IExpression Argument,
        ImmutableList<Cell> Environment,
        IContinuation Continuation)
        : IContinuation;
}
