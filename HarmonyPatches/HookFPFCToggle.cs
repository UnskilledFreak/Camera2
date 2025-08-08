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
        public static Transform FpfcTransform { get; private set; }
        public static bool IsInFpfc
        {
            get
            {
                if (!FoundSiraToggle)
                {
                    // fallback if this crashes on pre 1_39
                    return FpfcHandler.Instance.IsActive;
                }
                return _toggledIntoFpfc && FpfcTransform && FpfcTransform.gameObject && FpfcTransform.gameObject.activeInHierarchy;
            }
        }

        private static bool _toggledIntoFpfc;

        private static bool FoundSiraToggle { get; set; }
        private static PropertyInfo _fieldSimpleCameraControllerAllowInput;

        private static readonly PluginMetadata SiraUtilSimpleCameraController = PluginManager.GetPluginFromId("SiraUtil");

        //TODO: remove next version
        public static readonly bool IsSiraSettingLocalPostionYes = SiraUtilSimpleCameraController != null && SiraUtilSimpleCameraController.HVersion > new Hive.Versioning.Version("3.0.5");

        private static void SetFpfcActive(Transform transform, bool isActive = true)
        {
            FpfcTransform = transform;
            _toggledIntoFpfc = isActive;

            ScenesManager.ActiveSceneChanged();
        }

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Postfix(MonoBehaviour __instance)
        {
            var allowInput = true;

            if (__instance.transform == FpfcTransform)
            {
                if (_fieldSimpleCameraControllerAllowInput != null)
                {
                    allowInput = (bool)_fieldSimpleCameraControllerAllowInput.GetValue(__instance);
                }

                if (allowInput == _toggledIntoFpfc)
                {
                    return;
                }
            }

#if DEBUG
            Plugin.Log.Info($"HookSiraFPFCToggle: SimpleCameraController.AllowInput => {allowInput}");
#endif
            SetFpfcActive(__instance.transform, allowInput);
        }

        [UsedImplicitly]
        private static bool Prepare() => SiraUtilSimpleCameraController != null;

        [UsedImplicitly]
        private static MethodBase TargetMethod()
        {
            var x = SiraUtilSimpleCameraController.Assembly.GetType("SiraUtil.Tools.FPFC.SimpleCameraController");
            var y = x?.GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
            if (y == null)
            {
                Plugin.Log.Warn("HookFPFCToggle: update method not found!");
            }
            _fieldSimpleCameraControllerAllowInput = x?.GetProperty("AllowInput");
            if (_fieldSimpleCameraControllerAllowInput == null)
            {
                Plugin.Log.Warn("HookFPFCToggle: FIELD_SimpleCameraController_AllowInput is null");
            }
            
            FoundSiraToggle = y != null && _fieldSimpleCameraControllerAllowInput != null;
            if (!FoundSiraToggle)
            {
                Plugin.Log.Warn("HookFPFCToggle: foundSiraToggle is false, Harmony will throw an exception now:");
            }
            return FoundSiraToggle ? y : null;
        }

        [HarmonyPatch(typeof(FirstPersonFlyingController), nameof(FirstPersonFlyingController.OnEnable))]
        public static class HookBasegameFPFC
        {
            [UsedImplicitly]
            // ReSharper disable once InconsistentNaming
            private static void Postfix(Transform ____camera)
            {
                if (!FoundSiraToggle)
                {
                    SetFpfcActive(____camera.transform);
                }
            }
        }

        [UsedImplicitly]
        private static Exception Cleanup(Exception ex) => null;
    }
}