using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype.Events;

using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class Olfactory : BasePerception, IComposite
    {
        #region Constructors

        public Olfactory()
            : base()
        {
            Occludes = true;
            Tapers = true;
            Sense = PerceptionSense.Olfactory;
        }
        public Olfactory(GameObject Owner, int BaseScore, int BaseRadius)
            : base(Owner, PerceptionSense.Olfactory, BaseScore, BaseRadius)
        {
        }
        public Olfactory(GameObject Owner)
            : this(Owner, BASE_PERCEPTION_SCORE, BASE_PERCEPTION_RADIUS)
        {
            Occludes = true;
            Tapers = true;
        }

        #endregion

        public override bool Validate(GameObject Owner = null)
        {
            Owner ??= this.Owner;
            if (Owner == null)
                return false;

            if (Owner != this.Owner)
                return false;

            if (Owner.Body == null
                || Owner.Body.GetFirstPart("Face", false) is null)
                return false;

            return true;
        }

        protected override PerceptionRating? GetPerceptionRating(GameObject Owner = null)
        {
            throw new NotImplementedException();
        }

        public override int GetScore(GameObject Owner = null)
        {
            throw new NotImplementedException();
        }

        public override int GetRadius(GameObject Owner = null)
        {
            throw new NotImplementedException();
        }

        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            // Write Here.
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            // Read Here.
        }

        #endregion
    }
}
