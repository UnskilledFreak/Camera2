using UnityEngine;

namespace Camera2.Behaviours
{
    // When setting the parent of a camera this "shield" is placed in between so that, when the parent
    // is destroyed, the camera is "saved" by un-parenting before the destroying can cascade to it
    internal class ParentShield : MonoBehaviour
    {
        private Cam2 _cam;
        private bool _unParentOnDisable;

        public void Awake() => enabled = false;

        public void Init(Cam2 cam, Transform parent, bool worldPositionStays, bool unparentOnDisable = false)
        {
            _cam = cam;
            _unParentOnDisable = unparentOnDisable;

            transform.SetParent(parent, worldPositionStays);
        }

        public void OnDestroy() => _cam.SetOrigin(null);

        public void OnDisable()
        {
            if (_unParentOnDisable)
            {
                _cam.SetOrigin(null);
            }
        }
    }
}