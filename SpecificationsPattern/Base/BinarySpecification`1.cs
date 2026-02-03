using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public abstract class BinarySpecification<T> : ISpecification<T>
    {
        public ISpecification<T> SpecX { get; set; }
        public ISpecification<T> SpecY { get; set; }

        protected T _Subject;
        public virtual T Subject
        {
            get => _Subject;
            set => _Subject = value;
        }

        public BinarySpecification()
        {
            SpecX = null;
            SpecY = null;
            Subject = default;
        }
        public BinarySpecification(ISpecification<T> SpecX, ISpecification<T> SpecY, T Subject)
            : this()
        {
            this.SpecX = SpecX;
            this.SpecY = SpecY;
            this.Subject = Subject;
        }
        public BinarySpecification(ISpecification<T> SpecX, ISpecification<T> SpecY)
            : this(SpecX, SpecY, default) { }

        public BinarySpecification(BinarySpecification<T> Source)
            : this(Source.SpecX, Source.SpecY) { }

        #region Seialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.Write(SpecX);
            Writer.Write(SpecY);
            Writer.WriteObject(Subject);
        }
        public virtual void Read(SerializationReader Reader)
        {
            SpecX = Reader.ReadComposite() as ISpecification<T>;
            SpecY = Reader.ReadComposite() as ISpecification<T>;
            Subject = Reader.ReadObject<T>();
        }

        #endregion

        public abstract bool Check(T Subject);

        public virtual bool Check()
            => Check(Subject);

        public virtual void Dispose()
        {
            SpecX = null;
            SpecY = null;
            Subject = default;
        }

        public static implicit operator bool(BinarySpecification<T> Operand)
            => Operand.Check();

        public static implicit operator Predicate<T>(BinarySpecification<T> Operand)
            => Operand.Check;

        public static implicit operator Func<T, bool>(BinarySpecification<T> Operand)
            => Operand.Check;

        public static explicit operator BinarySpecification(BinarySpecification<T> Operand)
            => (BinarySpecification)(Operand as ISpecification);
    }
}
