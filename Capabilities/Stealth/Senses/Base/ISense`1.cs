using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Perceptions;

using XRL.World;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public abstract class ISense<T> : ISense
        where T : ISense<T>, new()
    {
        public ISense(int Intensity)
            : base(Intensity)
        {
        }

        public override double GetIntensity()
            => base.GetIntensity();

        public virtual bool CanSense(IPerception<T> Perception, GameObject Entity)
            => base.CanSense(Perception, Entity);

        public virtual AwarenessLevel CalculateAwareness(IPerception<T> Perception, out int Roll, GameObject Entity)
            => base.CalculateAwareness(Perception, out Roll, Entity);

        public virtual AwarenessLevel Sense(IPerception<T> Perception, out int Roll, GameObject Entity)
            => !CanSense(Perception, Entity)
            ? NoAwareness(out Roll)
            : CalculateAwareness(Perception, out Roll, Entity);

        public override void Write(SerializationWriter Writer)
        {
        }
        public override void Read(SerializationReader Reader)
        {
        }
    }
}
