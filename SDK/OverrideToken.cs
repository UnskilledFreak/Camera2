﻿using Camera2.Behaviours;
using Camera2.Configuration;
using Camera2.Managers;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Camera2.SDK
{
    /// <summary>
    /// Used to temporarily override certain camera settings
    /// </summary>
    public class OverrideToken
    {

        private Cam2 _cam;
        private string CamName { get; set; }
        
        private static readonly Dictionary<string, OverrideToken> Tokens = new Dictionary<string, OverrideToken>();

        private float _fov;
        public Vector3 Position { get; [UsedImplicitly] set; }
        public Vector3 Rotation { get; [UsedImplicitly] set; }

        public GameObjects VisibleObjects { get; private set; }

        public float FOV
        {
            get => _fov;
            [UsedImplicitly]
            set
            {
                _fov = value;
                if (IsValid)
                {
                    _cam.Camera.fieldOfView = _fov;
                }
            }
        }

        /// <summary>
        /// Request an OverrideToken for the camera with the given name.
        /// If there is no camera with the passed name, or there is already
        /// an active OverrideToken for this camera null will be returned
        /// </summary>
        /// <param name="camName">the name of the Camera</param>
        /// <returns>OverrideToken instance if successful, null otherwise</returns>
        [UsedImplicitly]
        public static OverrideToken GetTokenForCamera(string camName)
        {
            if (Tokens.ContainsKey(camName))
            {
                return null;
            }

            var cam = CamManager.GetCameraByName(camName);
            if (cam == null)
            {
                return null;
            }

            var token = new OverrideToken(cam);

            // Maybe something worth adding to keep track of who's doing funny

            return Tokens[camName] = token;
        }

        [UsedImplicitly]
        internal static OverrideToken GetTokenForCamera(Cam2 cam) => GetTokenForCamera(cam.Name);

        /// <summary>
        /// Returns if the camera instance that this OverrideToken was created for still exists
        /// </summary>
        [UsedImplicitly]
        public bool IsValid => _cam != null && CamManager.GetCameraByName(CamName) != null;

        /// <summary>
        /// Closes this OverrideToken and returns the camera's values back to their default
        /// </summary>
        [UsedImplicitly]
        public void Close()
        {
            Tokens.Remove(CamName);

            if (IsValid)
            {
                _cam.Settings.OverrideToken = null;

                // Trigger setter for update
                _cam.Settings.FOV = _cam.Settings.FOV;
                _cam.Settings.ApplyPositionAndRotation();
                _cam.Settings.ApplyLayerBitmask();
            }

            _cam = null;
        }

        /// <summary>
        /// Applies the currently set position / rotation to the camera, 
        /// needs to be called for changes to have an effect
        /// </summary>
        [UsedImplicitly]
        public void UpdatePositionAndRotation()
        {
            if (IsValid)
            {
                _cam.Settings.ApplyPositionAndRotation();
            }
        }

        /// <summary>
        /// Applies the currently configured object visibilities,
        /// needs to be called for changes to have an effect
        /// </summary>
        [UsedImplicitly]
        public void UpdateVisibleObjects()
        {
            if (IsValid)
            {
                _cam.Settings.ApplyLayerBitmask();
            }
        }

        private OverrideToken(Cam2 cam)
        {
            _cam = cam;
            CamName = cam.Name;
            Position = new Vector3(cam.Settings.TargetPosition.x, cam.Settings.TargetPosition.y, cam.Settings.TargetPosition.z);
            Rotation = new Vector3(cam.Settings.TargetRotation.x, cam.Settings.TargetRotation.y, cam.Settings.TargetRotation.z);
            _fov = cam.Settings.FOV;
            VisibleObjects = cam.Settings.VisibleObjects.GetCopy();

            cam.Settings.OverrideToken = this;
        }
    }
}