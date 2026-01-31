using System;
using System.Collections.Generic;
using System.Text;

namespace StealthSystemPrototype
{
    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class StealthSystemBaseClassAttribute : Attribute
    {
    }
}
