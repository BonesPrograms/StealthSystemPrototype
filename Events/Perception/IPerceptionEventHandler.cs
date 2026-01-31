using XRL.World;

namespace StealthSystemPrototype.Events
{
    public interface IPerceptionEventHandler
        : IModEventHandler<GetPerceptionsEvent>
        , IModEventHandler<AdjustTotalPerceptionLevelEvent>
        , IModEventHandler<AdjustTotalPurviewEvent>
    {
    }
}
