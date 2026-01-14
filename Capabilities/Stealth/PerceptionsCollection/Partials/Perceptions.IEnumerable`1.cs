using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using XRL.Collections;
using XRL.World;

using static StealthSystemPrototype.Utils;

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
            private Perceptions Perceptions;

            private BasePerception[] Items;

            private int Version;

            private int Index;

            public readonly BasePerception Current => Items[Index];
            readonly object IEnumerator.Current => Current;

            public Enumerator(Perceptions Perceptions)
            {
                this.Perceptions = Perceptions;
                Items = new BasePerception[Perceptions.Items.Length];
                Perceptions.CopyTo(Items, 0);
                Version = Perceptions.Version;
                Index = -1;
            }

            public readonly Enumerator GetEnumerator()
                => this;

            public bool MoveNext()
            {
                if (Version != Perceptions.Version)
                    throw new CollectionModifiedException(typeof(Perceptions));

                return ++Index < Perceptions.Count
                    && Current != null;
            }

            void IEnumerator.Reset()
            {
                if (Version != Perceptions.Version)
                    throw new CollectionModifiedException(typeof(Perceptions));

                Index = -1;
            }

            public void Dispose()
            {
                Perceptions = null;
                Array.Clear(Items, 0, Items.Length);
                Items = null;
                Index = default;
                Version = default;
            }
        }

        public IEnumerator<BasePerception> GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
