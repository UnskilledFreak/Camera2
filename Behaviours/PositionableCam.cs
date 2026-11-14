using System;
using UnityEngine;
using UnityEngine.EventSystems;
#if !PRE_1_40_8
using UnityEngine.Rendering;
#endif

namespace Camera2.Behaviours
{
    internal class PositionableCam : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        private Cam2 Cam { get; set; }

        private GameObject _camOrigin;
        private GameObject _camPreview;
        private Material _viewMaterial;

        //private static Material hoverMaterial = new Material(Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "HandleHologram"));
        //private static Material normalMaterial = new Material(Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "MenuShockwave"));
        private static Material hoverMaterial;
        private static Material normalMaterial;

        private MeshRenderer _renderer;

        public void Awake()
        {
            hoverMaterial ??= new Material(Shader.Find("Hidden/Internal-DepthNormalsTexture"));
#if PRE_1_40_8
            normalMaterial ??= new Material(Shader.Find("Standard")) { color = new Color(255, 255, 175, 0.7f) };
#else
            normalMaterial ??= CreateNormalMaterial();
#endif
            

            DontDestroyOnLoad(gameObject);
            
            _camOrigin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _camOrigin.transform.parent = transform;
            _camOrigin.transform.localScale = new Vector3(0.08f, 0.08f, 0.2f);
            _camOrigin.transform.localPosition = new Vector3(0, 0, -(_camOrigin.transform.localScale.x * .3f));
            _camOrigin.transform.localEulerAngles = new Vector3(90f, 0, 0);

            _renderer = _camOrigin.GetComponent<MeshRenderer>();
            _renderer.material = normalMaterial;

            _camPreview = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(_camPreview.GetComponent<Collider>());
            _camPreview.transform.parent = transform;

            _viewMaterial = new Material(Plugin.ShaderVolumetricBlit);
            _camPreview.GetComponent<MeshRenderer>().material = _viewMaterial;
        }
        
#if !PRE_1_40_8
        private static Material CreateNormalMaterial()
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                return new Material(Plugin.ShaderVolumetricBlit) { mainTexture = Texture2D.whiteTexture };
            }

            var material = new Material(shader) { color = new Color(1f, 1f, 1f, 0f) };
            material.SetInt("_SrcBlend", (int)BlendMode.One);
            material.SetInt("_DstBlend", (int)BlendMode.Zero);
            material.SetInt("_Cull", (int)CullMode.Back);
            material.SetInt("_ZWrite", 1);

            return material;
        }
#endif

        private void OnDestroy()
        {
            Destroy(_viewMaterial);
        }

        public void SetSource(Cam2 cam)
        {
            Cam = cam;
            _viewMaterial.SetTexture(MainTex, Cam.RenderTexture);
            SetPreviewPositionAndSize();
        }

        public void SetPreviewPositionAndSize(bool small = true)
        {
            var size = small ? Cam.Settings.PreviewScreenSize : Math.Min(Cam.Settings.PreviewScreenSize * 2f, 4);

            _camPreview.transform.localScale = Cam.Camera.aspect > 1f 
                ? new Vector3(size, size / Cam.Camera.aspect, 0) 
                : new Vector3(size * Cam.Camera.aspect, size, 0);

            _camOrigin.transform.localScale = Cam.Settings.WorldCamUnderScreen 
                ? new Vector3(0.08f, 0.08f, 0.2f) 
                : new Vector3(0.04f, 0.05f, 0.1f);

            _camPreview.transform.localPosition = new Vector3(
                0,
                Cam.Settings.WorldCamUnderScreen ? 0.15f + (_camPreview.transform.localScale.y / 2f) : 0,
                _camOrigin.transform.localPosition.z / 2
            );
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.currentInputModule is not VRUIControls.VRInputModule inputModule)
            {
                return;
            }

            CamPositioner.BeingDragCamera(Cam, inputModule);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _renderer.material = hoverMaterial;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _renderer.material = normalMaterial;
        }
    }
}