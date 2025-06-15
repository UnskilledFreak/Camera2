using IPA.Loader;
using System.Linq;

namespace Camera2.Utils
{
    internal static class ModMapUtil
    {
        private static readonly bool IsModCapable = PluginManager.EnabledPlugins.Any(x => x.Name == "MappingExtensions" || x.Name == "NoodleExtensions");
        private static readonly bool HasSongCore = PluginManager.EnabledPlugins.Any(x => x.Name == "SongCore"); // failsafe, Noodle / MapEx do require it themselves technically

#if PRE_1_37_1
        public static bool IsProbablyWallMap(IDifficultyBeatmap map) => IsModCapable && IsModdedMap(map);
#else
        public static bool IsProbablyWallMap(BeatmapLevel map, BeatmapKey beatmapKey) => IsModCapable && IsModdedMap(map, beatmapKey);
#endif

#if PRE_1_37_1
        public static bool IsModdedMap(IDifficultyBeatmap map) => HasSongCore && IsModCapable && IsModdedMapFunc(map);
#else
        public static bool IsModdedMap(BeatmapLevel map, BeatmapKey beatmapKey) => HasSongCore && IsModCapable && IsModdedMapFunc(map, beatmapKey);
#endif

        // Separate method so we dont throw if theres no SongCore
#if PRE_1_37_1
        private static bool IsModdedMapFunc(IDifficultyBeatmap map)
#else
        private static bool IsModdedMapFunc(BeatmapLevel map, BeatmapKey beatmapKey)
#endif
        {
            try
            {
#if PRE_1_37_1
                var x = SongCore.Collections.RetrieveDifficultyData(map)
#else
                var x = SongCore.Collections.RetrieveDifficultyData(map, beatmapKey)
#endif
                    ?.additionalDifficultyData
                    ._requirements
                    .Any(x => x == "Mapping Extensions" || x == "Noodle Extensions") == true;
#if PRE_1_37_1
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