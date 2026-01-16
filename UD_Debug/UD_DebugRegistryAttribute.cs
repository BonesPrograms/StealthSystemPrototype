using System;
using System.Collections.Generic;
using System.Text;

namespace StealthSystemPrototype.Logging
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class UD_DebugRegistryAttribute : Attribute
    {
    }
}
