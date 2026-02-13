using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL;
using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    /// <summary>
    /// Defines methods for determining whether or not an <see cref="IConcealedAction"/> occured within proximity of an <see cref="IPerception"/>.
    /// </summary>
    public interface IPurview
    {
        #region Static & Const

        public static int MIN_VALUE => 0;

        public static int MAX_VALUE => 84;

        public static int DEFAULT_VALUE => 4;

        #endregion

        int BaseValue { get; }

        bool Occludes { get; }

        bool CheckWithin(IPerception Perception, ref PerceptionSet.AlertEvent E);

        int GetEffectiveLevel(IPerception Perception, ref PerceptionSet.AlertEvent E);
    }
}
