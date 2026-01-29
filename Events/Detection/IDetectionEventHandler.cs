using XRL.World;

namespace StealthSystemPrototype.Events
{
    public interface IDetectionEventHandler
        : IModEventHandler<GetDetectionOpinionEvent>
        , IModEventHandler<BeforeDetectedEvent>
        , IModEventHandler<AfterDetectedEvent>
    {
    }
}
