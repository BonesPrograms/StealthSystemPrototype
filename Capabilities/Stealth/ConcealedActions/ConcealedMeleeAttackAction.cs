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
            BaseAlert.GetAlert<Kinesthetic>(Intensity: 35,
                Properties: new()
                {
                    { "Pain", null }
                }),
            BaseAlert.GetAlert<Visual>(Intensity: 25),
            BaseAlert.GetAlert<Auditory>(Intensity: 25),
            BaseAlert.GetAlert<Psionic>(Intensity: 15,
                Properties: new()
                {
                    { "Intent", "Negative" },
                    { "Aggressive", null }
                }),
        };

        public override bool Aggressive => true;

        public ConcealedMeleeAttackAction(GetAttackerHitDiceEvent E, string Action, string Description)
            : base(E, Action, true, Description)
        {
        }

        public ConcealedMeleeAttackAction(GetAttackerHitDiceEvent E, string Description)
            : this(E, "attacking", Description)
        {
        }

        public override BaseConcealedAction Initialize()
        {
            Dictionary<string, BaseAlert> alertTypes = new();
            foreach (BaseAlert defaultAlert in DefaultAlertTypes)
            {
                if (alertTypes.ContainsKey(defaultAlert.Name)
                    && alertTypes[defaultAlert.Name] is BaseAlert existingAlert)
                {
                    existingAlert.Intensity = defaultAlert.Intensity;

                    existingAlert.Properties ??= new();
                    foreach ((string name, string value) in defaultAlert.Properties)
                    {
                        if (existingAlert.Properties.ContainsKey(name))
                            existingAlert.Properties[name] += "," + value;
                        else
                            existingAlert.Properties[name] = value;
                    }
                }
                else
                    alertTypes[defaultAlert.Name] = defaultAlert;
            }

            Items ??= new BaseAlert[0];
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i] is BaseAlert itemsAlert)
                    alertTypes[itemsAlert.Name] = itemsAlert;
            }
            Clear();
            AddRange(alertTypes.Values);
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
