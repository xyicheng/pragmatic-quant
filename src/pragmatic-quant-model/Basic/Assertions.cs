using System;

namespace pragmatic_quant_model.Basic
{
    public static class Assertions
    {
        public static void ArraySize<T>(string errMsgSource, T[] a, int size)
        {
            if (a.Length != size)
                throw new Exception(errMsgSource + " : unexpected array size  !");
        }
    }
}
