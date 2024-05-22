using System.Reflection;

namespace Camera2.Utils
{
    internal static class CustomNotesUtil
    {
        private static PropertyInfo layerUtilsHmdOnly;

        public static void Reflect()
        {
            layerUtilsHmdOnly = IPA.Loader.PluginManager.GetPluginFromId("Custom Notes")
                ?.Assembly
                .GetType("CustomNotes.Utilities.LayerUtils")
                ?.GetProperty("HMDOnly", BindingFlags.Public | BindingFlags.Static);
        }

        public static bool HasHMDOnlyEnabled() => layerUtilsHmdOnly != null && (bool)layerUtilsHmdOnly.GetValue(null);
    }
}