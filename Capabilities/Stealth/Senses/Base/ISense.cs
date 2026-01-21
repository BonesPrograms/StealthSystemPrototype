using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

using StealthSystemPrototype.Perceptions;
using static StealthSystemPrototype.Utils;
using XRL;
using StealthSystemPrototype.Alerts;

namespace StealthSystemPrototype.Senses
{
    [HasModSensitiveStaticCache]
    [Serializable]
    public abstract class ISense : IComposite
    {
        [ModSensitiveStaticCache]
        private static SortedDictionary<string, ISense> _SortedSenses;
        public static SortedDictionary<string, ISense> SortedSenses => _SortedSenses ??= GetSenses();

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
        public ISense(ISense Source)
            : this(Source.Intensity)
        {
        }

        public virtual double GetIntensity() => Intensity;

        public virtual bool CanSense(IPerception Perception, GameObject Entity)
            => GetType() != Perception?.Sense
            && Entity != null;

        public static AwarenessLevel AwarenessFromRoll(int Roll)
            => (AwarenessLevel)((int)Math.Ceiling(((Roll + 1) / 20.0) - 1)).Clamp(0, 4);

        public virtual AwarenessLevel CalculateAwareness<T>(SenseContext<T> Context)
            where T : ISense<T>, new()
            => AwarenessFromRoll(Context.Roll);

        public abstract AwarenessLevel Sense<T>(SenseContext<T> Context)
            where T : ISense<T>, new();

        /*
        public virtual AwarenessLevel Sense(SenseContext Context)
        {
            if (!CanSense(Context.Perception, Context.Entity))
                return NoAwareness(out _);

            return CalculateAwareness(Context);
        }*/

        public virtual bool TrySense<T>(SenseContext Context)
        {
            switch (Sense(Context))
            {
                case AwarenessLevel.Alert:
                    Context.Owner?.Brain.PushGoal(new Investigate<T>(Context, this, AwarenessLevel.Alert));
                    return true;

                case AwarenessLevel.Aware:
                    Context.Owner?.Brain.PushGoal(new Investigate<T>(Context, this, AwarenessLevel.Aware));
                    return true;

                case AwarenessLevel.Suspect:
                    Context.Owner?.Brain.PushGoal(new Investigate<T>(Context, this, AwarenessLevel.Suspect));
                    return true;

                case AwarenessLevel.Awake:
                    Context.Owner?.Brain.PushGoal(new Investigate<T>(Context, this, AwarenessLevel.Awake));
                    return true;

                case AwarenessLevel.None:
                default:
                    return false;
            }
        }

        protected abstract ISense Copy();

        public static ISense Copy(ISense Sense)
            => Sense.Copy();

        public static ISense SampleSense(string SenseName)
            => !SortedSenses.IsNullOrEmpty()
            && SortedSenses.ContainsKey(SenseName)
            && SortedSenses[SenseName] is ISense sense
            ? Copy(sense)
            : null;

        public static TSense SampleSense<TSense>()
            where TSense : ISense<TSense>, new()
            => !SortedSenses.IsNullOrEmpty()
            && SortedSenses.ContainsKey(typeof(TSense).Name)
            && SortedSenses[typeof(TSense).Name] is TSense sense
            ? Copy(sense) as TSense
            : null;

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
