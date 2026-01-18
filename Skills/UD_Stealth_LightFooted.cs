using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Events;

using XRL.World.Skills;

namespace XRL.World.Parts.Skill
{
    public class UD_Stealth_LightFooted : BaseSkill, ISneakEventHandler
    {
        public override void Initialize()
        {
            base.Initialize();
            UD_Sneak.SyncAbility(ParentObject);
        }

        public override bool AddSkill(GameObject GO)
        {
            GO.RequirePart<UD_Sneak>();
            return base.AddSkill(GO);
        }

        public override bool RemoveSkill(GameObject GO)
        {
            NeedPartSupportEvent.Send(GO, UD_Sneak.SUPPORT_TYPE, this);
            return base.AddSkill(GO);
        }

        public override bool WantEvent(int ID, int cascade)
            => base.WantEvent(ID, cascade)
            || ID == PartSupportEvent.ID
            || ID == GetSneakPerformanceEvent.ID
            ;
        public override bool HandleEvent(PartSupportEvent E)
        {
            if (E.Skip != this
                && E.Type == UD_Sneak.SUPPORT_TYPE)
            {
                return false;
            }
            return base.HandleEvent(E);
        }
        public virtual bool HandleEvent(GetSneakPerformanceEvent E)
        {
            if (E.Hider == ParentObject)
                E.Performance.AdjustMoveSpeedMultiplier(this, -10);
            return base.HandleEvent(E);
        }
    }
}
