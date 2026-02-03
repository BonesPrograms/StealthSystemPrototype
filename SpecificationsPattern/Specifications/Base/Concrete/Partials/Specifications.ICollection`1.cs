using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    // [Serializable]
    public abstract partial class Specifications : ISpecification, ICollection<ISpecification>, IReadOnlyCollection<ISpecification>
    {
        public int Count => Length;

        public virtual bool IsReadOnly => false;

        public virtual void Add(ISpecification Item)
            => throw new NotImplementedException();

        public virtual void Clear()
            => throw new NotImplementedException();

        public virtual bool Contains(ISpecification Item)
            => IndexOf(Item) >= 0;

        public void CopyTo(ISpecification[] Array, int ArrayIndex)
            => System.Array.Copy(Items, 0, Array, ArrayIndex, Length);

        public virtual bool Remove(ISpecification Item)
            => throw new NotImplementedException();
    }
}
