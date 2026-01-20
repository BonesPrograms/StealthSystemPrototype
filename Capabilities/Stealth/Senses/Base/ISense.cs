using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public abstract class ISense : IComposite
    {
        public virtual int Order => 0;

        protected string _Name;
        public virtual string Name => _Name ??= GetType()?.ToStringWithGenerics();

        public ISense()
        {
            _Name = null;
        }

        public virtual double GetIntensity() => 1.0;

        public virtual void Write(SerializationWriter Writer)
        {
        }
        public virtual void Read(SerializationReader Reader)
        {
        }
    }
}
