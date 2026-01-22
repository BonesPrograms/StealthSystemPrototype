using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

using StealthSystemPrototype.Perceptions;
using static StealthSystemPrototype.Utils;
using XRL;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Events;

namespace StealthSystemPrototype.Senses
{
    [HasModSensitiveStaticCache]
    [Serializable]
    public abstract class ISense : IComposite
    {
        [ModSensitiveStaticCache]
        private static SortedDictionary<string, ISense> _SortedSenses;
        public static SortedDictionary<string, ISense> SortedSenses => _SortedSenses ??= GetSenses();

        public static string NAMESPACE => typeof(ISense).Namespace;

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

        public virtual int GetIntensity()
            => Intensity;

        public virtual int SetIntensity(int Intensity)
            => this.Intensity = Math.Max(0, Intensity);

        public virtual int AdjustIntensity(int Intensity)
            => SetIntensity(this.Intensity + Intensity);

        public virtual bool CanSense(IPerception Perception, GameObject Entity)
            => GetType() != Perception?.Sense
            && Entity != null;

        public static AwarenessLevel AwarenessFromRoll(int Roll)
            => (AwarenessLevel)((int)Math.Ceiling(((Roll + 1) / 20.0) - 1)).Clamp(0, 4);

        public abstract AwarenessLevel CalculateAwareness<T>(SenseContext<T> Context)
            where T : ISense<T>, new();

        public abstract AwarenessLevel Sense<T>(SenseContext<T> Context)
            where T : ISense<T>, new();

        protected abstract ISense Copy();

        public static ISense Copy(ISense Sense)
            => Sense.Copy();

        public static ISense SampleSense(string SenseName)
            => !SortedSenses.IsNullOrEmpty()
            && SortedSenses.ContainsKey(SenseName)
            && SortedSenses[SenseName] is ISense sense
            ? Copy(sense)
            : null;

        public static TSense SampleSense<TSense>(Type SenseType)
            where TSense : ISense<TSense>, new()
            => SenseType == typeof(TSense)
            && !SortedSenses.IsNullOrEmpty()
            && SortedSenses.ContainsKey(SenseType.ToStringWithGenerics())
            && SortedSenses[SenseType.ToStringWithGenerics()] is TSense sense
            ? Copy(sense) as TSense
            : null;

        public static TSense SampleSense<TSense>()
            where TSense : ISense<TSense>, new()
            => SampleSense<TSense>(typeof(TSense));

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
