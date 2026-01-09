using System;
using System.Collections.Generic;
using System.Text;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public enum PerceptionSense : int
    {
        None,
        Thermal,
        Olfactory,
        Auditory,
        Visual,
        Psionic,
        Sixth,
        Other,
    }
}
