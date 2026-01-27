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
using StealthSystemPrototype.Detetections;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using XRL.Liquids;
using XRL.World.Parts;
using XRL.Collections;
using XRL;

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

        public virtual OlfactoryPurview Purview
        {
            get => _Purview as OlfactoryPurview;
            set => _Purview = value;
        }

        public virtual bool AffectedByLiquidCovered { get; protected set; }
        public virtual bool AffectedByLiquidStained { get; protected set; }
        public virtual string InsensitiveLiquids { get; protected set; }

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
            OlfactoryPurview Purview)
            : base(
                  Owner: Owner,
                  Source: Source,
                  Level: Level,
                  Purview: Purview)
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
            OlfactoryPurview Purview)
            : this(
                  null,
                  Source: Source,
                  Level: Level,
                  AffectedByLiquidCovered: AffectedByLiquidCovered,
                  AffectedByLiquidStained: AffectedByLiquidStained,
                  InsensitiveLiquids: InsensitiveLiquids,
                  Purview: Purview)
        {
        }
        public OlfactoryBodyPartPerception(
            BodyPart Source,
            int Level,
            OlfactoryPurview Purview)
            : this(
                  Owner: null,
                  Source: Source,
                  Level: Level,
                  AffectedByLiquidCovered: true,
                  AffectedByLiquidStained: false,
                  InsensitiveLiquids: DefaultInsensitiveLiquids,
                  Purview: Purview)
        {
        }
        public OlfactoryBodyPartPerception(
            GameObject Owner,
            BodyPart Source,
            int Level,
            bool AffectedByLiquidCovered,
            bool AffectedByLiquidStained,
            string InsensitiveLiquids,
            int Purview)
            : this(
                  Owner: Owner,
                  Source: Source,
                  Level: Level,
                  AffectedByLiquidCovered: AffectedByLiquidCovered,
                  AffectedByLiquidStained: AffectedByLiquidStained,
                  InsensitiveLiquids: InsensitiveLiquids,
                  Purview: new OlfactoryPurview(Purview))
        {
        }
        public OlfactoryBodyPartPerception(
            BodyPart Source,
            int Level,
            bool AffectedByLiquidCovered,
            bool AffectedByLiquidStained,
            string InsensitiveLiquids,
            int Purview)
            : this(
                  null,
                  Source: Source,
                  Level: Level,
                  AffectedByLiquidCovered: AffectedByLiquidCovered,
                  AffectedByLiquidStained: AffectedByLiquidStained,
                  InsensitiveLiquids: InsensitiveLiquids,
                  Purview: Purview)
        {
        }
        public OlfactoryBodyPartPerception(
            BodyPart Source,
            int Level,
            int Purview)
            : this(
                  Source: Source,
                  Level: Level,
                  Purview: new OlfactoryPurview(Purview))
        {
        }
        public OlfactoryBodyPartPerception(GameObject Basis, SerializationReader Reader)
            : base(Basis, Reader)
        {
        }

        #endregion
        #region Serialization

        public virtual void WritePurview(SerializationWriter Writer, OlfactoryPurview Purview)
            => Writer.Write(Purview);

        public sealed override void WritePurview(SerializationWriter Writer, IPurview Purview)
        {
            if (Purview is not OlfactoryPurview typedPurview)
                typedPurview = _Purview as OlfactoryPurview;

            WritePurview(Writer, typedPurview);

            if (typedPurview == null)
                MetricsManager.LogModWarning(
                    mod: ThisMod,
                    Message: GetType().ToStringWithGenerics() + " Failed to Serialize Write " +
                        Purview.TypeStringWithGenerics() + " from untyped " + nameof(WritePurview) + " override.");
        }

        public virtual void ReadPurview(
            SerializationReader Reader,
            ref OlfactoryPurview Purview,
            IAlertTypedPerception<Olfactory, OlfactoryPurview> ParentPerception = null)
            => Purview = new OlfactoryPurview(Reader, ParentPerception ?? this);

        public sealed override void ReadPurview(
            SerializationReader Reader,
            ref IPurview Purview,
            IPerception ParentPerception = null)
        {
            if (Purview is not OlfactoryPurview typedPurview)
                typedPurview = _Purview as OlfactoryPurview;

            if (typedPurview != null)
            {
                ReadPurview(Reader, ref typedPurview, ParentPerception as IAlertTypedPerception<Olfactory, OlfactoryPurview> ?? this);
                _Purview = typedPurview;
            }
            else
                MetricsManager.LogModWarning(
                    mod: ThisMod,
                    Message: GetType().ToStringWithGenerics() + " Failed to Read Serialzed " +
                        Purview.TypeStringWithGenerics() + " from untyped " + nameof(ReadPurview) + " override.");
        }

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

        public override void Construct()
        {
            base.Construct();
            AffectedByLiquidCovered = false;
            AffectedByLiquidStained = false;
            InsensitiveLiquids = null;
        }

        public override Type GetAlertType()
            => ((IAlertTypedPerception<Olfactory, OlfactoryPurview>)this).GetAlertType();

        public override void AssignDefaultPurview(int Value)
            => Purview = GetDefaultPurview(Value);

        public override IPurview GetDefaultPurview(int Value)
            => GetDefaultPurview(
                Value: Value,
                purviewArgs: new object[]
                {
                    OlfactoryPurview.DefaultDiffuser.SetSteps(Value),
                });

        public virtual OlfactoryPurview GetDefaultPurview(int Value, params object[] purviewArgs)
        {
            var purview = new OlfactoryPurview(this as IAlertTypedPerception<Olfactory, IPurview<Olfactory>>, Value);
            if (!purviewArgs.IsNullOrEmpty())
                foreach (object arg in purviewArgs)
                {
                    if (arg is BaseDoubleDiffuser diffuserArg)
                        purview.Diffuser = diffuserArg;
                }
            return purview;
        }

        public override bool CanPerceiveAlert(IAlert Alert)
            => ((IAlertTypedPerception<Visual, IPurview<Visual>>)this).CanPerceiveAlert(Alert);

        public override bool TryPerceive(AlertContext Context)
            => base.TryPerceive(Context);

        public override IOpinionDetection RaiseDetection(AlertContext Context)
            => base.RaiseDetection(Context);

        public override BodyPart GetSource()
            => ((IBodyPartPerception)this).GetSource();

        public override bool Validate()
            => base.Validate()
            && Owner.Body?.GetFirstPart(SourceType, false) != null;
    }
}