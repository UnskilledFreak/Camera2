using Camera2.HarmonyPatches;
using System.Linq;
using Camera2.UI;
using UnityEngine;
using VRUIControls;
using CameraType = Camera2.Enums.CameraType;

namespace Camera2.Behaviours
{
    internal class CamPositioner : MonoBehaviour
    {
        private static VRController controller;
        private static Cam2 grabbedCamera;
        private static Vector3 grabStartPos;
        private static Quaternion grabStartRot;
        private static bool isGrabbingCam;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public static void BeingDragCamera(Cam2 camera)
        {
            if (isGrabbingCam)
            {
                FinishCameraMove();
            }

            var controllers = Resources.FindObjectsOfTypeAll<VRLaserPointer>();

            controller = (!HookFPFCToggle.isInFPFC
                    ? controllers.LastOrDefault(x => x.isActiveAndEnabled)
                    : controllers.LastOrDefault(x =>
                    {
                        var controllerTransform = x.transform;
                        return controllerTransform.eulerAngles + controllerTransform.position != Vector3.zero;
                    })
                )?.GetComponentInParent<VRController>();

            if (controller == null)
            {
                return;
            }

            grabbedCamera = camera;
            isGrabbingCam = true;
            
            grabStartPos = controller.transform.InverseTransformPoint(grabbedCamera.Transformer.Position);
            grabStartRot = Quaternion.Inverse(controller.rotation) * grabbedCamera.Transformer.Rotation;
            
            grabbedCamera.WorldCam.SetPreviewPositionAndSize(false);
        }

        public void Update()
        {
            if (!isGrabbingCam)
            {
                return;
            }

            if (controller != null && grabbedCamera.WorldCam.isActiveAndEnabled)
            {
                var position = controller.transform.TransformPoint(grabStartPos);
                var rotation = (controller.rotation * grabStartRot).eulerAngles;

                grabbedCamera.Transformer.Position = position;                
                // do not update rotation on follower type, will result in weird behavior otherwise
                if (grabbedCamera.Settings.Type != CameraType.Follower)
                {
                    Snap(ref rotation.x);
                    Snap(ref rotation.y);
                    Snap(ref rotation.z);
                    
                    grabbedCamera.Transformer.Rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
                }

                grabbedCamera.TransformChain.Calculate();

                if (controller.triggerValue > 0.5f || (HookFPFCToggle.isInFPFC && Input.GetMouseButton(0)))
                {
                    return;
                }
            }

            FinishCameraMove();
        }

        private static void Snap(ref float a, float snap = 4f, float step = 45f)
        {
            var l = a % step;

            if (l <= snap)
            {
                a -= l;
            }
            else if (l >= step - snap)
            {
                a += step - l;
            }
        }

        private static void FinishCameraMove()
        {
            if (!isGrabbingCam)
            {
                return;
            }
            
            grabbedCamera.Settings.TargetPos = grabbedCamera.Transformer.Position;
            grabbedCamera.Settings.TargetRot = grabbedCamera.Transformer.Rotation.eulerAngles;

            grabbedCamera.Settings.TargetRot -= new Vector3(180f, 180f, 180f);

            grabbedCamera.Settings.ApplyPositionAndRotation();

            grabbedCamera.WorldCam.SetPreviewPositionAndSize();

            grabbedCamera.Settings.Save();
            
            // null check is important! otherwise grabbing a cam without opening cam settings once will result in endless grab loop
            SettingsCoordinator.Instance?.ShowSettingsForCam(grabbedCamera, true);

            grabbedCamera = null;
            isGrabbingCam = false;
        }
    }
}