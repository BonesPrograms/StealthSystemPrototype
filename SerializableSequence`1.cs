using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public abstract class SerializableSequence<T> : IComposite, IEnumerable<T>, IReadOnlyList<T>, IReadOnlyDictionary<int, T>
    {
        #region Enumerators

        [Serializable]
        public struct Enumerator
            : IEnumerator<T>
            , IEnumerator
            , IDisposable
        {
            private SerializableSequence<T> SerializableSequence;
            private int Step;
            private T LastValue;

            public readonly T Current => SerializableSequence.Step(LastValue, Step);

            readonly object IEnumerator.Current => Current;

            public Enumerator(SerializableSequence<T> SerializableSequence)
            {
                this.SerializableSequence = SerializableSequence;
                Step = SerializableSequence.Steps.Start - 1;
                LastValue = SerializableSequence.StartValue;
            }

            public bool MoveNext()
            {
                LastValue = Current;
                return ++Step <= SerializableSequence.Steps.Length;
            }

            public void Reset()
            {
                LastValue = SerializableSequence.StartValue;
                Step = SerializableSequence.Steps.Start - 1;
            }

            public void Dispose()
            {
                SerializableSequence = null;
                Step = default;
                LastValue = default;
            }
        }

        [Serializable]
        public struct PairEnumerator
            : IEnumerator<KeyValuePair<int, T>>
            , IEnumerator
            , IDisposable
        {
            private SerializableSequence<T> SerializableSequence;
            private int Step;
            private T LastValue;

            public readonly KeyValuePair<int, T> Current => new(Step, SerializableSequence.Step(LastValue, Step));

            readonly object IEnumerator.Current => Current;

            public PairEnumerator(SerializableSequence<T> SerializableSequence)
            {
                this.SerializableSequence = SerializableSequence;
                Step = SerializableSequence.Steps.Start - 1;
                LastValue = SerializableSequence.StartValue;
            }

            public bool MoveNext()
            {
                LastValue = Current.Value;
                return ++Step <= SerializableSequence.Steps.Length;
            }

            public void Reset()
            {
                LastValue = SerializableSequence.StartValue;
                Step = SerializableSequence.Steps.Start - 1;
            }

            public void Dispose()
            {
                SerializableSequence = null;
                Step = default;
                LastValue = default;
            }
        }

        #endregion
        #region Fields & Properties

        public string Name => GetName();
        public string ShortName => GetName(true);

        protected InclusiveRange Steps;
        protected T StartValue;

        public int Count => Steps.Breadth();

        #endregion
        #region Constructors

        public SerializableSequence()
        {
            Steps = ..;
            StartValue = default;
        }
        public SerializableSequence(InclusiveRange Steps, T StartValue)
            : this()
        {
            this.StartValue = StartValue;
            this.Steps = Steps;
        }
        public SerializableSequence(int Steps, T StartValue)
            : this(0..Steps, StartValue)
        {
        }
        public SerializableSequence(SerializableSequence<T> Source)
            : this(Source.Steps, Source.StartValue)
        {
        }

        #endregion

        public virtual string GetName(bool Short = false)
            => GetType()?.ToStringWithGenerics(Short) ?? (Short ? "?" : "null?");

        public virtual string ToString(bool Short)
            => GetName(Short) +
            "(){" +
            GetStepValues().Aggregate("", (a, n) => a + (!a.IsNullOrEmpty() ? "," : null) + String.Format("{0:0.000}", n)) +
            "}";

        public override string ToString()
            => ToString(false);

        public T this[int Index]
            => (GetStepValues()?.ToArray() ?? new T[0])[Index];

        public T this[Index Index]
            => (GetStepValues()?.ToArray() ?? new T[0])[Index];

        public T[] this[Range Range]
            => (GetStepValues()?.ToArray() ?? new T[0])[Range];

        public abstract T Step(T LastValue, int Step);

        public virtual SerializableSequence<T> SetSteps(InclusiveRange Steps)
        {
            this.Steps = Steps;
            return this;
        }
        public virtual SerializableSequence<T> SetSteps(int Steps)
            => SetSteps(new InclusiveRange(this.Steps.Start, this.Steps.Start + Steps));

        public virtual SerializableSequence<T> SetStartValue(T StartValue)
        {
            this.StartValue = StartValue;
            return this;
        }

        #region IReadOnlyDicitonary<int, T>

        public bool ContainsKey(int Key)
            => Steps.Contains(Key);

        public bool TryGetValue(int Key, out T Value)
        {
            Value = default;
            if (ContainsKey(Key))
            {
                Value = this[Key];
                return true;
            }
            return false;
        }

        #endregion

        public IEnumerable<T> GetStepValues()
            => this;

        public IEnumerable<KeyValuePair<int, T>> GetPairs()
        {
            T lastValue = StartValue;
            foreach (int value in Steps)
                yield return new(value, Step(lastValue, value));
        }

        public IEnumerator<T> GetEnumerator()
            => new Enumerator(this);

        public IEnumerator<KeyValuePair<int, T>> GetPairEnumerator()
            => new PairEnumerator(this);

        #region Explicit Implementations

        IEnumerable<int> IReadOnlyDictionary<int, T>.Keys => Steps.GetValues();

        IEnumerable<T> IReadOnlyDictionary<int, T>.Values => GetStepValues();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        IEnumerator<KeyValuePair<int, T>> IEnumerable<KeyValuePair<int, T>>.GetEnumerator()
            => GetPairEnumerator();

        #endregion
        #region Serialization

        public abstract void WriteStartValue(SerializationWriter Writer, T StartValue);
        public abstract void ReadStartValue(SerializationReader Reader, out T StartValue);

        public virtual void Write(SerializationWriter Writer)
        {
            WriteStartValue(Writer, StartValue);
            Steps.WriteOptimized(Writer);
        }
        public virtual void Read(SerializationReader Reader)
        {
            ReadStartValue(Reader, out StartValue);
            Steps = InclusiveRange.ReadOptimizedInclusiveRange(Reader);
        }

        #endregion
        #region Conversion

        public static implicit operator Func<T, int, T>(SerializableSequence<T> SerializableSequence)
            => SerializableSequence.Step;

        #endregion
    }
}
