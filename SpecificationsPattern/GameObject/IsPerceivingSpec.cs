using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Perceptions.Specs
{
    [Serializable]
    public class IsPerceivingSpec : GameObjectSpecification
    {
        public GameObject Perceiver
        {
            get => Subject;
            set => Subject = value;
        }

        protected GameObject _Hider;
        public GameObject Hider
        {
            get => _Hider;
            set => _Hider = value;
        }

        public IsPerceivingSpec(GameObject Perceiver, GameObject Hider)
            : base(Perceiver)
        {
            this.Hider = Hider;
        }
        public IsPerceivingSpec(GameObject Hider)
            : base(null)
        {
            this.Hider = Hider;
        }

        public override bool Check()
            => Check(Perceiver);

        public override bool Check(GameObject Subject)
            => Subject != null
            && Hider != null
            && Perceiver.IsPerceiving(Hider);
    }
}
