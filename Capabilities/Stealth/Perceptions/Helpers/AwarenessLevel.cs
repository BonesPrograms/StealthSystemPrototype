using System;
using System.Collections.Generic;
using System.Text;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public enum AwarenessLevel : int
    {
        None,
        Awake,
        Suspect,
        Aware,
        Alert,
    }
}
