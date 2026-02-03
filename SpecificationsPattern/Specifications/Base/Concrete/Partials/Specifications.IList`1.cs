using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    // [Serializable]
    public abstract partial class Specifications : ISpecification, IList<ISpecification>, IReadOnlyList<ISpecification>
    {
        public virtual ISpecification this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public virtual int IndexOf(ISpecification item)
            => Items.IndexOf(item);

        public virtual void Insert(int index, ISpecification item)
            => throw new NotImplementedException();

        public virtual void RemoveAt(int index)
            => throw new NotImplementedException();
    }
}
