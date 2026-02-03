using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public abstract class Specifications<T> : Specifications, ISpecification<T>
    {
        protected T _Subject;
        public T Subject
        {
            get => _Subject;
            set => _Subject = value;
        }

        public abstract bool Check(T Subject);

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            Writer.WriteObject(Subject);
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            Subject = Reader.ReadObject<T>();
        }

        public static implicit operator bool(Specifications<T> Operand)
            => Operand.Check();

        public static implicit operator Predicate<T>(Specifications<T> Operand)
            => Operand.Check;
    }
}
