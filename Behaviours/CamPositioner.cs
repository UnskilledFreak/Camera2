﻿using Camera2.HarmonyPatches;
using System.Linq;
using Camera2.Utils;
using UnityEngine;
using VRUIControls;
using CameraType = Camera2.Enums.CameraType;

namespace Camera2.Behaviours
{
    internal class CamPositioner : MonoBehaviour
    {
        private static VRController controller;
        private static Cam2 grabbedCamera;
        private static Transform camTransform;
        private static Vector3 grabStartPos;
        private static Quaternion grabStartRot;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public static void BeingDragCamera(Cam2 camera)
        {
            if (grabbedCamera != null)
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

            //TODO: I should probably move this to use a Transformer...
            grabbedCamera = camera;
            camTransform = camera.Camera.transform;

            grabStartPos = controller.transform.InverseTransformPoint(camTransform.position);
            grabStartRot = Quaternion.Inverse(controller.rotation) * camTransform.rotation;

            grabbedCamera.WorldCam.SetPreviewPositionAndSize(false);
        }

        public void Update()
        {
            if (grabbedCamera == null)
            {
                return;
            }

            if (controller != null && grabbedCamera.WorldCam.isActiveAndEnabled)
            {
                var p = controller.transform.TransformPoint(grabStartPos);
                var r = (controller.rotation * grabStartRot).eulerAngles;

                //grabbedCamera.transformchain.BacktrackTo(grabbedCamera.transformer, ref p, ref r);

                grabbedCamera.Transformer.Position = p;                
                // do not update rotation on follower type cam
                if (grabbedCamera.Settings.Type != CameraType.Follower)
                {
                    //grabbedCamera.transformer.rotation = r;
                    Snap(ref r.x);
                    Snap(ref r.y);
                    Snap(ref r.z);
                    
                    grabbedCamera.Transformer.Rotation = Quaternion.Euler(r.x, r.y, r.z);
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
            if (grabbedCamera == null)
            {
                return;
            }
            
            grabbedCamera.Settings.TargetPos = grabbedCamera.Transformer.Position;
            grabbedCamera.Settings.TargetRot = grabbedCamera.Transformer.Rotation.eulerAngles;

            grabbedCamera.Settings.ApplyPositionAndRotation();

            grabbedCamera.WorldCam.SetPreviewPositionAndSize();

            grabbedCamera.Settings.Save();

            grabbedCamera = null;
        }
    }
}