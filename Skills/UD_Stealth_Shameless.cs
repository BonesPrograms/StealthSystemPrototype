using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Events;

using XRL.World.Skills;

namespace XRL.World.Parts.Skill
{
    public class UD_Stealth_Shameless : BaseSkill, ISneakEventHandler
    {

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register(GetSneakPerformanceEvent.ID, EventOrder.EXTREMELY_LATE);
            base.Register(Object, Registrar);
        }
        public override bool WantEvent(int ID, int cascade)
            => base.WantEvent(ID, cascade)
            // || ID == GetSneakPerformanceEvent.ID
            ;
        public virtual bool HandleEvent(GetSneakPerformanceEvent E)
        {
            if (E.Hider == ParentObject)
                E.Performance.AdjustMoveSpeedMultiplier(this, 10);
            return base.HandleEvent(E);
        }
    }
}
