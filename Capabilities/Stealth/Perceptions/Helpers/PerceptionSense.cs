using System;
using System.Collections.Generic;
using System.Text;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public enum PerceptionSense : int
    {
        None, // "null"
        Kinesthetic, // touch
        Thermal, // temperature
        Olfactory, // smell
        Auditory, // hearing
        Visual, // sight
        Sixth, // intuition/ghosts
        Psionic, // mental
        Other, // sundry, probably modded.
    }
}
