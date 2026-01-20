using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Events;

using XRL.World.Skills;

namespace XRL.World.Parts.Skill
{
    public class UD_Stealth_Shameless : BaseSkill, ISneakEventHandler
    {
        public override bool AddSkill(GameObject GO)
        {
            // something?
            return base.AddSkill(GO);
        }

        public override void Initialize()
        {
            base.Initialize();
            UD_Sneak.WantsSync(ParentObject)?.SyncAbility();
        }

        #region Event Handling

        public override bool AllowStaticRegistration()
            => true;

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            // do registrations here.
            base.Register(Object, Registrar);
        }
        public override bool WantEvent(int ID, int cascade)
            => base.WantEvent(ID, cascade)
            || ID == GetSneakPerformanceEvent.ID
            ;
        public virtual bool HandleEvent(GetSneakPerformanceEvent E)
        {
            if (E.Hider == ParentObject)
            {
                E.Performance.AdjustMoveSpeedMultiplier(this, 10);
                E.Performance.AdjustQuicknessMultiplier(this, 10);
            }

            return base.HandleEvent(E);
        }

        #endregion
    }
}
