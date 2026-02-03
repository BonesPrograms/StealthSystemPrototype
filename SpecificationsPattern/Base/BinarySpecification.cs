using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public abstract class BinarySpecification : ISpecification
    {
        public ISpecification SpecX { get; set; }
        public ISpecification SpecY { get; set; }

        public BinarySpecification()
        {
            SpecX = null;
            SpecY = null;
        }
        public BinarySpecification(ISpecification SpecX, ISpecification SpecY)
            : this()
        {
            this.SpecX = SpecX;
            this.SpecY = SpecY;
        }
        public BinarySpecification(BinarySpecification Source)
            : this(Source.SpecX, Source.SpecY) { }

        #region Seialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.Write(SpecX);
            Writer.Write(SpecY);
        }
        public virtual void Read(SerializationReader Reader)
        {
            SpecX = Reader.ReadComposite() as ISpecification;
            SpecY = Reader.ReadComposite() as ISpecification;
        }

        #endregion

        public abstract bool Check();

        public virtual void Dispose()
        {
            SpecX = null;
            SpecY = null;
        }

        public static implicit operator bool(BinarySpecification Operand)
            => Operand.Check();
    }
}
