using IPA.Loader;
using System.Linq;

namespace Camera2.Utils
{
    internal static class ModMapUtil
    {
        private static readonly bool IsModCapable = PluginManager.EnabledPlugins.Any(x => x.Name == "MappingExtensions" || x.Name == "NoodleExtensions");
        private static readonly bool HasSongCore = PluginManager.EnabledPlugins.Any(x => x.Name == "SongCore"); // failsafe, Noodle / MapEx do require it themselves technically

        public static bool IsProbablyWallMap(IDifficultyBeatmap map) => IsModCapable && IsModdedMap(map);

        public static bool IsModdedMap(IDifficultyBeatmap map) => HasSongCore && IsModCapable && IsModdedMapFunc(map);

        // Separate method so we dont throw if theres no SongCore
        private static bool IsModdedMapFunc(IDifficultyBeatmap map)
        {
            try
            {
                return SongCore.Collections.RetrieveDifficultyData(map)
                    ?.additionalDifficultyData
                    ._requirements
                    .Any(x => x == "Mapping Extensions" || x == "Noodle Extensions") == true;
            }
            catch
            {
                return false;
            }
        }
    }
}