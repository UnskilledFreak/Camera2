using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace Camera2.HarmonyPatches
{
    [HarmonyPatch]
    internal static class HookRoomAdjust
    {
        public static Vector3 Position { get; private set; }
        public static Quaternion Rotation { get; private set; }

        [UsedImplicitly]
        // ReSharper disable InconsistentNaming
        private static void Postfix(Vector3SO ____roomCenter, FloatSO ____roomRotation, MethodBase __originalMethod)
        {
            Position = ____roomCenter == null ? Vector3.zero : ____roomCenter;
            Rotation = Quaternion.Euler(____roomRotation == null ? Vector3.zero : new Vector3(0, ____roomRotation, 0));

#if DEBUG
            Plugin.Log.Warn("HookRoomAdjust.Postfix! " + __originalMethod.Name);
            Console.WriteLine("pos {0}, rot {1}", Position, Rotation);
#endif
        }

        [UsedImplicitly]
        private static void ApplyCustom(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;

#if DEBUG
            Plugin.Log.Warn("HookRoomAdjust.ApplyCustom!");
            Console.WriteLine("pos {0}, rot {1}", position, rotation);
#endif
        }

        [UsedImplicitly]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.OnEnable));
            yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.Start));
            yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.HandleRoomCenterDidChange));
            yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.HandleRoomRotationDidChange));
            yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.ResetRoom));
        }
    }
}