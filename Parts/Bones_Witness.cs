using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Events.Perception;
using StealthSystemPrototype.Events.Witness;

using static StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace XRL.World.Parts
{
    [Serializable]
    public class Bones_Witness : IScribedPart, IModEventHandler<GetWitnessesEvent>
    {
        public Perception Perception;

        public Bones_Witness()
        {
            Perception = null;
        }

        public override void Attach()
        {
            Perception = new(ParentObject);
            base.Attach();
        }

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || ID == BeforeTakeActionEvent.ID
            || ID == GetWitnessesEvent.ID
            ;

        public override bool HandleEvent(BeforeTakeActionEvent E)
        {
            EmitMessage(Perception.ToString());
            return base.HandleEvent(E);
        }

        public bool HandleEvent(GetWitnessesEvent E)
        {
            if (E.Hider?.CurrentCell is Cell { InActiveZone: true } hiderCell
                && ParentObject.CurrentCell is Cell { InActiveZone: true } myCell
                && hiderCell.CosmeticDistanceto(myCell.Location) is int distance
                && Perception != null
                && Perception.Scores
                    ?.Values
                    ?.Aggregate(new List<int>(), delegate (List<int> Accumulator, PerceptionScore Next)
                    {
                        Accumulator.Add(Next.Radius);
                        return Accumulator;
                    }) is List<int> radii
                && distance.EqualsAny(radii.ToArray()))
                E.AddWitness(this);
                
            return base.HandleEvent(E);
        }
    }
}
