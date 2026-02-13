using XRL.World;

namespace StealthSystemPrototype.Events
{
    public interface IWitnessSneakEventHandler
        : IModEventHandler<ObjectStartedSneakingEvent>
        , IModEventHandler<ObjectIsSneakingEvent>
        , IModEventHandler<ObjectStoppedSneakingEvent>
        , IModEventHandler<TryConcealActionEvent>
    { }
}
