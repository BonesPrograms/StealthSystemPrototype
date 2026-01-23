using XRL.World;

namespace StealthSystemPrototype.Events
{
    public interface IAlertEventHandler
        : IModEventHandler<BeforeAlertEvent>
        , IModEventHandler<AfterAlertEvent>
    {
    }
}
