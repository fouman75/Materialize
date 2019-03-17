#region

using General;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;

//using UnityEngine.Rendering.PostProcessing;

#endregion

namespace Gui
{
    public class PostProcessGui : MonoBehaviour, IHideable
    {
        private AmbientOcclusion _ambientOcclusion;
        private float _ambientOcclusionIntensity;
        private Bloom _bloom;
        private float _bloomIntensity;
        private float _bloomScatter;
        private DepthOfField _depthOfField;
        private float _dofFocusFarFocusEnd;
        private float _dofFocusFarFocusStart;
        private float _dofFocusNearFocusEnd;
        private float _dofFocusNearFocusStart;
        private bool _enableBloom;
        private bool _enablePostProcess = true;
        private bool _enableVignette;
        private float _lensDirtIntensity;
        private Vignette _vignette;
        private float _vignetteIntensity;
        private float _vignetteSmoothness;
        private int _windowId;

        private Rect _windowRect;
        [HideInInspector] public VolumeProfile Profile;

        public Volume SceneVolume;

        public bool Hide { get; set; }

        private void Awake()
        {
            Profile = SceneVolume.profile;

            Profile.TryGet(out _bloom);
            _bloomIntensity = _bloom.intensity.value;

            _bloomScatter = _bloom.scatter.value;

            _lensDirtIntensity = _bloom.dirtIntensity.value;

            Profile.TryGet(out _depthOfField);

            if (_depthOfField)
            {
                _dofFocusNearFocusStart = _depthOfField.nearFocusStart;
                _dofFocusNearFocusEnd = _depthOfField.nearFocusEnd;

                _dofFocusFarFocusStart = _depthOfField.farFocusStart;
                _dofFocusFarFocusEnd = _depthOfField.farFocusEnd;
            }

            Profile.TryGet(out _vignette);
            _vignetteIntensity = _vignette.intensity.value;

            _vignetteSmoothness = _vignette.smoothness.value;

            Profile.TryGet(out _ambientOcclusion);
            _ambientOcclusionIntensity = _ambientOcclusion.intensity.value;

            _windowRect = new Rect(10.0f, 265.0f, 300f, 580f);
        }

        private void Start()
        {
            _windowId = ProgramManager.Instance.GetWindowId;
        }

        private void UpdateValues()
        {
            _bloom.intensity.value = _bloomIntensity;
            _bloom.scatter.value = _bloomScatter;
            _bloom.dirtIntensity.value = _lensDirtIntensity;
            if (_depthOfField)
            {
                _depthOfField.nearFocusStart.value = _dofFocusNearFocusStart;
                _depthOfField.nearFocusEnd.value = _dofFocusNearFocusEnd;

                _depthOfField.farFocusStart.value = _dofFocusFarFocusStart;
                _depthOfField.farFocusEnd.value = _dofFocusFarFocusEnd;
            }

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
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
            else
            {
                MainGui.Instance.CloseWindows();
                gameObject.SetActive(true);
            }
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
            var offsetY = 20;

            _enablePostProcess = GUI.Toggle(new Rect(offsetX, offsetY, 280, 30), _enablePostProcess,
                "Enable Post Process");
            offsetY += 25;

            _bloom.active = GUI.Toggle(new Rect(offsetX, offsetY, 280, 30), _bloom.active,
                "Enable Bloom");
            offsetY += 25;

            if (_bloom && _bloom.active)
            {
                GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Bloom Scatter", _bloomScatter,
                    out _bloomScatter, 0.0f, 1.0f);
                offsetY += 40;

                GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Bloom Intensity", _bloomIntensity,
                    out _bloomIntensity, 0.0f, 1.0f);
                offsetY += 45;

                GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Lens Dirt Intensity", _lensDirtIntensity,
                    out _lensDirtIntensity, 0.0f, 10.0f);
                offsetY += 50;
            }

            _vignette.active = GUI.Toggle(new Rect(offsetX, offsetY, 280, 30), _vignette.active,
                "Enable Vignette");
            offsetY += 25;

            if (_vignette && _vignette.active)
            {
                GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Vignette Intensity", _vignetteIntensity,
                    out _vignetteIntensity, 0.0f, 1.0f);
                offsetY += 40;

                GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Vignette Smoothness", _vignetteSmoothness,
                    out _vignetteSmoothness, 0.0f, 1.0f);
                offsetY += 50;
            }

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Ambient Occlusion Intensity",
                _ambientOcclusionIntensity,
                out _ambientOcclusionIntensity, 0.0f, 4.0f);
            offsetY += 50;

            _depthOfField.active = GUI.Toggle(new Rect(offsetX, offsetY, 280, 30), _depthOfField.active,
                "Enable Depth Of Field");
            offsetY += 25;

            if (_depthOfField && _depthOfField.active)
            {
                GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "DOF Near Focus Start", _dofFocusNearFocusStart,
                    out _dofFocusNearFocusStart, 0.0f, 50.0f);

                offsetY += 45;

                if (_dofFocusNearFocusStart > _dofFocusNearFocusEnd) _dofFocusNearFocusStart = _dofFocusNearFocusEnd;

                GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "DOF Near Focus End", _dofFocusNearFocusEnd,
                    out _dofFocusNearFocusEnd, 0.0f, 50.0f);

                offsetY += 40;

                GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "DOF Far Focus Start", _dofFocusFarFocusStart,
                    out _dofFocusFarFocusStart, 0.0f, 50.0f);

                offsetY += 45;

                if (_dofFocusFarFocusStart > _dofFocusFarFocusEnd) _dofFocusFarFocusStart = _dofFocusFarFocusEnd;

                GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "DOF Far Focus End", _dofFocusFarFocusEnd,
                    out _dofFocusFarFocusEnd, 0.0f, 50.0f);
            }

            UpdateValues();
            GUI.DragWindow();
        }

        private void OnGUI()
        {
            if (Hide) return;
            var rect = new Rect(_windowRect);
            if (!_bloom.active) rect.height -= 135;
            if (!_vignette.active) rect.height -= 90;
            if (!_depthOfField.active) rect.height -= 170;
            MainGui.MakeScaledWindow(rect, _windowId, DoMyWindow, "Post Process", 0.9f);
        }
    }
}