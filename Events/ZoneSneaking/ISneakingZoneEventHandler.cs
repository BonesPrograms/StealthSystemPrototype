using XRL.World;

namespace StealthSystemPrototype.Events
{
    public interface ISneakingZoneEventHandler
        : IModEventHandler<GetZoneWitnessesEvent>
        , IModEventHandler<GetZoneSneakersEvent>
    {
    }
}
