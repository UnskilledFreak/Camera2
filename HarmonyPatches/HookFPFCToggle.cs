using Camera2.Managers;
using HarmonyLib;
using IPA.Loader;
using System;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace Camera2.HarmonyPatches
{
    [HarmonyPatch]
    internal static class HookFPFCToggle
    {
        public static Transform fpfcTransform { get; private set; }
        public static bool isInFPFC => toggledIntoFPFC && fpfcTransform && fpfcTransform.gameObject && fpfcTransform.gameObject.activeInHierarchy;
        private static bool toggledIntoFPFC;

        private static bool foundSiraToggle { get; set; }
        private static PropertyInfo FIELD_SimpleCameraController_AllowInput;

        private static PluginMetadata SiraUtilSimpleCameraController = PluginManager.GetPluginFromId("SiraUtil");

        //TODO: remove next version
        public static readonly bool isSiraSettingLocalPostionYes = SiraUtilSimpleCameraController != null && SiraUtilSimpleCameraController.HVersion > new Hive.Versioning.Version("3.0.5");

        private static void SetFPFCActive(Transform transform, bool isActive = true)
        {
            fpfcTransform = transform;
            toggledIntoFPFC = isActive;

            ScenesManager.ActiveSceneChanged();
        }

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
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

        [UsedImplicitly]
        private static bool Prepare() => SiraUtilSimpleCameraController != null;

        [UsedImplicitly]
        private static MethodBase TargetMethod()
        {
            var x = SiraUtilSimpleCameraController.Assembly.GetType("SiraUtil.Tools.FPFC.SimpleCameraController");

            var y = x?.GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
            FIELD_SimpleCameraController_AllowInput = x?.GetProperty("AllowInput");

            foundSiraToggle = y != null && FIELD_SimpleCameraController_AllowInput != null;

            return foundSiraToggle ? y : null;
        }

        [HarmonyPatch(typeof(FirstPersonFlyingController), nameof(FirstPersonFlyingController.OnEnable))]
        public static class HookBasegameFPFC
        {
            [UsedImplicitly]
            // ReSharper disable once InconsistentNaming
            private static void Postfix(Transform ____camera)
            {
                if (!foundSiraToggle)
                {
                    SetFPFCActive(____camera.transform);
                }
            }
        }

        [UsedImplicitly]
        private static Exception Cleanup(Exception ex) => null;
    }
}