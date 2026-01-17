using System;
using System.Collections.Generic;
using System.Text;

namespace StealthSystemPrototype.Perceptions
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
