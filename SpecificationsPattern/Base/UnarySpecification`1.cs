using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public abstract class UnarySpecification<T> : ISpecification<T>
    {
        public ISpecification<T> Spec { get; set; }

        protected T _Subject;
        public virtual T Subject
        {
            get => _Subject;
            set => _Subject = value;
        }

        public UnarySpecification()
        {
            Spec = null;
            Subject = default;
        }
        public UnarySpecification(ISpecification<T> Spec, T Subject)
            : this()
        {
            this.Spec = Spec;
            this.Subject = Subject;
        }
        public UnarySpecification(ISpecification<T> Spec)
            : this(Spec, default) { }

        public UnarySpecification(UnarySpecification<T> Source)
            : this(Source.Spec, Source.Subject) { }

        #region Seialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.Write(Spec);
            Writer.WriteObject(Subject);
        }
        public virtual void Read(SerializationReader Reader)
        {
            Spec = Reader.ReadComposite() as ISpecification<T>;
            Subject = Reader.ReadObject<T>();
        }

        #endregion

        public abstract bool Check(T Subject);

        public virtual bool Check()
            => Check(Subject);

        public virtual void Dispose()
        {
            Spec = null;
            Subject = default;
        }

        public static implicit operator bool(UnarySpecification<T> Operand)
            => Operand.Check();

        public static implicit operator Predicate<T>(UnarySpecification<T> Operand)
            => Operand.Check;

        public static implicit operator Func<T, bool>(UnarySpecification<T> Operand)
            => Operand.Check;

        public static explicit operator UnarySpecification(UnarySpecification<T> Operand)
            => (UnarySpecification)(Operand as ISpecification);
    }
}
