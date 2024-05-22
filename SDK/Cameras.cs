using Camera2.Managers;
using System.Collections.Generic;
using System.Linq;
using Camera2.Enums;
using JetBrains.Annotations;

namespace Camera2.SDK
{
    public static class Cameras
    {
        /// <summary>
        /// names of all cameras available
        /// </summary>
        [UsedImplicitly]
        public static IEnumerable<string> Available => CamManager.Cams.Keys.AsEnumerable();

        /// <summary>
        /// Names of all cameras which are currently active
        /// </summary>
        [UsedImplicitly]
        public static IEnumerable<string> Active => CamManager.Cams.Values.Where(x => x.gameObject.activeSelf).Select(x => x.Name);

        /// <summary>
        /// Enables or disables a given camera
        /// </summary>
        /// <param name="cameraName">Name of the camera</param>
        /// <param name="active">true / false depending on if the camera should be active</param>
        [UsedImplicitly]
        public static void SetCameraActive(string cameraName, bool active = false)
        {
            CamManager.Cams[cameraName]?.gameObject.SetActive(active);
        }

        /// <summary>
        /// Returns the type of the camera
        /// </summary>
        /// <param name="cameraName">Name of the camera</param>
        /// <returns>Type of the camera, null if there is no camera with the requested name</returns>
        [UsedImplicitly]
        public static CameraType? GetCameraType(string cameraName)
        {
            return CamManager.Cams[cameraName]?.Settings.Type;
        }
    }
}