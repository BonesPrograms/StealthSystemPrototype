using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype.Logging;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public abstract class SerializableSequence<T>
        : IComposite
        , IEnumerable<T>
        , IReadOnlyList<T>
        , IReadOnlyDictionary<int, T>
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
                SerializableSequence.EnumeratorConstructorValues(
                    SerializableSequence: out this.SerializableSequence,
                    Step: out Step,
                    LastValue: out LastValue);
            }

            public bool MoveNext()
            {
                LastValue = Current;
                if (SerializableSequence.Direction == 0)
                    return false;
                Step += SerializableSequence.Direction;
                return SerializableSequence.Steps.Contains(Step);
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
                SerializableSequence.EnumeratorConstructorValues(
                    SerializableSequence: out this.SerializableSequence,
                    Step: out Step,
                    LastValue: out LastValue);
            }

            public bool MoveNext()
            {
                LastValue = Current.Value;
                if (SerializableSequence.Direction == 0)
                    return false;
                Step += SerializableSequence.Direction;
                return SerializableSequence.Steps.Contains(Step);
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

        public int Count => Steps.AbsLength;
        public int Direction => Steps.Direction;
        public bool IsForward => Steps.IsForward;
        public bool IsBackwards => Steps.IsBackwards;

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

        private static string LastIndent(int Offset = 0)
        {
            using Indent indent = new(Offset);
            return indent.ToString();
        }
        private static string IndentOrComma(bool Comma, int Offset = 0)
            => Comma 
            ? ","
            : LastIndent(Offset);

        public virtual string ToString(bool Short, bool Inline, string FormatString = "{0:0.00}")
            => GetName(Short) +
            "()" + (Inline ? null : "\n" + LastIndent()) + "{" + (Inline ? " " : "\n" + LastIndent(1)) +
            GetStepValues()
                ?.Aggregate(
                    seed: "",
                    func: (a, n) => a + (!a.IsNullOrEmpty() ? IndentOrComma(Inline, 1) + (Inline ? " " : "\n") : null) + String.Format(FormatString, n)) +
            (Inline ? " " : "\n" + LastIndent()) + "}";

        public virtual string ToString(bool Short, string FormatString = "{0:0.00}")
            => ToString(Short, true, FormatString);

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

        public void EnumeratorConstructorValues(
            out SerializableSequence<T> SerializableSequence,
            out int Step,
            out T LastValue)
        {
            SerializableSequence = this;
            Step = Steps.Start - Direction;
            LastValue = StartValue;
        }

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
