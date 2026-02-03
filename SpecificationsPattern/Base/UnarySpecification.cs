using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public abstract class UnarySpecification : ISpecification
    {
        public ISpecification Spec { get; set; }

        public UnarySpecification(ISpecification Spec)
        {
            this.Spec = Spec;
        }
        public UnarySpecification(UnarySpecification Source)
            : this(Source.Spec) { }

        #region Seialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.Write(Spec);
        }
        public virtual void Read(SerializationReader Reader)
        {
            Spec = Reader.ReadComposite() as ISpecification;
        }

        #endregion

        public abstract bool Check();

        public virtual void Dispose()
        {
            Spec = null;
        }

        public static implicit operator bool(UnarySpecification Operand)
            => Operand.Check();
    }
}
