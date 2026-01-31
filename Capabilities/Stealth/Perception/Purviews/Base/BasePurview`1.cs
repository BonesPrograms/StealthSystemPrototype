using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Capabilities.Stealth.DelayedLinearDoubleDiffuser;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    [StealthSystemBaseClass]
    [Serializable]
    public abstract class BasePurview<A>
        : BasePurview
        where A : class, IAlert, new()
    {
        public override Type AlertType => typeof(A);

        #region Constructors

        public BasePurview()
            : base()
        {
        }
        protected BasePurview(BasePerception ParentPerception)
            : base(ParentPerception)
        {
        }
        protected BasePurview(BasePerception ParentPerception, int Value)
            : base(ParentPerception, Value)
        {
        }
        public BasePurview(BasePurview<A> Source)
            : base(Source.ParentPerception, Source.Value)
        {
        }

        #endregion
        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            // do writing here
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            // do reading here
        }

        #endregion

        public override Type GetAlertType()
            => AlertType;

        public override void Configure(Dictionary<string, object> args = null)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                    Debug.Arg(nameof(args), args?.Count ?? 0),
                });

            if (!args.IsNullOrEmpty())
            {
                args.ForEach(kvp => Debug.Log(kvp.Key, kvp.Value, Indent: indent[1]));

                if (args.ContainsKey(nameof(ParentPerception))
                    && args[nameof(ParentPerception)] is BasePerception parentPerceptionArg)
                {
                    ParentPerception = parentPerceptionArg;
                }
                if (args.ContainsKey(nameof(Value))
                    && args[nameof(Value)] is int valueArg)
                {
                    Value = valueArg;
                }
            }
        }

        #region Predicates

        public override bool IsForAlert(IAlert Alert)
            => Alert.IsType(typeof(A));

        #endregion
        #region Equatable

        public override bool Equals(IPurview Other)
            => base.Equals(Other)
            && GetAlertType() == Other.GetAlertType();

        #endregion
        #region Conversion

        public static explicit operator int(BasePurview<A> Operand)
            => Operand.GetEffectiveValue();

        #endregion
    }
}
