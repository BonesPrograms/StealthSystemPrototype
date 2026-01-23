using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Perceptions;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    /// <summary>
    /// Contracts a type as being capable of determining whether or not an <see cref="IConcealedAction"/> occured within proximity of an <see cref="IPerception"/>
    /// </summary>
    public interface IPurview
    {
        public string Attributes { get; set; }

        public abstract List<string> GetPerviewAttributes();
    }
}
