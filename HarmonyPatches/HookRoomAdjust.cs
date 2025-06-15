using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
#if V1_37_1
using BeatSaber.GameSettings;
#endif
using JetBrains.Annotations;
using UnityEngine;

namespace Camera2.HarmonyPatches
{
    [HarmonyPatch]
    internal static class HookRoomAdjust
    {
        public static Vector3 Position { get; private set; }
        public static Quaternion Rotation { get; private set; }
#if !PRE_1_37_1
        public static Vector3 EulerAngles { get; private set; }
#endif

#if !V1_29_1
        [HarmonyPatch(typeof(VRCenterAdjust), nameof(VRCenterAdjust.OnEnable))]
        [HarmonyPatch(typeof(VRCenterAdjust), nameof(VRCenterAdjust.Start))]
        [HarmonyPatch(typeof(VRCenterAdjust), nameof(VRCenterAdjust.SetRoomTransformOffset))]
        [HarmonyPatch(typeof(VRCenterAdjust), nameof(VRCenterAdjust.ResetRoom))]
#endif
        [UsedImplicitly]
        // ReSharper disable InconsistentNaming
#if PRE_1_37_1
        private static void Postfix(Vector3SO ____roomCenter, FloatSO ____roomRotation, MethodBase __originalMethod)
        {
            Position = ____roomCenter == null ? Vector3.zero : ____roomCenter;
            Rotation = Quaternion.Euler(____roomRotation == null ? Vector3.zero : new Vector3(0, ____roomRotation, 0));
        }
#elif V1_37_1
        private static void Postfix(MainSettingsHandler ____mainSettingsHandler, MethodBase __originalMethod)
        {
            if(____mainSettingsHandler != null) {
                Position = ____mainSettingsHandler.instance.roomCenter;
                EulerAngles = new Vector3(0, ____mainSettingsHandler.instance.roomRotation, 0);
            } else {
                Position = Vector3.zero;
                EulerAngles = Vector3.zero;
            }

            Rotation = Quaternion.Euler(EulerAngles);
        }
#else
        private static void Postfix(SettingsManager ____settingsManager, MethodBase __originalMethod)
        {
            if (____settingsManager != null)
            {
                Position = ____settingsManager.settings.room.center;
                EulerAngles = new Vector3(0, ____settingsManager.settings.room.rotation, 0);
            }
            else
            {
                Position = Vector3.zero;
                EulerAngles = Vector3.zero;
            }

            Rotation = Quaternion.Euler(EulerAngles);
        }
#endif

        [UsedImplicitly]
        private static void ApplyCustom(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
#if V1_29_1
        [UsedImplicitly]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.OnEnable));
            yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.Start));
            yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.HandleRoomCenterDidChange));
            yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.HandleRoomRotationDidChange));
            yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.ResetRoom));
        }
#endif
    }
}