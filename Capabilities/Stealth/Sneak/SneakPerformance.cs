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

using StealthSystemPrototype.Perceptions;

using Sense = StealthSystemPrototype.Perceptions.PerceptionSense;

namespace StealthSystemPrototype.Capabilities.Stealth.Sneak
{
    [Serializable]
    public class SneakPerformance : IComposite
    {
        #region Const & Static

        public static Dictionary<string, Sense> PerceptionSenses => Utils.GetValuesDictionary<Sense>() ?? new();

        public static Dictionary<Sense, Dictionary<string, Entry>> DefaultSneakPerformances => PerceptionSenses
            ?.Aggregate(
                seed: new Dictionary<Sense, Dictionary<string, Entry>>(),
                func: delegate (Dictionary<Sense, Dictionary<string, Entry>> Accumulator, KeyValuePair<string, Sense> Next)
                {
                    (string Name, Sense Sense) = Next;

                    Accumulator[Sense] = new();
                    if (Sense.Twixt(Sense.None, Sense.Other))
                        Accumulator[Sense][Name] = new Entry(Name, 5);

                    return Accumulator;
                })
            ?? new();

        public static string MOVESPEED_MULTI => "MoveSpeed_Multiplier";

        #endregion
        #region Helpers

        [Serializable]
        public class Entry : IComposite, IEquatable<Entry>, IComparable<Entry>
        {
            public static InclusiveRange DefaultClamp => new(..100);

            #region Instance Fields & Properties

            private Sense _Sense;
            public Sense Sense
            {
                get => _Sense;
                private set => _Sense = value;
            }

            private string _Name;
            public string Name
            {
                get => _Name;
                private set => SetName(value);
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

            private Entry()
            {
                Sense = Sense.None;
                _Name = null;
                Rating = 0;
                Clamp = default;
            }

            private Entry(int Rating, InclusiveRange Clamp)
            {
                this.Rating = Rating;
                this.Clamp = Clamp;
            }
            private Entry(Sense Sense, int Rating, InclusiveRange Clamp)
                : this(Rating, Clamp)
            {
                this.Sense = Sense;
                Name = Sense.ToString();
            }
            public Entry(string SenseName, int Rating)
                : this(Sense.Other, Rating, DefaultClamp)
            {
                if (PerceptionSenses.ContainsKey(SenseName)
                    && PerceptionSenses[SenseName] < Sense.Other)
                    this.Sense = PerceptionSenses[SenseName];

                Name = SenseName;
            }

            #endregion
            #region Serialization

            public virtual void Write(SerializationWriter Writer)
            {
                Writer.WriteOptimized((int)Sense);
                Writer.WriteOptimized(Name);
                Writer.WriteOptimized(Rating);
            }
            public virtual void Read(SerializationReader Reader)
            {
                Sense = (Sense)Reader.ReadOptimizedInt32();
                _Name = Reader.ReadOptimizedString();
                Rating = Reader.ReadOptimizedInt32();
            }

            #endregion

            public override string ToString()
                => Name + "(" + Sense + "): " + Rating + "[" + Clamp.ToString() + "]";

            private bool SetName(string Name = null)
            {
                if (Sense < Sense.Other)
                {
                    _Name = Sense.ToString();
                    return false;
                }
                if (Name == null)
                {
                    Sense = Sense.None;
                    return SetName();
                }

                _Name = Name;
                return true;
            }

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
                if (Utils.EitherNull(this, Other, out bool areEqual))
                    return areEqual;

                if (this == Other)
                    return true;

                if (Sense != Other.Sense)
                    return false;

                if (!Sense.Twixt(PerceptionSense.None, PerceptionSense.Other)
                    && Name != Other.Name)
                    return false;

                return true;
            }

            public bool Equals(Entry Other, bool IgnoreClamp)
            {
                if (Utils.EitherNull(this, Other, out bool areEqual))
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
                if (Utils.EitherNull(this, Other, out int nullComp))
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
                => new(Operand.Name, Operand.Rating);

            public static explicit operator Entry(KeyValuePair<string, int>  Operand)
                => new(Operand.Key, Operand.Value);

            public static implicit operator KeyValuePair<Sense, int>(Entry Operand)
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
                => Utils.EitherNull(this, other, out bool areEqual)
                ? areEqual
                : Class == other.Class;

            public static explicit operator (int Multi, string Source)(StatCollectorEntry Operand)
                => (Operand.Value, Operand.Source);
        }

        #endregion

        private Dictionary<Sense, Dictionary<string, Entry>> PerformanceEntries;

        public StringMap<List<StatCollectorEntry>> CollectedStats;
        public float MoveSpeedMultiplier => (GetCollectedStats(MOVESPEED_MULTI)?.Aggregate(0f, (a, n) => a + n.Value) ?? 100) / 100f;

        public bool WantsSync;

        public SneakPerformance()
        {
            PerformanceEntries = new(DefaultSneakPerformances);
            WantsSync = false;
            CollectedStats = new()
            {
                { "MoveSpeed Multiplier", new() },
            };
        }

        #region Serialization

        public void Write(SerializationWriter Writer)
        {
            Writer.WriteComposite(GetEntries(e => e > Sense.None).ToList());
        }
        public void Read(SerializationReader Reader)
        {
            PerformanceEntries = new(DefaultSneakPerformances);
            foreach (Entry entry in Reader.ReadCompositeList<Entry>() ?? new())
                PerformanceEntries[entry.Sense][entry.Name] = entry;
        }

        #endregion

        public IEnumerable<Entry> this[Sense Sense] => PerformanceEntries[Sense].Select(kvp => kvp.Value);

        public Entry this[string SenseName]
        {
            get
            {
                Entry output = null;
                if (!PerceptionSenses.ContainsKey(SenseName))
                    output = PerformanceEntries[Sense.Other][SenseName] = new Entry(SenseName, 5);
                else 
                {
                    Sense sense = PerceptionSenses[SenseName];
                    if (sense.Twixt(Sense.None, Sense.Other))
                        output = this[sense].First();
                    else
                    if (sense != Sense.None)
                        output = PerformanceEntries[Sense.Other][SenseName];
                }
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

        public SneakPerformance AdjustMoveSpeedMultiplier<T>(T Source, int Value, string SourceDisplay = null)
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
            if (CollectedStats[MOVESPEED_MULTI] is List<StatCollectorEntry> collectedStatsList
                && collectedStatsList.Any(e => e.Class == className))
                for (int i = 0; i < collectedStatsList.Count; i++)
                {
                    if (collectedStatsList[i].Equals(newEntry))
                    {
                        collectedStatsList[i] = newEntry;
                        break;
                    }
                }
            else
                CollectedStats[MOVESPEED_MULTI].Add(newEntry);
            return this;
        }

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
            foreach ((Sense sense, Dictionary<string, Entry> senseDict) in PerformanceEntries ?? new())
                if (sense == Sense.None)
                    continue;
                else
                    foreach ((string _, Entry entry) in senseDict)
                        if (Filter == null
                            || Filter(entry))
                            yield return entry;
        }

        public IEnumerable<Entry> GetEntries()
            => GetEntries((Predicate<Entry>)null);

        public IEnumerable<Entry> GetEntries(Predicate<Sense> Filter)
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
