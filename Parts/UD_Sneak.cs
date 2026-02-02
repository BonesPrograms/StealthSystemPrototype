using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Wish;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Parts.Skill;

using SerializeField = UnityEngine.SerializeField;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Capabilities.Stealth.Sneak;
using StealthSystemPrototype.Detetection.Opinions;
using StealthSystemPrototype.Alerts;
using XRL.World.AI;

namespace XRL.World.Parts
{
    [HasWishCommand]
    [Serializable]
    public class UD_Sneak : IScribedPart, ISneakSource
    {
        public static string SUPPORT_TYPE => nameof(UD_Sneak);
        public static string COMMAND_SNEAK => "CommandToggleSneaking";

        protected SneakPerformance _SneakPerformance;
        public SneakPerformance SneakPerformance
        {
            get
            {
                if (_SneakPerformance.IsNullOrEmpty()
                    || _SneakPerformance.WantSync)
                {
                    SyncSneakPerformance();
                }
                return _SneakPerformance;
            }
            protected set => _SneakPerformance = value;
        }
        protected bool CollectingSneakPerformance;

        [SerializeField]
        private Guid _SneakActivatedAbilityID;
        public Guid SneakActivatedAbilityID => _SneakActivatedAbilityID;

        Guid ISneakSource.SneakActivatedAbilityID
        { 
            get => SneakActivatedAbilityID;
            set => _SneakActivatedAbilityID = value;
        }

        public string SneakActivatedAbilityClass => "Skill";

        public int BaseSneakPerformance => ParentObject.StatMod("Intelligence");

        protected List<GameObject> _Witnesses;

        protected List<GameObject> Witnesses => _Witnesses ??= The.ActiveZone.GetObjects(GO => GO.WithinAnyPurview(ParentObject));

        public bool IsBeingPerceived
            => !IsSneaking() // non-sneakers are always perceptable in this system
            || (Witnesses?.Aggregate( // looping over the witness list
                seed: false, // return a bool, start with false (if it's empty, then there are no witness to perceive the sneaker)
                func: (a, n) // (bool a)ccumulator, (GameObject n)ext; a is the returned value (seed) from last iteration, n is the next (current) GameObject
                    => n.Brain is Brain brain // make sure there's a brain to have opinions in
                    && brain.TryGetOpinions(ParentObject, out OpinionList opinions) // get opinions on the sneaker
                    && !opinions // true when all detection opinions about the sneaker are not below "aware"
                        .Where(o => o is IOpinionDetection) // only detection opinions
                        .Select(o => o as IOpinionDetection) // cast them
                        .All(o => o.Level < AwarenessLevel.Aware) // all of them are below "aware"
                    || a) // if any are true, then the output seed is true.
                ?? false); // null coalesce to false if the witness list is null.
        // bit more on func, above: it's a Func<bool, GameObject, out bool> where the first parameter is given the seed on the first
        // iteration and each return value for every subsequent one. The GameObject parameter is each object in the list.
        // Come chat to me (UnderDoug) about it if understanding is still eluding you.

        [SerializeField]
        private bool _WantRecalc;
        public bool WantRecalc
        {
            get => _WantRecalc;
            set => _WantRecalc = value;
        }

        bool ISneakSource.IsSneaking => IsSneaking();

        public UD_Sneak()
        {
            SneakPerformance = null;
            _SneakActivatedAbilityID = Guid.Empty;
            WantRecalc = false;
        }

        #region Serialization

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
            Writer.WriteComposite(SneakPerformance);
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            SneakPerformance = Reader.ReadComposite<SneakPerformance>();
        }

        #endregion

        public override void Remove()
        {
            RemoveMyActivatedAbility(ref _SneakActivatedAbilityID);
            base.Remove();
        }

        public UD_Sneak WantsSync()
        {
            SneakPerformance.WantSync = true;
            WantRecalc = true;
            return this;
        }
        public static UD_Sneak WantsSync(GameObject Who)
            => Who?.GetPart<UD_Sneak>()?.WantsSync();

