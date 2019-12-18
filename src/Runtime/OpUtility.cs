namespace Crisp.Runtime
{
    static class OpUtility
    {
        public static int Count(this Op code)
        {
            switch (code)
            {
                case Op.Fn:
                    return 3;

                case Op.Const:
                case Op.Call:
                case Op.CallMthd:
                case Op.Jump:
                case Op.JumpTruthy:
                case Op.JumpFalsy:
                    return 2;

                default:
                    return 1;
            }
        }
    }
}