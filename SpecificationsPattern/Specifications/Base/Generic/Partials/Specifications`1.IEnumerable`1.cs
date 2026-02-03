using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    // [Serializable]
    public abstract partial class Specifications<T> : ISpecification<T>, IEnumerable<ISpecification<T>>
    {
        [Serializable]
        public struct Enumerator : IEnumerator<ISpecification<T>>, IEnumerator, IDisposable
        {
            private readonly Specifications<T> Specs;
            private readonly ISpecification<T>[] Items;
            private int Index;
            private int Variant;

            public readonly ISpecification<T> Current => Items[Index];
            readonly object IEnumerator.Current => Current;

            public Enumerator(Specifications<T> Specs)
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

        public virtual IEnumerator<ISpecification<T>> GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
