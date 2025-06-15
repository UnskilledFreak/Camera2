using System.Collections.Generic;
using System.Reflection;
using Camera2.Utils;
using HarmonyLib;

namespace Camera2.HarmonyPatches
{
    [HarmonyPatch]
    internal static class HookLeveldata
    {
#if PRE_1_37_1
        public static IDifficultyBeatmap difficultyBeatmap;
        public static GameplayModifiers gameplayModifiers;
#else
        public static BeatmapLevel BeatmapLevel;
        public static GameplayModifiers GameplayModifiers;
#endif
        
        public static bool Is360Level = false;
        public static bool IsModdedMap = false;
        public static bool IsWallMap = false;

#if PRE_1_37_1
        [HarmonyPriority(int.MinValue)]
        [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), nameof(StandardLevelScenesTransitionSetupDataSO.Init))]
        [HarmonyPatch(typeof(MissionLevelScenesTransitionSetupDataSO), nameof(MissionLevelScenesTransitionSetupDataSO.Init))]
        [HarmonyPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), nameof(MultiplayerLevelScenesTransitionSetupDataSO.Init))]
        private static void Postfix(IDifficultyBeatmap difficultyBeatmap, GameplayModifiers gameplayModifiers)
        {
            HookLeveldata.difficultyBeatmap = difficultyBeatmap;
            HookLeveldata.gameplayModifiers = gameplayModifiers;

            IsModdedMap = ModMapUtil.IsModdedMap(difficultyBeatmap);
            Is360Level = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.containsRotationEvents;
            IsWallMap = ModMapUtil.IsProbablyWallMap(difficultyBeatmap);
        }
#else
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods() {
            foreach(var m in AccessTools.GetDeclaredMethods(typeof(StandardLevelScenesTransitionSetupDataSO)))
            {
                if(m.Name == nameof(StandardLevelScenesTransitionSetupDataSO.Init))
                {
                    yield return m;
                }
            }

            foreach(var m in AccessTools.GetDeclaredMethods(typeof(MissionLevelScenesTransitionSetupDataSO)))
            {
                if(m.Name == nameof(MissionLevelScenesTransitionSetupDataSO.Init))
                {
                    yield return m;
                }
            }

            yield return AccessTools.FirstMethod(
                typeof(MultiplayerLevelScenesTransitionSetupDataSO),
                x => x.Name == "Init"
            );
        }

        [HarmonyPostfix]
        private static void Postfix(BeatmapKey beatmapKey, BeatmapLevel beatmapLevel, GameplayModifiers gameplayModifiers) {
            HookLeveldata.BeatmapLevel = beatmapLevel;
            HookLeveldata.GameplayModifiers = gameplayModifiers;
#if PRE_1_40_6
            IsModdedMap = ModMapUtil.IsModdedMap(beatmapLevel, beatmapKey);
            Is360Level = beatmapKey.beatmapCharacteristic.containsRotationEvents;
            IsWallMap = ModMapUtil.IsProbablyWallMap(beatmapLevel, beatmapKey);
#endif
            IsModdedMap = ModMapUtil.IsModdedMap(beatmapKey);
            Is360Level = beatmapKey.beatmapCharacteristic.containsRotationEvents;
            IsWallMap = ModMapUtil.IsProbablyWallMap(beatmapKey);
        }
#endif
        internal static void Reset()
        {
            Is360Level = IsModdedMap = IsWallMap = false;
#if PRE_1_37_1
            difficultyBeatmap = null;
#else
            BeatmapLevel = null;
#endif

        }
    }
}