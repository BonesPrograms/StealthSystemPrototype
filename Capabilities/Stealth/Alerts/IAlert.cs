using System;
using System.Collections.Generic;
using System.Linq;

using XRL;
using XRL.World;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    /// <summary>
    /// Contracts a type as being representative of the obviousness of one aspect of an <see cref="IConcealedAction"/> to an appropriate <see cref="IPerception"/>.
    /// </summary>
    /// <remarks>
    /// This serves as a non-generic base which should typically not be derived from directly.
    /// </remarks>
    public interface IAlert : IDisposable, IComposite
    {
        #region Static & Cache

        public static A GetAlert<A>(int Intensity)
            where A : class, IAlert, new()
            => new()
            {
                Intensity = Intensity
            };

        #endregion

        public bool IsBase { get; }

        public string Name { get; }

        public Type Type => GetType();

        public int DefaultIntensity { get; }

        public int Intensity { get; set; }

        public Dictionary<string, string> Properties { get; set; }

        public void Initialize();

        public void Created();

        public IAlert AdjustIntensity(int Amount);

        public IAlert Copy();

        public bool IsType(Type Type);

        public bool IsSame(IAlert Alert);

        public bool IsMatch(AlertContext Context);
    }
}
