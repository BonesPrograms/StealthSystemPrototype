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

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using static StealthSystemPrototype.Utils;

using SerializeField = UnityEngine.SerializeField;
using System.Diagnostics;
using XRL.Language;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [HasModSensitiveStaticCache]
    [HasGameBasedStaticCache]
    [Serializable]
    public class SneakPerformance : Rack<BaseAlert>
    {
        #region Const & Static

        public static SneakPerformance DefaultSneakPerformance => new(BaseAlert.Alerts);

        public static string MS_MULTI => "MoveSpeed_Multiplier";
        public static string QN_MULTI => "Quickness_Multiplier";

        #endregion
        #region Helpers

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

        public static StringMap<List<StatCollectorEntry>> DefaultCollectedStats => new()
        {
            { MS_MULTI, new() },
            { QN_MULTI, new() },
        };

        public StringMap<List<StatCollectorEntry>> CollectedStats = DefaultCollectedStats;

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
                {
                    Clear();
                    AddRange(BaseAlert.Alerts);
                }
                _WantsSync = value;
            }
        }

        public SneakPerformance()
            : base()
        {
        }
        public SneakPerformance(IReadOnlyList<BaseAlert> SourceList)
            : base(SourceList)
        {
        }
        public SneakPerformance(SneakPerformance Source)
            : this(Source as IReadOnlyList<BaseAlert>)
        {
            CollectedStats = Source.CollectedStats;

        }

        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            Writer.WriteOptimized(Variant);
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            Variant = Reader.ReadOptimizedInt32();
        }

        #endregion

        public override void Clear()
        {
            base.Clear();
            _WantsSync = false;
            CollectedStats = DefaultCollectedStats;
        }

        public BaseAlert this[BaseAlert Alert] => GetByType(Alert.Type);

        public BaseAlert this[string AlertName] => Items?.FirstOrDefault(a => a.Name == AlertName);

        public string EntriesDebugString(out string Contents, string Delimiter = "\n")
        {
            Contents = Items
                ?.Aggregate(
                    seed: "",
                    func: (a, n) => a + (!a.IsNullOrEmpty() ? Delimiter : null) + n.ToString());

            return "Entries";
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

        public SneakPerformance SetIntensity(string AlertName, int Intensity)
        {
            this[AlertName].Intensity = Intensity;
            return this;
        }
        public SneakPerformance AdjustRating(string AlertName, int Amount)
        {
            this[AlertName].Intensity += Amount;
            return this;
        }
        public SneakPerformance SetMax(string AlertName, int Max)
        {
            this[AlertName].Intensity = Math.Min(this[AlertName].Intensity, Max);
            return this;
        }
        public SneakPerformance SetMin(string AlertName, int Min)
        {
            this[AlertName].Intensity =  Math.Max(Min, this[AlertName].Intensity);
            return this;
        }
        public SneakPerformance SetClamp(string AlertName, InclusiveRange Clamp)
            => SetMin(AlertName, Clamp.Min)
                .SetMax(AlertName, Clamp.Max);

        public BaseAlert GetByType(Type AlertType)
        {
            if (Items != null)
                for (int i = 0; i < Count; i++)
                    if (Items[i] is BaseAlert alert
                        && alert.IsType(AlertType))
                        return alert;

            throw new ArgumentOutOfRangeException(
                paramName: nameof(AlertType),
                message: nameof(AlertType) + " (" + AlertType.ToStringWithGenerics() + ") " +
                    "does not exist in Collection (this should be impossible).");
        }

        public BaseAlert GetMatchingAlert(BaseAlert Alert)
            => GetByType(Alert.Type);

        public bool Contains<A>(A Alert)
            where A : IAlert
            => Items?.Any(a => a.IsType(Alert.Type)) ?? false;

        public static BaseAlert HigherRated(BaseAlert First, BaseAlert Second)
            => First == null
                || Second.Intensity.CompareTo(First.Intensity) > 0
            ? Second
            : First;

        public BaseAlert GetHighestRatedEntry()
            => Items
                ?.Aggregate(
                    seed: (BaseAlert)null,
                    func: HigherRated);
    }
}
