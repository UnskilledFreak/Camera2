using Camera2.Utils;
using HarmonyLib;

namespace Camera2.HarmonyPatches
{
    [HarmonyPatch]
    internal static class HookLeveldata
    {
        public static IDifficultyBeatmap difficultyBeatmap;
        public static GameplayModifiers gameplayModifiers;
        public static bool Is360Level = false;
        public static bool IsModdedMap = false;
        public static bool IsWallMap = false;

        [HarmonyPriority(int.MinValue)]
        [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), nameof(StandardLevelScenesTransitionSetupDataSO.Init))]
        [HarmonyPatch(typeof(MissionLevelScenesTransitionSetupDataSO), nameof(MissionLevelScenesTransitionSetupDataSO.Init))]
        [HarmonyPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), nameof(MultiplayerLevelScenesTransitionSetupDataSO.Init))]
        private static void Postfix(IDifficultyBeatmap difficultyBeatmap, GameplayModifiers gameplayModifiers)
        {
#if DEBUG
            Plugin.Log.Info("Got level data!");
#endif
            HookLeveldata.difficultyBeatmap = difficultyBeatmap;
            HookLeveldata.gameplayModifiers = gameplayModifiers;

            IsModdedMap = ModMapUtil.IsModdedMap(difficultyBeatmap);
            Is360Level = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.containsRotationEvents;
            IsWallMap = ModMapUtil.IsProbablyWallMap(difficultyBeatmap);
        }

        internal static void Reset()
        {
            Is360Level = IsModdedMap = IsWallMap = false;
            difficultyBeatmap = null;
        }
    }
}