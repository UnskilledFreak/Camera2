using Camera2.HarmonyPatches;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using VRUIControls;
using CameraType = Camera2.Enums.CameraType;

namespace Camera2.Behaviours
{
    internal class CamPositioner : MonoBehaviour
    {
        private static readonly FieldInfo VrPointerField = AccessTools.Field(typeof(VRInputModule), "_vrPointer");

        private static VRController controller;
        private static Cam2 grabbedCamera;
        private static Vector3 grabStartPos;
        private static Quaternion grabStartRot;
        private static bool isGrabbingCam;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public static void BeingDragCamera(Cam2 camera, VRInputModule inputModule)
        {
            if (isGrabbingCam)
            {
                FinishCameraMove();
            }

            //camera.LogDebug("pos: " + camera.Transformer.Position);
            //camera.LogDebug(camera.TransformChain.DebugChain());
            
            controller = GetPointingController(inputModule);

            if (controller == null)
            {
                return;
            }

#if DEBUG
            camera.LogInfo($"grabbed by {controller.name} at {controller.transform.position}, fpfc: {HookFPFCToggle.isInFPFC}");
#endif
            
            grabbedCamera = camera;
            isGrabbingCam = true;
            
            grabbedCamera.GetParentPose(out var parentPosition, out var parentRotation);

            grabStartPos = controller.transform.InverseTransformPoint(parentPosition + (parentRotation * grabbedCamera.Transformer.Position));
            grabStartRot = Quaternion.Inverse(controller.rotation) * parentRotation * grabbedCamera.Transformer.Rotation;
            
            grabbedCamera.WorldCam.SetPreviewPositionAndSize(false);
        }

        private static VRController GetPointingController(VRInputModule inputModule)
        {
            var pointer = VrPointerField?.GetValue(inputModule) as VRPointer;
            if (pointer == null)
            {
                return null;
            }

#if V1_29_1
            return pointer.vrController;
#else
            return pointer.lastSelectedVrController;
#endif
        }

        public void Update()
        {
            if (!isGrabbingCam)
            {
                return;
            }

            if (controller != null && grabbedCamera.WorldCam.isActiveAndEnabled)
            {
                grabbedCamera.GetParentPose(out var parentPosition, out var parentRotation);
                var toParentSpace = Quaternion.Inverse(parentRotation);

                var position = toParentSpace * (controller.transform.TransformPoint(grabStartPos) - parentPosition);
                var rotation = (toParentSpace * controller.rotation * grabStartRot).eulerAngles;

                grabbedCamera.Transformer.Position = position;                
                // do not update rotation on follower type, will result in weird behavior otherwise
                if (!grabbedCamera.Settings.IsFollowerCam())
                {
                    grabbedCamera.Transformer.Rotation = Quaternion.Euler(
                        Snap(rotation.x),
                        Snap(rotation.y),
                        Snap(rotation.z)
                    );
                }

                grabbedCamera.TransformChain.Calculate();

                if (controller.triggerValue > 0.5f || (HookFPFCToggle.isInFPFC && Input.GetMouseButton(0)))
                {
                    return;
                }
            }

            FinishCameraMove();
        }

        private static float Snap(float angle, float snap = 4f, float step = 45f)
        {
            var left = angle % step;

            if (left <= snap)
            {
                angle -= left;
            }
            else if (left >= step - snap)
            {
                angle += step - left;
            }

            return angle;
        }

        private static void FinishCameraMove()
        {
            if (!isGrabbingCam)
            {
                return;
            }
            
            grabbedCamera.Settings.TargetPosition = grabbedCamera.Transformer.Position;
            if (!grabbedCamera.Settings.IsFollowerCam())
            {
                grabbedCamera.Settings.TargetRotation = grabbedCamera.Transformer.Rotation.eulerAngles;
            }

            grabbedCamera.Settings.ApplyPositionAndRotation();

            grabbedCamera.WorldCam.SetPreviewPositionAndSize();

            grabbedCamera.Settings.Save();
            
            // null check is important! otherwise grabbing a cam without opening cam settings once will result in endless grab loop
            UI.SettingsFlowCoordinator.Instance?.ShowSettingsForCam(grabbedCamera, true);

            grabbedCamera = null;
            isGrabbingCam = false;
        }
    }
}