using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using XRL;
using XRL.World.Parts.Skill;
using XRL.World.Parts.Mutation;
using XRL.Collections;
using XRL.World;

using SerializeField = UnityEngine.SerializeField;

using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Senses;
using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Capabilities.Stealth.Sneak
{
    [HasModSensitiveStaticCache]
    [HasGameBasedStaticCache]
    [Serializable]
    public class SneakPerformance : IComposite
    {
        #region Const & Static

        [ModSensitiveStaticCache]
        [GameBasedStaticCache(CreateInstance = false)]
        private static SortedDictionary<string, ISense> _PerceptionSenses;
        public static SortedDictionary<string, ISense> PerceptionSenses => _PerceptionSenses ??= GetSenses();

        public static Dictionary<string, Entry> DefaultSneakPerformances => PerceptionSenses
            ?.Aggregate(
                seed: new Dictionary<string, Entry>(),
                func: delegate (Dictionary<string, Entry> Accumulator, KeyValuePair<string, ISense> Next)
                {
                    Accumulator[Next.Key] = new Entry(Next.Value);
                    return Accumulator;
                })
            ?? new();

        public static string MS_MULTI => "MoveSpeed_Multiplier";
        public static string QN_MULTI => "Quickness_Multiplier";

        #endregion
        #region Helpers

        [Serializable]
        public class Entry : IComposite, IEquatable<Entry>, IComparable<Entry>
        {
            public static InclusiveRange DefaultClamp => new(..100);

            #region Instance Fields & Properties

            private ISense _Sense;
            public virtual ISense Sense
            {
                get => _Sense;
                protected set => _Sense = value;
            }

            private int _Rating;

            public int Rating
            {
                get => _Rating.Clamp(Clamp);
                set => _Rating = value.Clamp(Clamp);
            }

            private InclusiveRange _Clamp;

            public InclusiveRange Clamp
            {
                get => _Clamp;
                set => _Clamp = value.Clamp(DefaultClamp);
            }

            #endregion 
            #region Constructors

            protected Entry()
            {
                Sense = null;
                Rating = 0;
                Clamp = default;
            }
            public Entry(ISense Sense, int Rating, InclusiveRange Clamp)
                : this()
            {
                this.Sense = Sense;
                this.Rating = Rating;
                this.Clamp = Clamp;
            }
            public Entry(ISense Sense)
                : this(Sense, 5, DefaultClamp)
            {
            }

            #endregion
            #region Serialization

            public virtual void Write(SerializationWriter Writer)
            {
                Sense.Write(Writer);
                Writer.WriteOptimized(Rating);
                Clamp.WriteOptimized(Writer);
            }
            public virtual void Read(SerializationReader Reader)
            {
                Sense = Reader.ReadComposite() as ISense;
                Rating = Reader.ReadOptimizedInt32();
                Clamp = InclusiveRange.ReadOptimizedInclusiveRange(Reader);
            }

            #endregion

            public override string ToString()
                => Sense.Name + "(" + Sense + "): " + Rating + "[" + Clamp.ToString() + "]";

            public Entry SetClamp(InclusiveRange Clamp)
            {
                this.Clamp = Clamp.Clamp(DefaultClamp);
                return this;
            }

            public Entry SetMin(int Min)
                => SetClamp(new InclusiveRange(Min, Clamp).Clamp(DefaultClamp));

            public Entry SetMax(int Max)
                => SetClamp(new InclusiveRange(Clamp, Max).Clamp(DefaultClamp));

            #region Equatability & Comparison

            public bool SenseEquals(Entry Other)
            {
                if (EitherNull(this, Other, out bool areEqual))
                    return areEqual;

                if (this == Other)
                    return true;

                if (Sense != Other.Sense)
                    return false;

                if (Sense.Order != Other.Sense.Order)
                    return false;

                return true;
            }

            public bool Equals(Entry Other, bool IgnoreClamp)
            {
                if (EitherNull(this, Other, out bool areEqual))
                    return areEqual;

                if (this == Other)
                    return true;

                if (!SenseEquals(Other))
                    return false;

                if (Rating != Other.Rating)
                    return false;

                return IgnoreClamp
                    || Clamp == Other.Clamp;
            }

            public bool Equals(Entry Other)
                => Equals(Other, false);

            public int CompareTo(Entry Other)
            {
                if (EitherNull(this, Other, out int nullComp))
                    return nullComp;

                if (Equals(Other))
                    return 0;

                if (!SenseEquals(Other))
                    return 0;

                if (Rating.CompareTo(Other.Rating) is int ratingComp
                    && ratingComp != 0)
                    return ratingComp;

                return Clamp.CompareTo(Other.Clamp);
            }

            #endregion
            #region Conversions

            public static implicit operator KeyValuePair<string, int>(Entry Operand)
                => new(Operand.Sense.Name, Operand.Rating);

            public static explicit operator Entry(KeyValuePair<ISense, int>  Operand)
                => new(Operand.Key, Operand.Value, DefaultClamp);

            public static implicit operator KeyValuePair<ISense, int>(Entry Operand)
                => new(Operand.Sense, Operand.Rating);

            #endregion
        }

        [Serializable]
        public struct StatCollectorEntry : IComposite, IEquatable<StatCollectorEntry>
        {
            public string Class;
            public int Value;
            public string Source;

            public StatCollectorEntry(string Class, int Value, string Source)
            {
                this.Class = Class;
                this.Value = Value;
                this.Source = Source;
            }

            public readonly void Deconstruct(out int Value, out string Source)
            {
                Value = this.Value;
                Source = this.Source;
            }

            public readonly bool Equals(StatCollectorEntry other)
                => EitherNull(this, other, out bool areEqual)
                ? areEqual
                : Class == other.Class;

            public readonly float GetMulti()
                => 1f + (Value / 100f);
        }

        #endregion

        private Dictionary<string, Entry> PerformanceEntries;

        public StringMap<List<StatCollectorEntry>> CollectedStats;
        public float MoveSpeedMultiplier => (GetCollectedStats(MS_MULTI)?.Aggregate(0f, (a, n) => a + n.Value) ?? 100) / 100f;
        public float QuicknessMultiplier => (GetCollectedStats(QN_MULTI)?.Aggregate(0f, (a, n) => a + n.Value) ?? 100) / 100f;

        [SerializeField]
        private bool _WantsSync;
        public bool WantsSync
        {
            get => _WantsSync;
            set
            {
                if (value)
                    Reset();
                _WantsSync = value;
            }
        }

        public SneakPerformance()
        {
            Reset();
        }

        #region Serialization

        public void Write(SerializationWriter Writer)
        {
            Writer.WriteComposite(GetEntries().ToList());
        }
        public void Read(SerializationReader Reader)
        {
            PerformanceEntries = new(DefaultSneakPerformances);
            foreach (Entry entry in Reader.ReadCompositeList<Entry>() ?? new())
                PerformanceEntries[entry.Sense.Name] = entry;
        }

        #endregion

        public void Reset()
        {
            PerformanceEntries = new(DefaultSneakPerformances);
            _WantsSync = false;
            CollectedStats = new()
            {
                { MS_MULTI, new() },
                { QN_MULTI, new() },
            };
        }

        public Entry this[ISense Sense] => PerformanceEntries[Sense.Name];

        public Entry this[string SenseName]
        {
            get
            {
                Entry output = null;
                if (!PerceptionSenses.ContainsKey(SenseName))
                {
                    string thisIndexerName = CallChain(nameof(SneakPerformance), nameof(Entry)) + "[string " + SenseName + "]";
                    if (ModManager.ResolveType(ISense.NAMESPACE, SenseName) is Type senseType)
                    {
                        if (Activator.CreateInstance(senseType) is ISense newSense)
                            output = PerformanceEntries[SenseName] = new Entry(newSense);
                        else
                            MetricsManager.LogModWarning(
                                mod: ModManager.GetMod(senseType.Assembly),
                                Message: thisIndexerName + " could not create instance of " + nameof(Type) + " " + senseType.ToString());
                    }
                    else
                        MetricsManager.LogModWarning(ThisMod, thisIndexerName + " could resolve " + nameof(Type) + " " + CallChain(ISense.NAMESPACE, SenseName));
                }
                else
                    output = PerformanceEntries[SenseName];

                return output;
            }
        }

        public string PerformanceEntriesDebugString(out string Contents, string Delimiter = "\n")
        {
            Contents = GetEntries()
                ?.Aggregate(
                    seed: "",
                    func: (a, n) => a + (!a.IsNullOrEmpty() ? Delimiter : null) + n.ToString());

            return nameof(PerformanceEntries);
        }

        public string CollectedStatsEntriesDebugString(out string Contents, string Delimiter = "\n")
        {
            Contents = CollectedStats
                ?.Aggregate(
                    seed: "",
                    func: (a, n) => a + (!a.IsNullOrEmpty() ? Delimiter : null) + n.Key + ": " + ((n.Value?.Aggregate(0f, (a, n) => a + n.Value) ?? 100) / 100f));

            return nameof(CollectedStats);
        }

        protected SneakPerformance AdjustStatMultiplier<T>(
            string STAT_MULTI,
            T Source,
            int Value,
            string SourceDisplay = null)
            where T : IComponent<GameObject>, new()
        {
            string className = Source?.TypeStringWithGenerics();
            if (SourceDisplay.IsNullOrEmpty())
            {
                SourceDisplay = Source?.TypeStringWithGenerics();
                if (Source is BaseSkill skillSource)
                    SourceDisplay = skillSource.DisplayName;
                else
                if (Source is BaseMutation sourceMutation)
                    SourceDisplay = sourceMutation.GetDisplayName() + " " + sourceMutation.GetMutationTerm();
                else
                if (Source is Effect sourceEffect)
                    SourceDisplay = sourceEffect.DisplayName;
            }
            StatCollectorEntry newEntry = new(className, Value, SourceDisplay);
            CollectedStats[STAT_MULTI] ??= new();
            if (CollectedStats[STAT_MULTI] is List<StatCollectorEntry> collectedStatsList)
            {
                if (collectedStatsList.Any(e => e.Class == className))
                    for (int i = 0; i < collectedStatsList.Count; i++)
                    {
                        if (collectedStatsList[i].Equals(newEntry))
                        {
                            collectedStatsList[i] = newEntry;
                            break;
                        }
                    }
                else
                    CollectedStats[STAT_MULTI].Add(newEntry);
            }
            return this;
        }

        public SneakPerformance AdjustMoveSpeedMultiplier<T>(
            T Source,
            int Value,
            string SourceDisplay = null)
            where T : IComponent<GameObject>, new()
            => AdjustStatMultiplier(MS_MULTI, Source, Value, SourceDisplay);

        public SneakPerformance AdjustQuicknessMultiplier<T>(
            T Source,
            int Value,
            string SourceDisplay = null)
            where T : IComponent<GameObject>, new()
            => AdjustStatMultiplier(QN_MULTI, Source, Value, SourceDisplay);

        public IEnumerable<StatCollectorEntry> GetCollectedStats(string Stat, Predicate<StatCollectorEntry> Filter)
        {
            if ((CollectedStats[Stat] ??= new()) is List<StatCollectorEntry> statList)
                for (int i = 0; i < statList.Count; i++)
                    if (statList[i] is StatCollectorEntry entry 
                        && (Filter == null
                            || Filter(entry)))
                        yield return entry;
        }

        public IEnumerable<StatCollectorEntry> GetCollectedStats(string Stat)
            => GetCollectedStats(Stat, null);

        public SneakPerformance SetClamp(string SenseName, InclusiveRange Clamp)
        {
            this[SenseName].SetClamp(Clamp);
            return this;
        }
        public SneakPerformance SetMin(string SenseName, int Min)
        {
            this[SenseName].SetMin(Min);
            return this;
        }
        public SneakPerformance SetRating(string SenseName, int Rating)
        {
            this[SenseName].Rating = Rating;
            return this;
        }
        public SneakPerformance AdjustRating(string SenseName, int Amount)
        {
            this[SenseName].Rating += Amount;
            return this;
        }
        public SneakPerformance SetMax(string SenseName, int Max)
        {
            this[SenseName].SetMax(Max);
            return this;
        }

        public IEnumerable<Entry> GetEntries(Predicate<Entry> Filter)
        {
            foreach ((string _, Entry entry) in PerformanceEntries ?? new())
                if (Filter == null
                    || Filter(entry))
                    yield return entry;
        }

        public IEnumerable<Entry> GetEntries()
            => GetEntries((Predicate<Entry>)null);

        public IEnumerable<Entry> GetEntries(Predicate<ISense> Filter)
            => GetEntries((Entry e) => Filter == null || Filter(e.Sense));

        public IEnumerable<Entry> GetEntries(Predicate<InclusiveRange> Filter)
            => GetEntries((Entry e) => Filter == null || Filter(e.Clamp));

        public IEnumerable<Entry> GetEntries(Predicate<int> Filter)
            => GetEntries((Entry e) => Filter == null || Filter(e.Rating));

        public static Entry HigherRated(Entry First, Entry Second)
            => First == null
                || Second.CompareTo(First) > 0 
            ? Second 
            : First;

        public static Entry HigherPotential(Entry First, Entry Second)
            => First == null
                || Second.Clamp.CompareTo(First.Clamp) > 0 
            ? Second 
            : First;

        public Entry GetHighestRatedEntry()
            => GetEntries()
                ?.Aggregate(
                    seed: (Entry)null,
                    func: HigherRated);

        public Entry GetHighestPotentialEntry()
            => GetEntries()
                ?.Aggregate(
                    seed: (Entry)null,
                    func: HigherPotential);
    }
}
