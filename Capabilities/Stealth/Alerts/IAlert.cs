using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

using StealthSystemPrototype.Perceptions;
using XRL.Collections;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    /// <summary>
    /// Contracts a type as being representative of the obviousness of one aspect of an <see cref="IConcealedAction"/> to an appropriate <see cref="IPerception"/>.
    /// </summary>
    public interface IAlert<A> : IAlert
        where A : IAlert<A>
    {
        public new A AdjustIntensity(int Amount);
        public new A Copy();
    }

    /// <summary>
    /// Contracts a type as being representative of the obviousness of one aspect of an <see cref="IConcealedAction"/> to an appropriate <see cref="IPerception"/>.
    /// </summary>
    /// <remarks>
    /// This serves as a non-generic base which should typically not be derived from directly.
    /// </remarks>
    public interface IAlert : IDisposable, IComposite
    {
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

        public bool IsType(Type Type)
            => Type.InheritsFrom(GetType());

        public static void ReadAlert(SerializationWriter Writer, IAlert Alert)
        {
            Writer.WriteOptimized(Alert.Intensity);
            Writer.WriteOptimized(Alert.Properties);
        }

    }
}
