using XRL.World;

namespace StealthSystemPrototype.Events
{
    public interface ISneakEventHandler
        : IModEventHandler<BeforeSneakEvent>
        , IModEventHandler<GetSneakPerformanceEvent>
    {
    }
}
