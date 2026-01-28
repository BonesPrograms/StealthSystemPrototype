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

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [HasModSensitiveStaticCache]
    [HasGameBasedStaticCache]
    [Serializable]
    public class SneakPerformance : IComposite
    {
        #region Const & Static

        [ModSensitiveStaticCache]
        [GameBasedStaticCache(CreateInstance = false)]
        private static Dictionary<string, AlertRating> _DefaultSneakPerformances;
        public static Dictionary<string, AlertRating> DefaultSneakPerformances => _DefaultSneakPerformances ??= IAlert.AlertsByName
            ?.Aggregate(
                seed: new Dictionary<string, AlertRating>(),
                func: delegate (Dictionary<string, AlertRating> Accumulator, KeyValuePair<string, IAlert> Next)
                {
                    Accumulator[Next.Key] = new AlertRating(Next.Value);
                    return Accumulator;
                })
            ?? new();

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

        private Dictionary<string, AlertRating> PerformanceEntries;

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
            foreach (AlertRating entry in Reader.ReadCompositeList<AlertRating>() ?? new())
                PerformanceEntries[entry.Alert.Name] = entry;
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

        public AlertRating this[IAlert Alert] => PerformanceEntries[Alert.Name];

        public AlertRating this[string AlertName]
        {
            get
            {
                if (!PerformanceEntries.ContainsKey(AlertName))
                {
                    string thisIndexerName = CallChain(nameof(SneakPerformance), nameof(AlertRating)) + "[string " + AlertName + "]";

                    if (!IAlert.AlertsByName.ContainsKey(AlertName))
                        MetricsManager.LogModWarning(ThisMod, thisIndexerName + " is not a cached " + nameof(IAlert) + " (" + nameof(IAlert.AlertsByName) + ").");

                    if (IAlert.AlertsByName[AlertName] is IAlert alert)
                    {
                        if (alert.Copy() is IAlert newAlert)
                            return PerformanceEntries[AlertName] = new AlertRating(newAlert);
                        else
                            MetricsManager.LogModWarning(
                                mod: ModManager.GetMod(alert.Type.Assembly),
                                Message: thisIndexerName + " could not create copy of " + nameof(Type) + " " + alert.Name);
                    }
                    return null;
                }
                return PerformanceEntries[AlertName];
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
            this[SenseName].SetRating(Rating);
            return this;
        }
        public SneakPerformance AdjustRating(string SenseName, int Amount)
        {
            this[SenseName].AdjustRating(Amount);
            return this;
        }
        public SneakPerformance SetMax(string SenseName, int Max)
        {
            this[SenseName].SetMax(Max);
            return this;
        }

        public A GetAlert<A>(int Intensity)
            where A : class, IAlert, new()
        {
            A alert = new()
            {
                Intensity = Intensity
            };
            alert.AdjustIntensity(-GetEntry<A>().Rating);
            return alert;
        }

        public IEnumerable<AlertRating> GetEntries(Predicate<AlertRating> Filter)
        {
            foreach ((string _, AlertRating entry) in PerformanceEntries ?? new())
                if (Filter == null
                    || Filter(entry))
                    yield return entry;
        }

        public AlertRating GetEntry<A>()
            where A : class, IAlert, new()
            => this[new A()];

        public IEnumerable<AlertRating> GetEntries()
            => GetEntries((Predicate<AlertRating>)null);

        public IEnumerable<AlertRating> GetEntries(Predicate<IAlert> Filter)
            => GetEntries((AlertRating e) => Filter == null || Filter(e.Alert));

        public IEnumerable<AlertRating> GetEntries(Predicate<int> Filter)
            => GetEntries((AlertRating e) => Filter == null || Filter(e.Rating));

        public static AlertRating HigherRated(AlertRating First, AlertRating Second)
            => First == null
                || Second.CompareTo(First) > 0 
            ? Second 
            : First;

        public static AlertRating HigherPotential(AlertRating First, AlertRating Second)
            => First == null
                || Second.Clamp.CompareTo(First.Clamp) > 0 
            ? Second 
            : First;

        public AlertRating GetHighestRatedEntry()
            => GetEntries()
                ?.Aggregate(
                    seed: (AlertRating)null,
                    func: HigherRated);

        public AlertRating GetHighestPotentialEntry()
            => GetEntries()
                ?.Aggregate(
                    seed: (AlertRating)null,
                    func: HigherPotential);
    }
}
