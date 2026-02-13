using XRL.World;

namespace StealthSystemPrototype.Events
{
    public interface ISneakPerformanceEventHandler
        : IModEventHandler<BeforeSneakEvent>
        , IModEventHandler<GetSneakPerformanceEvent>
        , IModEventHandler<GetSneakDetailsEvent>
        , IModEventHandler<GetActionAlertsEvent>
    {
    }
}
