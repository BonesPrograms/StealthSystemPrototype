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
        public virtual T Subject
        {
            get => _Subject;
            set => _Subject = value;
        }

        protected Specification()
            : base()
        {
            Subject = default;
        }
        public Specification(T Subject)
            : this()
        {
            this.Subject = Subject;
        }

        #region Serialization

        public abstract void Write(SerializationWriter Writer);

        public abstract void Read(SerializationReader Reader);

        #endregion

        public override bool Check()
            => Check(Subject);

        public abstract bool Check(T Subject);

        public override void Dispose()
        {
            Subject = default;
        }

        public static implicit operator bool(Specification<T> Operand)
            => Operand.Check();

        public static implicit operator Predicate<T>(Specification<T> Operand)
            => Operand.Check;

        public static implicit operator Func<T, bool>(Specification<T> Operand)
            => Operand.Check;
    }
}
