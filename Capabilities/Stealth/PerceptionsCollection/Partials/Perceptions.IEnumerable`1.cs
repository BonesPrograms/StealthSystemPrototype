using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public partial class Perceptions
        : IEnumerable<BasePerception>
        , IEnumerable
    {
        [Serializable]
        public struct Enumerator
            : IEnumerator<BasePerception>
            , IEnumerator
            , IDisposable
        {
            private readonly Perceptions Perceptions;

            private readonly BasePerception[] Items;

            private readonly int Version;

            private int Index;

            public readonly BasePerception Current => Items[Index];
            readonly object IEnumerator.Current => Current;

            public Enumerator(Perceptions Perceptions)
            {
                this.Perceptions = Perceptions;
                Items = Perceptions.Items;
                Version = Perceptions.Version;
                Index = -1;
            }

            public readonly Enumerator GetEnumerator()
                => this;

            public readonly bool MoveNext()
            {
                if (Version != Perceptions.Version)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

                if (Index < Items.Length)
                    return false;

                return false;
            }

            void IEnumerator.Reset()
            {
                if (Version != Perceptions.Version)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

                Index = -1;
            }

            public void Dispose()
            {
                Index = -1;
            }
        }

        public IEnumerator<BasePerception> GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
