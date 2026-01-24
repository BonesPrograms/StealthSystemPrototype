using System;
using System.Collections.Generic;
using System.Text;

namespace StealthSystemPrototype
{
    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class StealthSystemBaseClassAttribute : Attribute
    {
    }
}
