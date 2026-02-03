using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    // [Serializable]
    public partial class Specifications : Specification, IList<Specification>, IReadOnlyList<Specification>
    {
        public virtual Specification this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public virtual int IndexOf(Specification item)
            => Items.IndexOf(item);

        public virtual void Insert(int index, Specification item)
            => throw new NotImplementedException();

        public virtual void RemoveAt(int index)
            => throw new NotImplementedException();
    }
}