        public void SyncSneakPerformance()
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(ParentObject?.DebugName ?? "null"),
                });

            if (!CollectingSneakPerformance)
            {
                CollectingSneakPerformance.Toggle();

                GetSneakPerformanceEvent.GetFor(ParentObject, ref _SneakPerformance);

                CollectingSneakPerformance.Toggle();
            }
            PerformRecalc();
        }

        public void PerformRecalc()
        {
            if (WantRecalc)
            {
                ParentObject.ForeachEffect((UD_Sneaking fx) => fx.RecalcStatMultipliers());
                WantRecalc = false;
            }
        }

        public UD_Sneak SyncAbility(bool Silent = false)
        {
            PerformRecalc();

            bool removed = false;
            if (ParentObject.GetActivatedAbilityByCommand(COMMAND_SNEAK) is ActivatedAbilityEntry abilityEntry
                && abilityEntry.ID != SneakActivatedAbilityID)
            {
                removed = RemoveMyActivatedAbility(ref _SneakActivatedAbilityID) || removed;
                removed = RemoveMyActivatedAbility(ref abilityEntry.ID) || removed;

                ParentObject.RemoveAllEffects<UD_Sneaking>();
            }

            if (SneakActivatedAbilityID == Guid.Empty)
            {
                _SneakActivatedAbilityID = AddMyActivatedAbility(
                    Name: "Sneak",
                    Command: COMMAND_SNEAK,
                    Class: "Maneuvers",
                    Description: null, // write one into the xml files. Remove this comment when done.
                    Icon: "\u001a",
                    Toggleable: true,
                    ActiveToggle: true,
                    Silent: Silent || removed);
            }
            return this;
        }
        public static UD_Sneak SyncAbility(GameObject Who, bool Silent = false)
            => Who?.GetPart<UD_Sneak>()?.SyncAbility(Silent);

        public bool IsSneaking()
            => ParentObject.HasEffect<UD_Sneaking>();

        public bool StartSneaking()
            => !IsSneaking()
            && ParentObject.CheckFrozen()
            && ParentObject.CanChangeMovementMode("sneak", ShowMessage: true)
            && ParentObject.CheckNotOnWorldMap("sneak", ShowMessage: true)
            && ParentObject.ApplyEffect(new UD_Sneaking())
            && ToggleMyActivatedAbility(SneakActivatedAbilityID, SetState: true);
            // CooldownMyActivatedAbility(ActivatedAbilityID, 100, null, "Intelligence");

        public bool StopSneaking()
            => IsSneaking()
            && ParentObject.RemoveAllEffects<UD_Sneaking>() > 0
            && ToggleMyActivatedAbility(SneakActivatedAbilityID, SetState: false);

        public bool ToggleSneaking()
            => !IsSneaking()
            ? StartSneaking()
            : StopSneaking();

        #region Event Handling

        public override bool AllowStaticRegistration()
            => true;

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || ID == NeedPartSupportEvent.ID
            || ID == EndTurnEvent.ID
            || ID == CommandEvent.ID
            || ID == GetSneakPerformanceEvent.ID
            ;
        public override bool HandleEvent(NeedPartSupportEvent E)
        {
            if (E.Type == SUPPORT_TYPE
                && !PartSupportEvent.Check(E, this))
                ParentObject.RemovePart(this);

            return base.HandleEvent(E);
        }
        public override bool HandleEvent(EndTurnEvent E)
        {
            _Witnesses = null;
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(CommandEvent E)
        {
            if (E.Command == COMMAND_SNEAK
                && !ToggleSneaking())
                return false;

            return base.HandleEvent(E);
        }
        public virtual bool HandleEvent(GetSneakPerformanceEvent E)
        {
            if (E.Hider == ParentObject
                && !E.Hider.HasSkill(nameof(UD_Stealth_LightFooted)))
            {
                E.AdjustMoveSpeedMultiplier(this, -10);
                E.AdjustQuicknessMultiplier(this, -10);
            }
            return base.HandleEvent(E);
        }

        #endregion
    }
}
