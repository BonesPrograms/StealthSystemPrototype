using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public abstract class SerializableSequence<T> : IComposite, IEnumerable<T>, IEnumerable<KeyValuePair<int, T>>, IReadOnlyList<T>//, IReadOnlyDictionary<int, T>
    {
        [Serializable]
        public struct Enumerator
            : IEnumerator<T>
            , IEnumerator
            , IDisposable
        {
            private readonly SerializableSequence<T> Diffuser;

            private int Step;
            private T LastValue;

            public readonly T Current => Diffuser.DiffuseStep(LastValue, Step);

            readonly object IEnumerator.Current => Current;

            public Enumerator(SerializableSequence<T> Diffuser)
            {
                this.Diffuser = Diffuser;
                LastValue = Diffuser.StartValue;
                Step = Diffuser.Steps.Start.Value;
            }

            public bool MoveNext()
                => ++Step < Diffuser.Steps.End.Value;

            public void Reset()
            {
                LastValue = Diffuser.StartValue;
                Step = -1;
            }

            public void Dispose()
            {
                Step = -1;
            }
        }

        private Range Steps;
        private T StartValue;

        public T CurrentValue;

        public int Count => Steps.Breadth();

        public T this[int index] => throw new NotImplementedException("This needs to be implemented");

        public SerializableSequence()
        {
            Steps = ..;
            StartValue = default;
        }
        public SerializableSequence(Range Steps, T StartValue)
            : this()
        {
            this.StartValue = StartValue;
            this.Steps = Steps;
        }
        public SerializableSequence(int Steps, T StartValue)
            : this(0..Steps, StartValue)
        {
        }

        public abstract T DiffuseStep(T LastValue, int Step);

        public virtual SerializableSequence<T> SetSteps(int Steps)
        {
            this.Steps = new(this.Steps.Start, this.Steps.Start.Value + Steps);
            return this;
        }
        public virtual SerializableSequence<T> SetStartValue(T StartValue)
        {
            this.StartValue = StartValue;
            return this;
        }

        /*
        public IEnumerable<T> GetStepValues()
        {
            T last;
            foreach (int step in (new int[11]).Select((o, i) => i))
        }
        */

        public IEnumerator<T> GetEnumerator()
            => new Enumerator(this);

        IEnumerator<KeyValuePair<int, T>> IEnumerable<KeyValuePair<int, T>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public static implicit operator Func<T, int, T>(SerializableSequence<T> Diffuser)
            => Diffuser.DiffuseStep;
    }
}
