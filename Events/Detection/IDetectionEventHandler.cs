using XRL.World;

namespace StealthSystemPrototype.Events
{
    public interface IDetectionEventHandler
        : IModEventHandler<BeforeDetectedEvent>
        , IModEventHandler<AfterDetectedEvent>
    {
    }
}
