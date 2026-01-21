using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Perceptions;

using XRL.World;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public abstract class ISense : IComposite
    {
        public static string NAMESPACE => "StealthSystemPrototype.Senses";

        public virtual int Order => 0;

        protected string _Name;
        public virtual string Name => _Name ??= GetType()?.ToStringWithGenerics();

        [NonSerialized]
        protected int Intensity;

        protected ISense()
        {
            _Name = null;
            Intensity = 0;
        }

        public ISense(int Intensity)
            : this()
        {
            this.Intensity = Intensity;
        }

        public virtual double GetIntensity() => Intensity;

        public bool CanSense(IPerception Perception, GameObject Entity)
            => GetType() != Perception?.Sense
            && Entity != null;

        public static AwarenessLevel AwarenessFromRoll(int Roll)
            => (AwarenessLevel)((int)Math.Ceiling(((Roll + 1) / 20.0) - 1)).Clamp(0, 4);

        public AwarenessLevel CalculateAwareness(IPerception Perception, out int Roll, GameObject Entity)
            => AwarenessFromRoll(Roll = Perception?.Roll(Entity) ?? 0);

        public AwarenessLevel Sense(IPerception Perception, out int Roll, GameObject Entity)
        {
            if (!CanSense(Perception, Entity))
                return NoAwareness(out Roll);

            return CalculateAwareness(Perception, out Roll, Entity);
        }

        protected static AwarenessLevel NoAwareness(out int Roll)
            => (AwarenessLevel)(Roll = (int)AwarenessLevel.None);

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteOptimized(Intensity);
        }
        public virtual void Read(SerializationReader Reader)
        {
            Intensity = Reader.ReadOptimizedInt32();
        }
    }
}
