using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype.Events;

using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class Auditory : BasePerception, IComposite
    {
        #region Constructors

        public Auditory()
            : base()
        {
            Occludes = true;
            Tapers = true;
            Sense = PerceptionSense.Auditory;
        }
        public Auditory(GameObject Owner, int BaseScore, int BaseRadius)
            : base(Owner, PerceptionSense.Auditory, BaseScore, BaseRadius)
        {
            Occludes = true;
            Tapers = true;
        }
        public Auditory(GameObject Owner)
            : this(Owner, BASE_PERCEPTION_SCORE, BASE_PERCEPTION_RADIUS)
        {
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
