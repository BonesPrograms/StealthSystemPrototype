using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    // [Serializable]
    public abstract partial class Specifications<T> : ISpecification<T>, IList<ISpecification<T>>, IReadOnlyList<ISpecification<T>>
    {
        public virtual ISpecification<T> this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public virtual int IndexOf(ISpecification<T> item)
            => Items.IndexOf(item);

        public virtual void Insert(int index, ISpecification<T> item)
            => throw new NotImplementedException();

        public virtual void RemoveAt(int index)
            => throw new NotImplementedException();
    }
}
