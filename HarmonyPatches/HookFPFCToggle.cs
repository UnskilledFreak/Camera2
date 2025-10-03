using Camera2.Managers;
using HarmonyLib;
using IPA.Loader;
using System;
using System.Reflection;
using Camera2.Handler;
using JetBrains.Annotations;
using UnityEngine;

namespace Camera2.HarmonyPatches
{
    [HarmonyPatch]
    internal static class HookFPFCToggle
    {
        public static Transform fpfcTransform { get; set; } = null;
        public static bool isInFPFC => toggledIntoFPFC && fpfcTransform && fpfcTransform.gameObject && fpfcTransform.gameObject.activeInHierarchy;
        public static bool toggledIntoFPFC = false;
        
#if V1_29_1
        private const string SiraVersion = "3.1.2";
#elif V1_34_2
        private const string SiraVersion = "3.1.6";
#elif V1_37_1
        private const string SiraVersion = "3.1.11";
#else
        private const string SiraVersion = "3.2.1";
#endif
        public static readonly bool isSiraSettingLocalPostionYes = SiraUtilSimpleCameraController != null && SiraUtilSimpleCameraController.HVersion > new Hive.Versioning.Version(SiraVersion);
        public static bool foundSiraToggle { get; private set; } = false;
        private static PropertyInfo FIELD_SimpleCameraController_AllowInput = null;

        public static void SetFPFCActive(Transform transform, bool isActive = true)
        {
            fpfcTransform = transform;
            toggledIntoFPFC = isActive;

            ScenesManager.ActiveSceneChanged();
        }
        
        [UsedImplicitly]
        private static void Postfix(MonoBehaviour __instance)
        {
            var allowInput = true;

            if (__instance.transform == fpfcTransform)
            {
                if (FIELD_SimpleCameraController_AllowInput != null)
                {
                    allowInput = (bool)FIELD_SimpleCameraController_AllowInput.GetValue(__instance);
                }

                if (allowInput == toggledIntoFPFC)
                {
                    return;
                }
            }

#if DEBUG
            Plugin.Log.Info($"HookSiraFPFCToggle: SimpleCameraController.AllowInput => {allowInput}");
#endif
            SetFPFCActive(__instance.transform, allowInput);
        }

        private static PluginMetadata SiraUtilSimpleCameraController = PluginManager.GetPluginFromId("SiraUtil");
        
        [UsedImplicitly]
        private static Exception Cleanup(Exception ex) => null;
        
        [UsedImplicitly]
        private static bool Prepare() => SiraUtilSimpleCameraController != null;

        [UsedImplicitly]
        private static MethodBase TargetMethod()
        {
            var x = SiraUtilSimpleCameraController.Assembly.GetType("SiraUtil.Tools.FPFC.SimpleCameraController");
            var y = x?.GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
            if (y == null)
            {
                Plugin.Log.Info("HookFPFCToggle: new SiraUtil version, next exception will be expected:");
            }
            FIELD_SimpleCameraController_AllowInput = x?.GetProperty("AllowInput");

            foundSiraToggle = y != null && FIELD_SimpleCameraController_AllowInput != null;

            return foundSiraToggle ? y : null;
        }

        [HarmonyPatch(typeof(FirstPersonFlyingController), nameof(FirstPersonFlyingController.OnEnable))]
        internal static class HookBasegameFPFC
        {
            [UsedImplicitly]
            private static void Postfix(Transform ____camera)
            {
                if (!foundSiraToggle)
                {
                    SetFPFCActive(____camera.transform);
                }
            }
        }
    }
}