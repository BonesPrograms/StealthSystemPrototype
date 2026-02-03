using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Perceptions.Specs
{
    [Serializable]
    public class HasPerceptionsSpec : GameObjectSpecification
    {
        public HasPerceptionsSpec(GameObject Subject)
            : base(Subject) { }

        public override bool Check(GameObject Subject)
            => Subject != null
            && Subject.HasAnyPerceptions();
    }
}
