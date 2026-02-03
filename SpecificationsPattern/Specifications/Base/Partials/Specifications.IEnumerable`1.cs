using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    // [Serializable]
    public partial class Specifications : Specification, IEnumerable<Specification>
    {
        [Serializable]
        public struct Enumerator : IEnumerator<Specification>, IEnumerator, IDisposable
        {
            private readonly Specifications Specs;
            private readonly Specification[] Items;
            private int Index;
            private int Variant;

            public readonly Specification Current => Items[Index];
            readonly object IEnumerator.Current => Current;

            public Enumerator(Specifications Specs)
            {
                this.Specs = Specs;
                Items = Specs.Items;
                Index = -1;
                Variant = Specs.Variant;
            }

            public bool MoveNext()
                => Variant == Specs.Variant
                ? ++Index < Items.Length
                : throw new InvalidOperationException();

            public void Reset()
            {
                if (Variant != Specs.Variant)
                    throw new InvalidOperationException();

                Index = -1;
            }

            public void Dispose()
            {
                Index = default;
                Variant = default;
            }
        }

        public virtual IEnumerator<Specification> GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
