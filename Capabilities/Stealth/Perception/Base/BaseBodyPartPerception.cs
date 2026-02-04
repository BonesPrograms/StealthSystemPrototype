using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Rules;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts.Mutation;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Perceptions
{
    [StealthSystemBaseClass]
    [Serializable]
    public abstract class BaseBodyPartPerception
        : BasePerception
        , IBodyPartPerception
    {
        #region Debug
        [UD_DebugRegistry]
        public static void BaseBodyPartPerception_DoDebugRegistry(DebugMethodRegistry Registry)
        {
            Registry.RegisterEach(
                Type: typeof(StealthSystemPrototype.Perceptions.BaseBodyPartPerception),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    { nameof(Validate), false },
                });
        }
        #endregion

        public override GameObject Owner
        {
            get => base.Owner ??= FindOwner(_Source);
            set => base.Owner = value;
        }

        protected string _SourceType = null;
        public virtual string SourceType
        {
            get => _SourceType ?? _Source.Type;
            protected set => _SourceType = value;
        }

        protected BodyPart _Source = null;
        public virtual BodyPart Source
        {
            get => _Source ??= FindSource(_Owner, SourceType, ref _Source);
            set => _Source = value;
        }

        #region Constructors

        public BaseBodyPartPerception()
            : base()
        {
        }
        public BaseBodyPartPerception(
            GameObject Owner,
            BodyPart Source,
            int Level,
            int? PurviewValue = null)
            : base(Owner, Level)
        {
            this.Owner = Owner;
            _Source = Source;
            SourceType = Source.Type;
            Purview?.MaybeSetValue(PurviewValue);
        }
        public BaseBodyPartPerception(
            BodyPart Source,
            int Level,
            int? PurviewValue = null)
            : this(FindOwner(Source), Source, Level, PurviewValue)
        {
        }

        #endregion
        #region Serialization

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
            // do writing here
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            // do reading here
        }

        #endregion

        public static BodyPart FindSource(GameObject Owner, string SourceType, ref BodyPart Source)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Owner?.MiniDebugName()),
                    Debug.Arg(nameof(SourceType), SourceType),
                });

            if (Source == null)
            {
                Debug.Log(nameof(Source) + " null", "finding new one", Indent: indent[1]);
                if (Owner?.Body?.LoopPart(SourceType, ExcludeDismembered: true) is List<BodyPart> bodyParts)
                {
                    if (bodyParts.IsNullOrEmpty())
                        Debug.CheckNah(nameof(bodyParts), "empty", Indent: indent[2]);

                    if (bodyParts.Count > 1)
                    {
                        Debug.Log(
                            Label: nameof(bodyParts) + " " + bodyParts.Count,
                            Value: CallChain(nameof(bodyParts), nameof(bodyParts.Sort)) + "(" + nameof(ClosestBodyPart) + ")",
                            Indent: indent[3]);
                        bodyParts.Sort(ClosestBodyPart);
                    }
                    if (bodyParts[0] is BodyPart foundPart)
                    {
                        Source = foundPart;
                        Debug.CheckYeh(nameof(foundPart), foundPart, Indent: indent[1]);
                    }
                    else
                        Debug.CheckNah(nameof(foundPart), "empty (this shouldn't be possible)", Indent: indent[1]);
                }
                else
                    Debug.CheckNah(nameof(bodyParts), "null", Indent: indent[2]);
            }
            return Source;
        }

        public virtual BodyPart GetSource()
            => Source;

        public static GameObject FindOwner(BodyPart Source)
            => IBodyPartPerception.FindOwner(Source);

        public override bool Validate()
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                    Debug.Arg(Owner?.MiniDebugName()),
                    Debug.Arg(nameof(SourceType), SourceType),
                });
            if (!base.Validate())
            {
                Debug.CheckNah(CallChain("base", nameof(Validate)), Indent: indent[1]);
                return false;
            }
            Debug.CheckYeh(CallChain("base", nameof(Validate)), Indent: indent[1]);
            if (Source == null)
            {
                Debug.CheckNah(nameof(Source), "null", Indent: indent[1]);
                return false;
            }
            Debug.CheckYeh(nameof(Source), Source, Indent: indent[1]);
            Debug.CheckYeh(nameof(Validate), Indent: indent[0]);
            return true;
        }
        /*
        => base.Validate()
            && Source != null;
        */
    }
}
