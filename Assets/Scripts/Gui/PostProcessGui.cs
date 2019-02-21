#region

using General;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;

//using UnityEngine.Rendering.PostProcessing;

#endregion

namespace Gui
{
    public class PostProcessGui : MonoBehaviour
    {
        private float _bloomIntensity;
        private float _bloomScatter;
        private float _dofFocusDistance;
        private float _lensDirtIntensity;
        private float _vignetteIntensity;
        private float _vignetteSmoothness;
        private float _ambientOcclusionIntensity;

        private Rect _windowRect;

        public Volume SceneVolume;
        [HideInInspector] public VolumeProfile Profile;
        private Bloom _bloom;
        private DepthOfField _depthOfField;
        private Vignette _vignette;
        private bool _enablePostProcess = true;
        private AmbientOcclusion _ambientOcclusion;

        private void Awake()
        {
            Profile = SceneVolume.profile;

            Profile.TryGet(out _bloom);
            _bloomIntensity = _bloom.intensity.value;

            _bloomScatter = _bloom.scatter.value;

            _lensDirtIntensity = _bloom.dirtIntensity.value;

            Profile.TryGet(out _depthOfField);

            _dofFocusDistance = _depthOfField ? _depthOfField.focusDistance.value : 0;

            Profile.TryGet(out _vignette);
            _vignetteIntensity = _vignette.intensity.value;

            _vignetteSmoothness = _vignette.smoothness.value;

            Profile.TryGet(out _ambientOcclusion);
            _ambientOcclusionIntensity = _ambientOcclusion.intensity.value;
            
            _windowRect = new Rect(10.0f, 265.0f, 300f, 540f);
        }

        private void UpdateValues()
        {
            _bloom.intensity.value = _bloomIntensity;
            _bloom.scatter.value = _bloomScatter;
            _bloom.dirtIntensity.value = _lensDirtIntensity;
            if (_depthOfField != null)
                _depthOfField.focusDistance.value = _dofFocusDistance;
            _vignette.intensity.value = _vignetteIntensity;
            _vignette.smoothness.value = _vignetteSmoothness;
            _ambientOcclusion.intensity.value = _ambientOcclusionIntensity;
        }

        public void PostProcessOn()
        {
            SceneVolume.enabled = true;
        }

        public void PostProcessOff()
        {
            SceneVolume.enabled = false;
        }

        public void TogglePostProcessGui()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        private void Update()
        {
            if (_enablePostProcess)
                PostProcessOn();
            else
                PostProcessOff();
        }

        private void DoMyWindow(int windowId)
        {
            const int offsetX = 10;
            var offsetY = 30;

            _enablePostProcess = GUI.Toggle(new Rect(offsetX, offsetY, 280, 30), _enablePostProcess,
                "Enable Post Process");
            offsetY += 40;


            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Bloom Scatter", _bloomScatter,
                out _bloomScatter, 0.0f, 1.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Bloom Intensity", _bloomIntensity,
                out _bloomIntensity, 0.0f, 1.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Lens Dirt Intensity", _lensDirtIntensity,
                out _lensDirtIntensity, 0.0f, 10.0f);
            offsetY += 60;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Vignette Intensity", _vignetteIntensity,
                out _vignetteIntensity, 0.0f, 1.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Vignette Smoothness", _vignetteSmoothness,
                out _vignetteSmoothness, 0.0f, 1.0f);
            offsetY += 60;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Ambient Occlusion Intensity",
                _ambientOcclusionIntensity,
                out _ambientOcclusionIntensity, 0.0f, 4.0f);
            offsetY += 60;

            if (_depthOfField)
                GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "DOF Focus Distance", _dofFocusDistance,
                    out _dofFocusDistance, 0.0f, 20.0f);

            offsetY += 50;

            if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Close")) gameObject.SetActive(false);

            UpdateValues();
            GUI.DragWindow();
        }

        private void OnGUI()
        {
            var pivotPoint = new Vector2(_windowRect.x, _windowRect.y);
            GUIUtility.ScaleAroundPivot(ProgramManager.Instance.GuiScale, pivotPoint);

            _windowRect = GUI.Window(19, _windowRect, DoMyWindow, "Post Process");
        }
    }
}