using System;
using System.Collections.Generic;
using System.Text;

namespace StealthSystemPrototype.Senses
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
