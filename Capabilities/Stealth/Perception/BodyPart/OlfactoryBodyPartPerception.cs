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
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using XRL.Liquids;
using XRL.World.Parts;
using XRL.Collections;
using XRL;
using StealthSystemPrototype.Detetection.Opinions;

namespace StealthSystemPrototype.Perceptions
{
    [HasModSensitiveStaticCache]
    [Serializable]
    public class OlfactoryBodyPartPerception
        : BaseBodyPartPerception
        , IOlfactoryPerception
    {
        #region Static Cache

        private static List<string> _DefaultInsensitiveLiquidTypeNames => new()
        {
            nameof(LiquidAcid),
            nameof(LiquidAsphalt),
            nameof(LiquidCider),
            nameof(LiquidGel),
            nameof(LiquidGoo),
            nameof(LiquidInk),
            nameof(LiquidLava),
            nameof(LiquidOil),
            nameof(LiquidOoze),
            nameof(LiquidPutrescence),
            nameof(LiquidSlime),
            nameof(LiquidSludge),
            nameof(LiquidWater),
            nameof(LiquidWax),
            nameof(LiquidWine),
        };

        [ModSensitiveStaticCache]
        public static List<string> DefaultInsensitiveLiquidTypeNames = null;

        public static string DefaultInsensitiveLiquids => LiquidVolume.Liquids
            ?.Aggregate(
                seed: "",
                func: delegate (string Accumulator, KeyValuePair<string, BaseLiquid> Next)
                {
                    if (Next.Value?.GetType()?.ToString() is string liquidTypeName
                        && DefaultInsensitiveLiquidTypeNames.Contains(liquidTypeName))
                    {
                        if (!Accumulator.IsNullOrEmpty())
                            Accumulator += ",";

                        Accumulator += Next.Value.ID;
                    }
                    return Accumulator;
                });

        [ModSensitiveCacheInit]
        public static void InitDefaultInsensitiveLiquids()
        {
            DefaultInsensitiveLiquidTypeNames = _DefaultInsensitiveLiquidTypeNames;
        }

        #endregion

        public override BasePurview Purview => new OlfactoryPurview(this);

        public virtual Type AlertType => typeof(Olfactory);

        private bool _AffectedByLiquidCovered = false;
        public virtual bool AffectedByLiquidCovered
        {
            get => _AffectedByLiquidCovered;
            protected set => _AffectedByLiquidCovered = value;
        }

        private bool _AffectedByLiquidStained = false;
        public virtual bool AffectedByLiquidStained
        {
            get => _AffectedByLiquidStained;
            protected set => _AffectedByLiquidStained = value;
        }

        private string _InsensitiveLiquids = null;
        public virtual string InsensitiveLiquids
        {
            get => _InsensitiveLiquids;
            protected set => _InsensitiveLiquids = value;
        }

        #region Constructors

        public OlfactoryBodyPartPerception()
            : base()
        {
        }
        public OlfactoryBodyPartPerception(
            GameObject Owner,
            BodyPart Source,
            int Level,
            bool AffectedByLiquidCovered,
            bool AffectedByLiquidStained,
            string InsensitiveLiquids,
            int? PurviewValue = null)
            : base(
                  Owner: Owner,
                  Source: Source,
                  Level: Level,
                  PurviewValue: PurviewValue)
        {
            this.AffectedByLiquidCovered = AffectedByLiquidCovered;
            this.AffectedByLiquidStained = AffectedByLiquidStained;
            this.InsensitiveLiquids = InsensitiveLiquids;
        }
        public OlfactoryBodyPartPerception(
            BodyPart Source,
            int Level,
            bool AffectedByLiquidCovered,
            bool AffectedByLiquidStained,
            string InsensitiveLiquids,
            int? PurviewValue = null)
            : this(
                  null,
                  Source: Source,
                  Level: Level,
                  AffectedByLiquidCovered: AffectedByLiquidCovered,
                  AffectedByLiquidStained: AffectedByLiquidStained,
                  InsensitiveLiquids: InsensitiveLiquids,
                  PurviewValue: PurviewValue)
        {
        }
        public OlfactoryBodyPartPerception(
            BodyPart Source,
            int Level,
            int? PurviewValue = null)
            : this(
                  Owner: null,
                  Source: Source,
                  Level: Level,
                  AffectedByLiquidCovered: true,
                  AffectedByLiquidStained: false,
                  InsensitiveLiquids: DefaultInsensitiveLiquids,
                  PurviewValue: PurviewValue)
        {
        }

        #endregion
        #region Serialization

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
            Writer.Write(AffectedByLiquidCovered);
            Writer.Write(AffectedByLiquidStained);
            Writer.WriteOptimized(InsensitiveLiquids);
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            AffectedByLiquidCovered = Reader.ReadBoolean();
            AffectedByLiquidStained = Reader.ReadBoolean();
            InsensitiveLiquids = Reader.ReadOptimizedString();
        }

        #endregion

        public override Type GetAlertType()
            => AlertType;

        public V GetTypedPurview<V>()
            where V : BasePurview<Olfactory>
            => Purview as V;

        public override BasePurview GetPurview()
            => Purview;

        public override void ConfigurePurview(int Value, Dictionary<string, object> args = null)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                });

            args ??= new();
            args[nameof(Value)] = Value;
            args[nameof(IDiffusingPurview.ConfigureDiffuser)] = new Dictionary<string, object>()
            {
                { nameof(BasePurview.ParentPerception), this },
                { nameof(IDiffusingPurview.Diffuser.SetSteps), Value },
            };

            args.ForEach(kvp => Debug.Log(kvp.Key, kvp.Value, Indent: indent[1]));

            base.ConfigurePurview(Value, args);
        }

        public override bool TryPerceive(AlertContext Context, out int SuccessMargin, out int FailureMargin)
            => base.TryPerceive(Context, out SuccessMargin, out FailureMargin);

        public override IOpinionDetection RaiseDetection(AlertContext Context, int SuccessMargin)
            => base.RaiseDetection(Context, SuccessMargin);

        public override BodyPart GetSource()
            => ((IBodyPartPerception)this).GetSource();

        public override bool Validate()
            => base.Validate()
            && Owner.Body?.GetFirstPart(SourceType, false) != null;
    }
}