using System.Reflection;

namespace Camera2.Utils
{
    internal static class CustomNotesUtil
    {
#if PRE_1_40_6
        private static PropertyInfo layerUtilsHmdOnly;
#else
        private static PropertyInfo LayerUtilsHMDOnly;
#endif
        

        public static void Reflect()
        {
#if PRE_1_40_6
            layerUtilsHmdOnly = IPA.Loader.PluginManager.GetPluginFromId("Custom Notes")
                ?.Assembly
                .GetType("CustomNotes.Utilities.LayerUtils")
                ?.GetProperty("HMDOnly", BindingFlags.Public | BindingFlags.Static);
#else
            LayerUtilsHMDOnly = IPA.Loader.PluginManager.GetPluginFromId("CustomNotes")?
                .Assembly.GetType("CustomNotes.Utilities.Configuration")?
                .GetProperty("HmdOnlyEnabled", BindingFlags.Public | BindingFlags.Static);
#endif
        }

#if PRE_1_40_6
        public static bool HasHMDOnlyEnabled() => layerUtilsHmdOnly != null && (bool)layerUtilsHmdOnly.GetValue(null);
#else
        public static bool HasHMDOnlyEnabled() => LayerUtilsHMDOnly != null && (bool)LayerUtilsHMDOnly.GetValue(null);
#endif
        
    }
}