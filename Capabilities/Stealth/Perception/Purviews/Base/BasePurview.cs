using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HarmonyLib;

using XRL.World;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.Capabilities.Stealth.DelayedLinearDoubleDiffuser;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    [StealthSystemBaseClass]
    [Serializable]
    public abstract class BasePurview
        : IPurview
        , IDisposable
    {
        #region Debug
        [UD_DebugRegistry]
        public static void doDebugRegistry(DebugMethodRegistry Registry)
        {
            Registry.RegisterEach(
                Type: typeof(StealthSystemPrototype.Capabilities.Stealth.Perception.BasePurview),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    { nameof(GetPurviewValueAdjustment), false },
                });
        }
        #endregion

        protected bool _Occludes = false;
        public virtual bool Occludes
        {
            get => _Occludes;
            protected set => _Occludes = value;
        }

        protected int _BaseValue = IPurview.DEFAULT_VALUE;
        public int BaseValue
        {
            get => _BaseValue;
            protected set => _BaseValue = value;
        }

        #region Constructors

        public BasePurview()
        { }
        public BasePurview(int BaseValue, bool Occludes)
            : this()
        {
            this.BaseValue = BaseValue;
            this.Occludes = Occludes;
        }
        public BasePurview(int BaseValue)
            : this(BaseValue, false)
        { }
        public BasePurview(BasePurview Source)
            : this(Source.BaseValue, Source.Occludes)
        { }

        #endregion
        #region Serialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteOptimized(BaseValue);
            Writer.Write(Occludes);
        }
        public virtual void Read(SerializationReader Reader)
        {
            BaseValue = Reader.ReadOptimizedInt32();
            Occludes = Reader.ReadBoolean();
        }

        #endregion

        public virtual int GetPurviewValueAdjustment(IPerception Perception)
            => AdjustTotalPurviewEvent.GetFor(Perception.Perceiver, Perception, this, BaseValue);

        public virtual bool CheckWithin(IPerception Perception, ref PerceptionSet.AlertEvent E)
            => GetEffectiveLevel(Perception, ref E) > 0;

        public abstract int GetEffectiveLevel(IPerception Perception, ref PerceptionSet.AlertEvent E);

        public virtual void Dispose()
        {
            _BaseValue = IPurview.DEFAULT_VALUE;
            _Occludes = false;
        }
    }
}
