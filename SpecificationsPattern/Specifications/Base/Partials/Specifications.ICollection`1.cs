using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    // [Serializable]
    public partial class Specifications : Specification, ICollection<Specification>, IReadOnlyCollection<Specification>
    {
        public int Count => Length;

        public virtual bool IsReadOnly => false;

        public virtual void Add(Specification Item)
            => throw new NotImplementedException();

        public virtual void Clear()
            => throw new NotImplementedException();

        public virtual bool Contains(Specification Item)
            => IndexOf(Item) >= 0;

        public void CopyTo(Specification[] Array, int ArrayIndex)
            => System.Array.Copy(Items, 0, Array, ArrayIndex, Length);

        public virtual bool Remove(Specification Item)
            => throw new NotImplementedException();
    }
}
