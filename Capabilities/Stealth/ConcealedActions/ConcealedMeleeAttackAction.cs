using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype.Alerts;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public class ConcealedMeleeAttackAction : ConcealedMinAction<GetAttackerHitDiceEvent>
    {
        public static BaseAlert[] DefaultAlertTypes => new BaseAlert[]
        {
            new Kinesthetic(30),
            new Visual(15),
            new Auditory(12),
        };

        public override bool Aggressive => true;

        public ConcealedMeleeAttackAction(GetAttackerHitDiceEvent E, string Description)
            : base(E, true, Description)
        {
        }

        public override BaseConcealedAction Initialize()
        {
            Items = DefaultAlertTypes;
            return base.Initialize();
        }

        public override void Configure()
        {
            if (Event != null)
            {
                foreach (BaseAlert actionAlert in this)
                {
                    if (Event.Weapon?.GetTier() is int tier)
                        AdjustIntensityByWeaponTier(actionAlert, tier);
                }
            }
            base.Configure();
        }

        protected void AdjustIntensityByWeaponTier(BaseAlert Alert, int Tier)
        {
            if (DefaultAlertTypes.Any(a => a.IsSame(Alert)))
            Alert.AdjustIntensity(
                Amount: Tier switch
                {
                    8 => -2,
                    7 => 0,
                    6 or
                    5 => 2,
                    4 or
                    3 => 3,
                    2 => 4,
                    1 => 5,
                    _ => 6,
                });
        }
    }
}
