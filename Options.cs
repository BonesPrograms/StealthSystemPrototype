using XRL;

namespace StealthSystemPrototype
{
    [HasModSensitiveStaticCache]
    [HasOptionFlagUpdate(Prefix = "Option_Bones_PrototypeStealthSystem_")]
    public static class Options
    {
        // Debug Settings
        [OptionFlag] public static bool DebugEnableTestKit;
        [OptionFlag] public static bool DebugEnableLogging;
        [OptionFlag] public static bool DebugDisableWorldGenLogging;
        [OptionFlag] public static bool DebugEnableAllLogging;
    }
}
