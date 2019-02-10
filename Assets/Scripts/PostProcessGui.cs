#region

using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

#endregion

public class PostProcessGui : MonoBehaviour
{
    private float _bloomIntensity;
    private string _bloomIntensityText;

    private float _bloomThreshold;
    private string _bloomThresholdText;

    private float _dofFocalLength;
    private string _dofFocalLengthText;

    private float _dofFocusDistance;
    private string _dofFocusDistanceText;

    private float _lensDirtIntensity;
    private string _lensDirtIntensityText;

    private float _vignetteIntensity;
    private string _vignetteIntensityText;

    private float _vignetteSmoothness;
    private string _vignetteSmoothnessText;

    private float _ambientOcclusionIntensity;
    private string _ambientOcclusionIntensityText;

    private Rect _windowRect = new Rect(360, 330, 300, 530);

    public PostProcessVolume PostProcessVolume;
    [HideInInspector] public PostProcessProfile Profile;
    private Bloom _bloom;
    private DepthOfField _depthOfField;
    private Vignette _vignette;
    private bool _enablePostProcess = true;
    private AmbientOcclusion _ambientOcclusion;

    private void Awake()
    {
        Profile = PostProcessVolume.profile;

        _bloom = Profile.GetSetting<Bloom>();
        _bloomIntensity = _bloom.intensity.value;
        _bloomIntensityText = _bloomIntensity.ToString();

        _bloomThreshold = _bloom.threshold.value;
        _bloomThresholdText = _bloom.threshold.ToString();

        _lensDirtIntensity = _bloom.dirtIntensity.value;
        _lensDirtIntensityText = _lensDirtIntensity.ToString();

        _depthOfField = Profile.GetSetting<DepthOfField>();
        _dofFocalLength = _depthOfField.focalLength.value;
        _dofFocalLengthText = _dofFocalLength.ToString();

        _dofFocusDistance = _depthOfField.focusDistance.value;
        _dofFocusDistanceText = _dofFocusDistance.ToString();

        _vignette = Profile.GetSetting<Vignette>();
        _vignetteIntensity = _vignette.intensity.value;
        _vignetteIntensityText = _vignetteIntensity.ToString();

        _vignetteSmoothness = _vignette.smoothness.value;
        _vignetteSmoothnessText = _vignetteSmoothness.ToString();

        _ambientOcclusion = Profile.GetSetting<AmbientOcclusion>();
        _ambientOcclusionIntensity = _ambientOcclusion.intensity.value;
        _ambientOcclusionIntensityText = _ambientOcclusion.intensity.ToString();
    }

    private void UpdateValues()
    {
        _bloom.intensity.value = _bloomIntensity;
        _bloom.threshold.value = _bloomThreshold;
        _bloom.dirtIntensity.value = _lensDirtIntensity;
        _depthOfField.focalLength.value = _dofFocalLength;
        _depthOfField.focusDistance.value = _dofFocusDistance;
        _vignette.intensity.value = _vignetteIntensity;
        _vignette.smoothness.value = _vignetteSmoothness;
        _ambientOcclusion.intensity.value = _ambientOcclusionIntensity;
    }

    public void PostProcessOn()
    {
        PostProcessVolume.enabled = true;
    }

    public void PostProcessOff()
    {
        PostProcessVolume.enabled = false;
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

        _enablePostProcess = GUI.Toggle(new Rect(offsetX, offsetY, 280, 30), _enablePostProcess, "Enable Post Process");
        offsetY += 40;


        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Bloom Threshold", _bloomThreshold, _bloomThresholdText,
            out _bloomThreshold, out _bloomThresholdText, 0.0f, 2.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Bloom Intensity", _bloomIntensity, _bloomIntensityText,
            out _bloomIntensity, out _bloomIntensityText, 0.0f, 8.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Lens Dirt Intensity", _lensDirtIntensity,
            _lensDirtIntensityText,
            out _lensDirtIntensity, out _lensDirtIntensityText, 0.0f, 2.0f);
        offsetY += 60;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Vignette Intensity", _vignetteIntensity,
            _vignetteIntensityText,
            out _vignetteIntensity, out _vignetteIntensityText, 0.0f, 1.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Vignette Smoothness", _vignetteSmoothness,
            _vignetteSmoothnessText,
            out _vignetteSmoothness, out _vignetteSmoothnessText, 0.0f, 1.0f);
        offsetY += 60;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Ambient Occlusion Intensity", _ambientOcclusionIntensity,
            _ambientOcclusionIntensityText,
            out _ambientOcclusionIntensity, out _ambientOcclusionIntensityText, 0.0f, 10.0f);
        offsetY += 60;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "DOF Focal Length", _dofFocalLength, _dofFocalLengthText,
            out _dofFocalLength, out _dofFocalLengthText, 0.0f, 200.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "DOF Focus Distance", _dofFocusDistance,
            _dofFocusDistanceText,
            out _dofFocusDistance, out _dofFocusDistanceText, 0.0f, 50.0f);
        offsetY += 50;

        if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Close")) gameObject.SetActive(false);

        UpdateValues();
        GUI.DragWindow();
    }

    private void OnGUI()
    {
        _windowRect.width = 300;
        _windowRect.height = 510;

        _windowRect = GUI.Window(19, _windowRect, DoMyWindow, "Post Process");
    }
}