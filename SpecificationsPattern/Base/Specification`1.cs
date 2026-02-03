using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public abstract class Specification<T> : Specification, ISpecification<T>
    {
        protected T _Subject;
        public T Subject
        {
            get => _Subject;
            set => _Subject = value;
        }

        protected Specification()
        {
            _Subject = default;
        }
        public Specification(T Subject)
            : this()
        {
            _Subject = Subject;
        }

        #region Serialization

        public abstract void Write(SerializationWriter Writer);

        public abstract void Read(SerializationReader Reader);

        #endregion

        public abstract bool Check(T Subject);

        public override void Dispose()
        {
            base.Dispose();
            _Subject = default;
        }

        public static implicit operator bool(Specification<T> Operand)
            => Operand.Check();

        public static implicit operator Predicate<T>(Specification<T> Operand)
            => Operand.Check;
    }
}
