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
        
        public static bool IsModdedMap(IDifficultyBeatmap map) => HasSongCore && IsModCapable && IsModdedMapFunc(map);
#elif PRE_1_40_6
        public static bool IsProbablyWallMap(BeatmapLevel map, BeatmapKey beatmapKey) => IsModCapable && IsModdedMap(map, beatmapKey);
        
        public static bool IsModdedMap(BeatmapLevel map, BeatmapKey beatmapKey) => HasSongCore && IsModCapable && IsModdedMapFunc(map, beatmapKey);
#else
        public static bool IsProbablyWallMap(BeatmapKey beatmapKey) => IsModCapable && IsModdedMap(beatmapKey);
        
        public static bool IsModdedMap(BeatmapKey beatmapKey) => HasSongCore && IsModCapable && IsModdedMapFunc(beatmapKey);
#endif


        // Separate method so we dont throw if theres no SongCore
#if PRE_1_37_1
        private static bool IsModdedMapFunc(IDifficultyBeatmap map)
#elif PRE_1_40_6 
        private static bool IsModdedMapFunc(BeatmapLevel map, BeatmapKey beatmapKey)
#else
        private static bool IsModdedMapFunc(BeatmapKey beatmapKey)
#endif
        {
            try
            {
#if PRE_1_37_1
                return SongCore.Collections.RetrieveDifficultyData(map)
                    ?.additionalDifficultyData
                    ._requirements
                    .Any(x => x == "Mapping Extensions" || x == "Noodle Extensions") == true;
#elif PRE_1_40_6
                return map != null && SongCore.Collections.RetrieveDifficultyData(map, beatmapKey)
                    ?.additionalDifficultyData
                    ._requirements
                    .Any(x => x == "Mapping Extensions" || x == "Noodle Extensions") == true;
#else
                return SongCore.Collections.GetCustomLevelSongDifficultyData(beatmapKey)
                    ?.additionalDifficultyData
                    ._requirements
                    .Any(x => x == "Mapping Extensions" || x == "Noodle Extensions") == true;
#endif
            }
            catch
            {
                return false;
            }
        }
    }
}