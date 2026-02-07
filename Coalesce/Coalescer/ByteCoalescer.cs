using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using StealthSystemPrototype.Coalescence;

namespace StealthSystemPrototype
{
    [Serializable]
    internal class ByteCoalescer : Coalescer<byte>
    {
        public ByteCoalescer()
            : base()
        { }
        public ByteCoalescer(CoalesceMethod Method)
            : base(Method)
        { }

        public override byte CoalesceCombine(byte X, byte Y)
            => (byte)(X ^ Y);

        public override byte CoalesceDifference(byte X, byte Y)
            => (byte)(X ^ ~Y);

        public override byte CoalesceGreater(byte X, byte Y)
            => Y > X
            ? Y
            : X;

        public override byte CoalesceLesser(byte X, byte Y)
            => Y < X
            ? Y
            : X;
    }
}
