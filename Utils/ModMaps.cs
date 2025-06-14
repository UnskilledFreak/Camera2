using IPA.Loader;
using System.Linq;
using System.Xaml;

namespace Camera2.Utils
{
    internal static class ModMapUtil
    {
        private static readonly bool IsModCapable = PluginManager.EnabledPlugins.Any(x => x.Name == "MappingExtensions" || x.Name == "NoodleExtensions");
        private static readonly bool HasSongCore = PluginManager.EnabledPlugins.Any(x => x.Name == "SongCore"); // failsafe, Noodle / MapEx do require it themselves technically

        public static bool IsProbablyWallMap(IDifficultyBeatmap map) => IsModCapable && IsModdedMap(map);

        public static bool IsModdedMap(IDifficultyBeatmap map) => HasSongCore && IsModCapable && IsModdedMapFunc(map);
        /*
        public static bool HasPlayerTrack(IDifficultyBeatmap map)
        {
            if (!IsModdedMap(map))
            {
                return false;
            }

            var hash = SongCore.Collections.hashForLevelID(map.level.levelID);

            var test = SongCore.Loader.CustomLevels.FirstOrDefault(x => x.Value.levelID == difficultyBeatmap.level.levelID).Key;

            return true;
        }
        */

        // Separate method so we dont throw if theres no SongCore
        private static bool IsModdedMapFunc(IDifficultyBeatmap map)
        {
            try
            {
                var x = SongCore.Collections.RetrieveDifficultyData(map)
                    ?.additionalDifficultyData
                    ._requirements
                    .Any(x => x == "Mapping Extensions" || x == "Noodle Extensions") == true;
#if V1_29_1
                return x;
#else
                return map != null && x;
#endif
            }
            catch
            {
                return false;
            }
        }
    }
}