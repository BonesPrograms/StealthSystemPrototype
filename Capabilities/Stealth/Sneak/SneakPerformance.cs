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
    public class SneakPerformance : Rack<IAlert>
    {
        #region Const & Static

        public static SneakPerformance DefaultSneakPerformance => new(IAlert.Alerts);

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
                {
                    Clear();
                    AddRange(IAlert.Alerts);
                }
                _WantsSync = value;
            }
        }

        public SneakPerformance()
            : base()
        {
        }
        public SneakPerformance(IReadOnlyList<IAlert> SourceList)
            : base(SourceList)
        {
        }
        public SneakPerformance(SneakPerformance Source)
            : this(Source as IReadOnlyList<IAlert>)
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
            CollectedStats = new()
            {
                { MS_MULTI, new() },
                { QN_MULTI, new() },
            };
        }

        public int this[IAlert Alert]
        {
            get => GetByType(Alert.Type).Intensity;
            set => GetByType(Alert.Type).Intensity = value;
        }

        public int this[string AlertName]
        {
            get => Items?.FirstOrDefault(a => a.Name == AlertName)?.Intensity ?? 0;
            set
            {
                if (Items?.FirstOrDefault(a => a.Name == AlertName) is IAlert alert)
                    alert.Intensity = value;
                else
                {
                    ModInfo callingMod = ThisMod;

                    StackTrace trace = new(1);
                    StackFrame frame = null;
                    for (int i = 0; i < 8; i++)
                    {
                        frame = trace?.GetFrame(i);
                        if (frame.GetMethod()?.DeclaringType is Type declaringType
                            && declaringType != typeof(SneakPerformance))
                        {
                            callingMod = ModManager.GetMod(declaringType.Assembly);
                            break;
                        }
                    }
                    MetricsManager.LogModWarning(
                        mod: callingMod,
                        Message: "Attempted to adjust " + nameof(alert.Intensity) + " of non-existent " + nameof(IAlert) + ": " + AlertName + ". " +
                            "Did you mean " + IAlert.Alerts?.Aggregate("NO_MATCHING_ALERT", (a, n) => GetCloserMatch(AlertName, a, n.Name)) + "?\n" +
                            frame);
                }
            }
        }

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

        public SneakPerformance SetRating(string AlertName, int Rating)
        {
            this[AlertName] = Rating;
            return this;
        }
        public SneakPerformance AdjustRating(string AlertName, int Amount)
        {
            this[AlertName] += Amount;
            return this;
        }
        public SneakPerformance SetMax(string AlertName, int Max)
        {
            this[AlertName] =  Math.Min(this[AlertName], Max);
            return this;
        }
        public SneakPerformance SetMin(string AlertName, int Min)
        {
            this[AlertName] =  Math.Max(Min, this[AlertName]);
            return this;
        }
        public SneakPerformance SetClamp(string AlertName, InclusiveRange Clamp)
            => SetMin(AlertName, Clamp.Min)
                .SetMax(AlertName, Clamp.Max);

        public IAlert GetByType(Type AlertType)
        {
            if (Items != null)
                for (int i = 0; i < Count; i++)
                    if (Items[i] is IAlert alert
                        && alert.IsType(AlertType))
                        return alert;

            throw new ArgumentOutOfRangeException(
                paramName: nameof(AlertType),
                message: nameof(AlertType) + " (" + AlertType.ToStringWithGenerics() + ") " +
                    "does not exist in Collection (this should be impossible).");
        }

        public IAlert GetMatchingAlert(IAlert Alert)
            => GetByType(Alert.Type);

        public bool Contains<A>(A Alert)
            where A : IAlert
            => Items?.Any(a => a.IsType(Alert.Type)) ?? false;

        public static IAlert HigherRated(IAlert First, IAlert Second)
            => First == null
                || Second.Intensity.CompareTo(First.Intensity) > 0
            ? Second
            : First;

        public IAlert GetHighestRatedEntry()
            => Items
                ?.Aggregate(
                    seed: (IAlert)null,
                    func: HigherRated);
    }
}
