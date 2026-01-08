using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype.Events;

using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class Perception2 : IComposite
    {
        [Serializable]
        public struct PerceptionScore : IComposite
        {
            #region Const & Static Values
            public static string BASE_PERCEPTION_TYPE => "";

            public static string VISIUAL => "Visual";
            public static string AUDITORY => "Auditory";
            public static string OLFACTORY => "Olfactory";

            public const int MIN_PERCEPTION_VALUE = 0;
            public const int MAX_PERCEPTION_VALUE = 100;
            public const int BASE_PERCEPTION_VALUE = 20; // AwarenessLevel.Awake

            public const int MIN_PERCEPTION_RADIUS = 0;
            public const int MAX_PERCEPTION_RADIUS = 84; // corner to corner of a single zone.
            public const int BASE_PERCEPTION_RADIUS = 5;

            public static PerceptionScore Empty => new(null, MIN_PERCEPTION_VALUE, MIN_PERCEPTION_RADIUS);
            public static PerceptionScore Base => new(BASE_PERCEPTION_TYPE, BASE_PERCEPTION_VALUE, BASE_PERCEPTION_RADIUS);
            #endregion

            public string Type;

            private int _Value;
            public int Value
            {
                get => _Value = RestrainPerceptionScore(_Value);
                set => _Value = RestrainPerceptionScore(value);
            }

            private int _Radius;
            public int Radius
            {
                get => _Radius = RestrainPerceptionScore(_Radius);
                set => _Radius = RestrainPerceptionScore(value);
            }

            public PerceptionScore(string Type, int Value, int Radius)
            {
                this.Type = Type;
                _Value = RestrainPerceptionScore(Value);
                _Radius = RestrainPerceptionRadius(Radius);
            }

            public static int RestrainPerceptionScore(int Value, int? Cap = null)
                => Value.Restrain(MIN_PERCEPTION_VALUE, Math.Max(Cap ?? MAX_PERCEPTION_VALUE, MAX_PERCEPTION_VALUE));

            public static int RestrainPerceptionRadius(int Radius, int? Cap = null)
                => Radius.Restrain(MIN_PERCEPTION_RADIUS, Math.Max(Cap ?? MAX_PERCEPTION_RADIUS, MAX_PERCEPTION_RADIUS));

            private int CalculateAwareness()
                => (int)Math.Ceiling(((Value + 1) / 20.0) - 1);

            // ((Value - 1) / 20) - 1
            // Value >= 80 -> Alert
            // Value >= 60 -> Aware
            // Value >= 40 -> Suspect
            // Value >= 20 -> Awake
            // Value <  20 -> None
            public AwarenessLevel GetAwarenessLevel(AwarenessLevel? Cap = null)
                => (AwarenessLevel)CalculateAwareness().Restrain(
                    Min: (int)AwarenessLevel.None, 
                    Max: Cap != null 
                        ? Math.Max((int)Cap, (int)AwarenessLevel.Alert)
                        : (int)AwarenessLevel.Alert);

            public string ToString(bool Short)
                => (Short ? (Type?[0] ?? '?').ToString() : Type ?? "null?") + "[" + Value + "]@R(" + Radius + ")";

            public override string ToString()
                => ToString(false);

            #region Serialization
            public readonly void Write(SerializationWriter Writer)
            {
                Writer.WriteOptimized(_Value);
                Writer.WriteOptimized(_Radius);
            }

            public void Read(SerializationReader Reader)
            {
                Value = Reader.ReadOptimizedInt32();
                Radius = Reader.ReadOptimizedInt32();
            }
            #endregion
        }

        public Dictionary<string, PerceptionScore> Scores;

        public Perception2()
        {
            Scores = null;
        }

        public Perception2(Dictionary<string, PerceptionScore> Scores)
            : base()
        {
            this.Scores = Scores ?? new();
        }

        public Perception2(GameObject Perciever)
            : this(GetPerceptionTypesEvent.GetFor(Perciever))
        {
        }

        public string ToString(GameObject ParentObject, bool Short = true)
        {
            string opening = ParentObject?.DebugName?.Strip();
            if (!opening.IsNullOrEmpty())
                opening += ": ";

            return Scores?.Aggregate("", (a, n) => a + (!a.IsNullOrEmpty() ? ", " : opening) + n.Value.ToString(Short));
        }
        public override string ToString()
            => ToString(null);
    }
}
