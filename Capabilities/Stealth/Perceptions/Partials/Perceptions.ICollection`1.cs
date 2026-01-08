using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public partial class Perceptions : ICollection<Perception>
    {
        public int Count => ((ICollection<Perception>)Items).Count;

        public bool IsReadOnly => ((ICollection<Perception>)Items).IsReadOnly;

        public void Add(Perception item)
        {
            ((ICollection<Perception>)Items).Add(item);
        }

        public void Clear()
        {
            ((ICollection<Perception>)Items).Clear();
        }

        public bool Contains(Perception item)
        {
            return ((ICollection<Perception>)Items).Contains(item);
        }

        public void CopyTo(Perception[] array, int arrayIndex)
        {
            ((ICollection<Perception>)Items).CopyTo(array, arrayIndex);
        }

        public bool Remove(Perception item)
        {
            return ((ICollection<Perception>)Items).Remove(item);
        }
    }
}
