using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Perceptions;

using XRL.Rules;
using XRL.World;
using XRL.World.AI.Pathfinding;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public class ISense<TSense> : ISense
        where TSense : ISense<TSense>, new()
    {
        public ISense()
            : base()
        {
        }
        public ISense(int Intensity)
            : base(Intensity)
        {
        }
        public ISense(ISense Source)
            : base(Source)
        {
        }

        public override double GetIntensity()
            => base.GetIntensity();

        public override bool CanSense(IPerception Perception, GameObject Entity)
            => base.CanSense(Perception, Entity);

        public override AwarenessLevel CalculateAwareness<T>(SenseContext<T> Context)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Context.Perception), Context?.Perception?.ToString(Short: true) ?? "null"),
                    Debug.Arg(nameof(Context.Entity), Context?.Entity?.DebugName ?? "null"),
                });

            if (!Context.InRadius)
                return 0;

            int roll = Context.Roll;
            DieRoll dieRoll = Context.Perception.DieRoll.GetDieRoll();
            Radius radius = Context.Radius;

            double diffusion = Context.Diffusion;

            roll = (int)Math.Floor(roll * diffusion);

            Debug.Log(nameof(Context.Roll), Context.Roll, Indent: indent[1]);
            Debug.Log(nameof(Context.Distance), Context.Distance, Indent: indent[1]);

            string diffussesString = Context.Perception.Diffuses.ToString() + ", " + Context.Diffusion.WithDigits(3);
            string diffusionCountString = Context.Distance.Clamp(new(radius.GetValue())) + "/" + (Context.Diffusions?.Length ?? 0);
            Debug.Log(nameof(Context.Perception.Diffuses), diffussesString + " (" + diffusionCountString + ")", Indent: indent[1]);
            Debug.Log(radius.GetDiffusionDebug(Inline: false), Indent: indent[2]);

            Debug.Log(nameof(dieRoll), dieRoll, Indent: indent[1]);
        }

        public override AwarenessLevel Sense<T>(SenseContext<T> Context)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Context.Perception), Context?.Perception?.ToString(Short: true) ?? "null"),
                    Debug.Arg(nameof(Context.Entity), Context?.Entity?.DebugName ?? "null"),
                });

            if (Context == null)
                return AwarenessLevel.None;

            if (!CanSense(Context.Perception, Context.Entity))
                return AwarenessLevel.None;

            return CalculateAwareness(Context);
        }

        public override bool TrySense(SenseContext Context)
            => base.TrySense(SenseContext Context);

        protected override ISense Copy()
            => new ISense<TSense>(this);
            

        public override void Write(SerializationWriter Writer)
        {
        }
        public override void Read(SerializationReader Reader)
        {
        }
    }
}
