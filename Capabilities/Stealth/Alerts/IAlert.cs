using System;
using System.Collections.Generic;
using System.Linq;

using XRL;
using XRL.World;

using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.Alerts.AlertSet;

namespace StealthSystemPrototype.Alerts
{
    /// <summary>
    /// Contracts a type as being representative of the obviousness of one aspect of an <see cref="IConcealedAction"/> to an appropriate <see cref="IPerception"/>.
    /// </summary>
    /// <remarks>
    /// This serves as a non-generic base which should typically not be derived from directly.
    /// </remarks>
    public interface IAlert : ICoalescible<IAlert>, ICoalescible<int>, IDisposable, IComposite
    {
        #region Static & Cache

        public static A GetAlert<A>(int Intensity, Dictionary<string, string> Properties = null)
            where A : class, IAlert, new()
        {
            A newAlert = new()
            {
                Intensity = Intensity,
            };
            newAlert.Initialize();
            newAlert.Properties ??= new();
            foreach ((string name, string value) in Properties)
            {
                if (newAlert.Properties.ContainsKey(name))
                    newAlert.Properties[name] += "," + value;
                else
                    newAlert.Properties[name] = value;
            }
            newAlert.Created();
            return newAlert;
        }

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
