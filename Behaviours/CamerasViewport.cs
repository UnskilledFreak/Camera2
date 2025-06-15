using Camera2.HarmonyPatches;
using Camera2.Managers;
using Camera2.Utils;
using System.Linq;
using Camera2.Enums;
using UnityEngine;

namespace Camera2.Behaviours
{
    internal class CamerasViewport : MonoBehaviour
    {
        private const float GrabberSize = 25;
        
        private static CameraDesktopView TargetCam { get; set; }
        
        private static Canvas canvas;
        private static readonly int[][] DeltaSchemes = {
            new[] { 1, 1, 1, 1 }, // Drag
            new[] { 0, 1, 1, 0 }, // Resize from bottom right
            new[] { 1, 0, 0, 1 }, // Resize from top left
            new[] { 0, 0, 1, 1 }, // Resize from top right
            new[] { 1, 1, 0, 0 } // Resize from bottom left
        };

        private Vector3 _lastMousePos;
        private Vector2 _mouseStartPos01;
        private Vector2 _lastScreenRes = Vector2.zero;
        private CamAction _possibleAction = CamAction.None;
        private CamAction _currentAction = CamAction.None;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);

            canvas = gameObject.AddComponent<Canvas>();
            // I know this logs a stupid warning because VR is active, no way to fix that it seems.
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        public CameraDesktopView AddNewView()
        {
            var img = new GameObject().AddComponent<CameraDesktopView>();

            img.transform.SetParent(gameObject.transform, true);

            return img;
        }

        private CameraDesktopView GetViewAtPoint(Vector2 point, ref CamAction actionAtPoint)
        {
            // This should already be sorted in the correct order
            foreach (var camScreen in GetComponentsInChildren<CameraDesktopView>(false).Reverse())
            {
                var d = new Rect(camScreen.Rect.position, camScreen.Rect.rect.size);

                if (d.Contains(point) && (!camScreen.Cam.Settings.IsScreenLocked || UI.CameraSettingsViewController.CurrentCam == camScreen.Cam))
                {
                    var relativeCursorPos = point - d.position;

                    if (relativeCursorPos.y <= GrabberSize && relativeCursorPos.x >= d.width - GrabberSize)
                    {
                        actionAtPoint = CamAction.ResizeBR;
                    }
                    else if (relativeCursorPos.y >= d.height - GrabberSize && relativeCursorPos.x >= d.width - GrabberSize)
                    {
                        actionAtPoint = CamAction.ResizeTR;
                    }
                    else if (relativeCursorPos.y >= d.height - GrabberSize && relativeCursorPos.x <= GrabberSize)
                    {
                        actionAtPoint = CamAction.ResizeTL;
                    }
                    else if (relativeCursorPos.y <= GrabberSize && relativeCursorPos.x <= GrabberSize)
                    {
                        actionAtPoint = CamAction.ResizeBL;
                    }
                    else
                    {
                        actionAtPoint = CamAction.Move;
                    }

                    return camScreen;
                }
            }
            
            return null;
        }

        public void CompleteReload()
        {
            Plugin.Log.Info("Reloading Camera2 Config...");
            MovementScriptManager.LoadMovementScripts(true);
            CamManager.Reload();
            Plugin.Log.Info("Reloading done");
        }

        public void Update()
        {
            if (Input.anyKeyDown)
            {
                //Some custom scenes to do funny stuff with
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
                    {
                        CompleteReload();
                    }
                    else
                    {
                        ScenesManager.LoadGameScene();
                    }
                }
                else
                {
                    foreach (var k in ScenesManager.Settings.CustomSceneBindings.Where(k => Input.GetKeyDown(k.Key)))
                    {
                        ScenesManager.SwitchToCustomScene(k.Value);
                        break;
                    }
                }
            }

            // This doesn't really belong here.....
            var curRes = new Vector2(Screen.width, Screen.height);
            if (_lastScreenRes != Vector2.zero)
            {
                foreach (var cam in CamManager.Cams)
                {
                    cam.UpdateRenderTextureAndView();
                }
            }

            _lastScreenRes = curRes;

            if (HookFPFCToggle.isInFPFC)
            {
                if ((int)_currentAction >= 2)
                {
                    ProcessCamAction(true);
                }

                TargetCam = null;
                return;
            }

            if (_currentAction == CamAction.None && _lastMousePos != Input.mousePosition)
            {
                if (!Application.isFocused)
                {
                    return;
                }

                _possibleAction = CamAction.None;
                _lastMousePos = Input.mousePosition;

                if (_lastMousePos.x < 0 || _lastMousePos.y < 0 || _lastMousePos.x > Screen.width || _lastMousePos.y > Screen.height)
                {
                    return;
                }

                var pCam = TargetCam;
                TargetCam = GetViewAtPoint(_lastMousePos, ref _possibleAction);

                if (TargetCam != pCam)
                {
                    if (TargetCam != null)
                    {
                        TargetCam.Cam.PrepareMiddleWareRender(true);
                    }

                    if (pCam != null)
                    {
                        pCam.Cam.PrepareMiddleWareRender(true);
                    }
                }

                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (_possibleAction)
                {
                    case CamAction.ResizeBR:
                    case CamAction.ResizeTL:
                        WinAPI.SetCursor(WindowsCursor.IdcSizeNwSe);
                        break;
                    case CamAction.ResizeBL:
                    case CamAction.ResizeTR:
                        WinAPI.SetCursor(WindowsCursor.IdcSizeNeSw);
                        break;
                }
            }

            if (_possibleAction == CamAction.None)
            {
                return;
            }

            // Drag handler / Resize
            if (Input.GetMouseButtonDown(0) && TargetCam != null && _currentAction == CamAction.None)
            {
                _mouseStartPos01 = _lastMousePos / new Vector2(Screen.width, Screen.height);
                _currentAction = _possibleAction;
            }

            if (_currentAction == CamAction.None)
            {
                return;
            }

            var released = !Input.GetMouseButton(0) || !TargetCam.isActiveAndEnabled;

            ProcessCamAction(released);
        }

        private void ProcessCamAction(bool finished)
        {
            var x = Input.mousePosition / new Vector2(Screen.width, Screen.height);

            if ((int)_currentAction >= 2)
            {
                TargetCam.SetPositionClamped(
                    // We take the current configured position and set the view position to it + the cursor move delta
                    x - _mouseStartPos01,
                    DeltaSchemes[(int)_currentAction - 2],
                    // And only when the button was released, save it to the config to make it the new config value
                    finished
                );
            }

            GL.Clear(true, true, Color.black);
            if (finished)
            {
                _currentAction = CamAction.None;
            }
        }
    }
}