#region

using System;
using System.Collections;
using General;
using Settings;
using UnityEngine;

#endregion

// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

namespace Gui
{
    public class SmoothnessGui : MonoBehaviour, IProcessor, IHideable
    {
        public bool Active
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }

        private static readonly int MetalSmoothness = Shader.PropertyToID("_MetalSmoothness");
        private static readonly int IsolateSample1 = Shader.PropertyToID("_IsolateSample1");
        private static readonly int UseSample1 = Shader.PropertyToID("_UseSample1");
        private static readonly int SampleColor1 = Shader.PropertyToID("_SampleColor1");
        private static readonly int SampleUv1 = Shader.PropertyToID("_SampleUV1");
        private static readonly int HueWeight1 = Shader.PropertyToID("_HueWeight1");
        private static readonly int SatWeight1 = Shader.PropertyToID("_SatWeight1");
        private static readonly int LumWeight1 = Shader.PropertyToID("_LumWeight1");
        private static readonly int MaskLow1 = Shader.PropertyToID("_MaskLow1");
        private static readonly int MaskHigh1 = Shader.PropertyToID("_MaskHigh1");
        private static readonly int Sample1Smoothness = Shader.PropertyToID("_Sample1Smoothness");
        private static readonly int IsolateSample2 = Shader.PropertyToID("_IsolateSample2");
        private static readonly int UseSample2 = Shader.PropertyToID("_UseSample2");
        private static readonly int SampleColor2 = Shader.PropertyToID("_SampleColor2");
        private static readonly int SampleUv2 = Shader.PropertyToID("_SampleUV2");
        private static readonly int HueWeight2 = Shader.PropertyToID("_HueWeight2");
        private static readonly int SatWeight2 = Shader.PropertyToID("_SatWeight2");
        private static readonly int LumWeight2 = Shader.PropertyToID("_LumWeight2");
        private static readonly int MaskLow2 = Shader.PropertyToID("_MaskLow2");
        private static readonly int MaskHigh2 = Shader.PropertyToID("_MaskHigh2");
        private static readonly int Sample2Smoothness = Shader.PropertyToID("_Sample2Smoothness");
        private static readonly int IsolateSample3 = Shader.PropertyToID("_IsolateSample3");
        private static readonly int UseSample3 = Shader.PropertyToID("_UseSample3");
        private static readonly int SampleColor3 = Shader.PropertyToID("_SampleColor3");
        private static readonly int SampleUv3 = Shader.PropertyToID("_SampleUV3");
        private static readonly int HueWeight3 = Shader.PropertyToID("_HueWeight3");
        private static readonly int SatWeight3 = Shader.PropertyToID("_SatWeight3");
        private static readonly int LumWeight3 = Shader.PropertyToID("_LumWeight3");
        private static readonly int MaskLow3 = Shader.PropertyToID("_MaskLow3");
        private static readonly int MaskHigh3 = Shader.PropertyToID("_MaskHigh3");
        private static readonly int Sample3Smoothness = Shader.PropertyToID("_Sample3Smoothness");
        private static readonly int BaseSmoothness = Shader.PropertyToID("_BaseSmoothness");
        private static readonly int Slider = Shader.PropertyToID("_Slider");
        private static readonly int BlurOverlay = Shader.PropertyToID("_BlurOverlay");
        private static readonly int FinalContrast = Shader.PropertyToID("_FinalContrast");
        private static readonly int FinalBias = Shader.PropertyToID("_FinalBias");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int MetallicTex = Shader.PropertyToID("_MetallicTex");
        private static readonly int OverlayBlurTex = Shader.PropertyToID("_OverlayBlurTex");
        private RenderTexture _blurMap;
        private Camera _camera;

        private int _currentSelection;

        private Texture2D _diffuseMap;
        private Texture2D _diffuseMapOriginal;
        private bool _doStuff;

        private int _imageSizeX;
        private int _imageSizeY;
        private bool _lastUseAdjustedDiffuse;

        private Texture2D _metallicMap;
        private bool _mouseButtonDown;
        private bool _newTexture;
        private RenderTexture _overlayBlurMap;

        private Texture2D _sampleColorMap1;
        private Texture2D _sampleColorMap2;
        private Texture2D _sampleColorMap3;

        private bool _selectingColor;

        private SmoothnessSettings _settings;

        private bool _settingsInitialized;

        private float _slider = 0.5f;
        private Texture2D _smoothnessMap;

        private RenderTexture _tempMap;

        private Rect _windowRect;
        private int _windowId;

        public Texture2D DefaultMetallicMap;
        public GameObject TestObject;

        public Material ThisMaterial;
        public ComputeShader BlurCompute;
        public ComputeShader SmoothnessCompute;
        private bool _readyToProcess;

        private void Awake()
        {
            _camera = Camera.main;
            _windowRect = new Rect(10.0f, 265.0f, 300f, 450f);
        }

        private void OnDisable()
        {
            CleanUpTextures();
            _readyToProcess = false;
        }

        public void GetValues(ProjectObject projectObject)
        {
            InitializeSettings();
            projectObject.SmoothnessSettings = _settings;
        }

        public void SetValues(ProjectObject projectObject)
        {
            InitializeSettings();
            if (projectObject.SmoothnessSettings != null)
            {
                _settings = projectObject.SmoothnessSettings;
            }
            else
            {
                _settingsInitialized = false;
                InitializeSettings();
            }

            _sampleColorMap1.SetPixel(1, 1, _settings.SampleColor1);
            _sampleColorMap1.Apply(false);

            _sampleColorMap2.SetPixel(1, 1, _settings.SampleColor2);
            _sampleColorMap2.Apply(false);

            _sampleColorMap3.SetPixel(1, 1, _settings.SampleColor3);
            _sampleColorMap3.Apply(false);

            _doStuff = true;
        }

        private void InitializeSettings()
        {
            if (_settingsInitialized) return;
            _settings = new SmoothnessSettings();

            _sampleColorMap1 = TextureManager.Instance.GetStandardTexture(1, 1);
            _sampleColorMap1.SetPixel(1, 1, _settings.SampleColor1);
            _sampleColorMap1.Apply(false);

            _sampleColorMap2 = TextureManager.Instance.GetStandardTexture(1, 1);
            _sampleColorMap2.SetPixel(1, 1, _settings.SampleColor2);
            _sampleColorMap2.Apply(false);

            _sampleColorMap3 = TextureManager.Instance.GetStandardTexture(1, 1);
            _sampleColorMap3.SetPixel(1, 1, _settings.SampleColor3);
            _sampleColorMap3.Apply(false);

            _settingsInitialized = true;
        }

        // Use this for initialization
        private void Start()
        {
            TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

            InitializeSettings();
            _windowId = ProgramManager.Instance.GetWindowId;
        }

        public void DoStuff()
        {
            _doStuff = true;
        }

        public void NewTexture()
        {
            _newTexture = true;
        }

        // Update is called once per frame
        private void Update()
        {
            if (ProgramManager.IsLocked) return;

            if (_selectingColor) SelectColor();

            if (_newTexture)
            {
                InitializeTextures();
                _newTexture = false;
            }

            if (_settings.UseAdjustedDiffuse != _lastUseAdjustedDiffuse)
            {
                _lastUseAdjustedDiffuse = _settings.UseAdjustedDiffuse;
                _doStuff = true;
            }

            if (_doStuff)
            {
                StartCoroutine(ProcessBlur());
                _doStuff = false;
            }

            //thisMaterial.SetFloat ("_BlurWeight", BlurWeight);

            ThisMaterial.SetFloat(MetalSmoothness, _settings.MetalSmoothness);

            ThisMaterial.SetInt(IsolateSample1, _settings.IsolateSample1 ? 1 : 0);
            ThisMaterial.SetInt(UseSample1, _settings.UseSample1 ? 1 : 0);
            ThisMaterial.SetColor(SampleColor1, _settings.SampleColor1);
            ThisMaterial.SetVector(SampleUv1, new Vector4(_settings.SampleUv1.x, _settings.SampleUv1.y, 0, 0));
            ThisMaterial.SetFloat(HueWeight1, _settings.HueWeight1);
            ThisMaterial.SetFloat(SatWeight1, _settings.SatWeight1);
            ThisMaterial.SetFloat(LumWeight1, _settings.LumWeight1);
            ThisMaterial.SetFloat(MaskLow1, _settings.MaskLow1);
            ThisMaterial.SetFloat(MaskHigh1, _settings.MaskHigh1);
            ThisMaterial.SetFloat(Sample1Smoothness, _settings.Sample1Smoothness);

            ThisMaterial.SetInt(IsolateSample2, _settings.IsolateSample2 ? 1 : 0);
            ThisMaterial.SetInt(UseSample2, _settings.UseSample2 ? 1 : 0);
            ThisMaterial.SetColor(SampleColor2, _settings.SampleColor2);
            ThisMaterial.SetVector(SampleUv2, new Vector4(_settings.SampleUv2.x, _settings.SampleUv2.y, 0, 0));
            ThisMaterial.SetFloat(HueWeight2, _settings.HueWeight2);
            ThisMaterial.SetFloat(SatWeight2, _settings.SatWeight2);
            ThisMaterial.SetFloat(LumWeight2, _settings.LumWeight2);
            ThisMaterial.SetFloat(MaskLow2, _settings.MaskLow2);
            ThisMaterial.SetFloat(MaskHigh2, _settings.MaskHigh2);
            ThisMaterial.SetFloat(Sample2Smoothness, _settings.Sample2Smoothness);

            ThisMaterial.SetInt(IsolateSample3, _settings.IsolateSample3 ? 1 : 0);
            ThisMaterial.SetInt(UseSample3, _settings.UseSample3 ? 1 : 0);
            ThisMaterial.SetColor(SampleColor3, _settings.SampleColor3);
            ThisMaterial.SetVector(SampleUv3, new Vector4(_settings.SampleUv3.x, _settings.SampleUv3.y, 0, 0));
            ThisMaterial.SetFloat(HueWeight3, _settings.HueWeight3);
            ThisMaterial.SetFloat(SatWeight3, _settings.SatWeight3);
            ThisMaterial.SetFloat(LumWeight3, _settings.LumWeight3);
            ThisMaterial.SetFloat(MaskLow3, _settings.MaskLow3);
            ThisMaterial.SetFloat(MaskHigh3, _settings.MaskHigh3);
            ThisMaterial.SetFloat(Sample3Smoothness, _settings.Sample3Smoothness);

            ThisMaterial.SetFloat(BaseSmoothness, _settings.BaseSmoothness);

            ThisMaterial.SetFloat(Slider, _slider);
            ThisMaterial.SetFloat(BlurOverlay, _settings.BlurOverlay);
            ThisMaterial.SetFloat(FinalContrast, _settings.FinalContrast);
            ThisMaterial.SetFloat(FinalBias, _settings.FinalBias);

            ThisMaterial.SetTexture(MainTex, _settings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal);
        }

        private void SelectColor()
        {
            if (Input.GetMouseButton(0))
            {
                _mouseButtonDown = true;

                if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit))
                    return;

                var rend = hit.transform.GetComponent<Renderer>();
                var meshCollider = hit.collider as MeshCollider;
                if (!rend || !rend.sharedMaterial || !rend.sharedMaterial.mainTexture || !meshCollider)
                    return;

                var pixelUv = hit.textureCoord;

                var sampledColor = _settings.UseAdjustedDiffuse
                    ? _diffuseMap.GetPixelBilinear(pixelUv.x, pixelUv.y)
                    : _diffuseMapOriginal.GetPixelBilinear(pixelUv.x, pixelUv.y);

                switch (_currentSelection)
                {
                    case 1:
                        _settings.SampleUv1 = pixelUv;
                        _settings.SampleColor1 = sampledColor;
                        _sampleColorMap1.SetPixel(1, 1, _settings.SampleColor1);
                        _sampleColorMap1.Apply(false);
                        break;
                    case 2:
                        _settings.SampleUv2 = pixelUv;
                        _settings.SampleColor2 = sampledColor;
                        _sampleColorMap2.SetPixel(1, 1, _settings.SampleColor2);
                        _sampleColorMap2.Apply(false);
                        break;
                    case 3:
                        _settings.SampleUv3 = pixelUv;
                        _settings.SampleColor3 = sampledColor;
                        _sampleColorMap3.SetPixel(1, 1, _settings.SampleColor3);
                        _sampleColorMap3.Apply(false);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            if (!Input.GetMouseButtonUp(0) || !_mouseButtonDown) return;
            _mouseButtonDown = false;
            _selectingColor = false;
            _currentSelection = 0;
        }

        private void DoMyWindow(int windowId)
        {
            const int offsetX = 10;
            var offsetY = 30;

            GUI.enabled = _diffuseMap != null;
            if (GUI.Toggle(new Rect(offsetX, offsetY, 140, 30), _settings.UseAdjustedDiffuse, " Use Edited Diffuse"))
            {
                _settings.UseAdjustedDiffuse = true;
                _settings.UseOriginalDiffuse = false;
            }

            GUI.enabled = true;
            if (GUI.Toggle(new Rect(offsetX + 150, offsetY, 140, 30), _settings.UseOriginalDiffuse,
                " Use Original Diffuse"))
            {
                _settings.UseAdjustedDiffuse = false;
                _settings.UseOriginalDiffuse = true;
            }

            offsetY += 30;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Smoothness Reveal Slider");
            _slider = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 20, 280, 10), _slider, 0.0f, 1.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Metal Smoothness", _settings.MetalSmoothness,
                out _settings.MetalSmoothness, 0.0f, 1.0f);
            offsetY += 40;

            _settings.UseSample1 =
                GUI.Toggle(new Rect(offsetX, offsetY, 150, 20), _settings.UseSample1, "Use Color Sample 1");
            if (_settings.UseSample1)
            {
                _settings.IsolateSample1 =
                    GUI.Toggle(new Rect(offsetX + 180, offsetY, 150, 20), _settings.IsolateSample1, "Isolate Mask");
                if (_settings.IsolateSample1)
                {
                    _settings.IsolateSample2 = false;
                    _settings.IsolateSample3 = false;
                }

                offsetY += 30;

                if (GUI.Button(new Rect(offsetX, offsetY + 5, 80, 20), "Pick Color"))
                {
                    _selectingColor = true;
                    _currentSelection = 1;
                }

                GUI.DrawTexture(new Rect(offsetX + 10, offsetY + 35, 60, 60), _sampleColorMap1);

                GUI.Label(new Rect(offsetX + 90, offsetY, 250, 30), "Hue");
                _settings.HueWeight1 = GUI.VerticalSlider(new Rect(offsetX + 95, offsetY + 30, 10, 70),
                    _settings.HueWeight1, 1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 120, offsetY, 250, 30), "Sat");
                _settings.SatWeight1 =
                    GUI.VerticalSlider(new Rect(offsetX + 125, offsetY + 30, 10, 70), _settings.SatWeight1, 1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 150, offsetY, 250, 30), "Lum");
                _settings.LumWeight1 =
                    GUI.VerticalSlider(new Rect(offsetX + 155, offsetY + 30, 10, 70), _settings.LumWeight1, 1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 180, offsetY, 250, 30), "Low");
                _settings.MaskLow1 = GUI.VerticalSlider(new Rect(offsetX + 185, offsetY + 30, 10, 70),
                    _settings.MaskLow1,
                    1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 210, offsetY, 250, 30), "High");
                _settings.MaskHigh1 = GUI.VerticalSlider(new Rect(offsetX + 215, offsetY + 30, 10, 70),
                    _settings.MaskHigh1,
                    1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 240, offsetY, 250, 30), "Smooth");
                _settings.Sample1Smoothness = GUI.VerticalSlider(new Rect(offsetX + 255, offsetY + 30, 10, 70),
                    _settings.Sample1Smoothness, 1.0f, 0.0f);

                offsetY += 110;
            }
            else
            {
                offsetY += 30;
                _settings.IsolateSample1 = false;
            }


            _settings.UseSample2 =
                GUI.Toggle(new Rect(offsetX, offsetY, 150, 20), _settings.UseSample2, "Use Color Sample 2");
            if (_settings.UseSample2)
            {
                _settings.IsolateSample2 =
                    GUI.Toggle(new Rect(offsetX + 180, offsetY, 150, 20), _settings.IsolateSample2, "Isolate Mask");
                if (_settings.IsolateSample2)
                {
                    _settings.IsolateSample1 = false;
                    _settings.IsolateSample3 = false;
                }

                offsetY += 30;

                if (GUI.Button(new Rect(offsetX, offsetY + 5, 80, 20), "Pick Color"))
                {
                    _selectingColor = true;
                    _currentSelection = 2;
                }

                GUI.DrawTexture(new Rect(offsetX + 10, offsetY + 35, 60, 60), _sampleColorMap2);

                GUI.Label(new Rect(offsetX + 90, offsetY, 250, 30), "Hue");
                _settings.HueWeight2 = GUI.VerticalSlider(new Rect(offsetX + 95, offsetY + 30, 10, 70),
                    _settings.HueWeight2, 1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 120, offsetY, 250, 30), "Sat");
                _settings.SatWeight2 =
                    GUI.VerticalSlider(new Rect(offsetX + 125, offsetY + 30, 10, 70), _settings.SatWeight2, 1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 150, offsetY, 250, 30), "Lum");
                _settings.LumWeight2 =
                    GUI.VerticalSlider(new Rect(offsetX + 155, offsetY + 30, 10, 70), _settings.LumWeight2, 1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 180, offsetY, 250, 30), "Low");
                _settings.MaskLow2 = GUI.VerticalSlider(new Rect(offsetX + 185, offsetY + 30, 10, 70),
                    _settings.MaskLow2,
                    1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 210, offsetY, 250, 30), "High");
                _settings.MaskHigh2 = GUI.VerticalSlider(new Rect(offsetX + 215, offsetY + 30, 10, 70),
                    _settings.MaskHigh2,
                    1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 240, offsetY, 250, 30), "Smooth");
                _settings.Sample2Smoothness = GUI.VerticalSlider(new Rect(offsetX + 255, offsetY + 30, 10, 70),
                    _settings.Sample2Smoothness, 1.0f, 0.0f);

                offsetY += 110;
            }
            else
            {
                offsetY += 30;
                _settings.IsolateSample2 = false;
            }

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Base Smoothness", _settings.BaseSmoothness,
                out _settings.BaseSmoothness, 0.0f, 1.0f);
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Sample Blur Size", _settings.BlurSize,
                out _settings.BlurSize, 0, 100)) _doStuff = true;
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "High Pass Blur Size", _settings.OverlayBlurSize,
                out _settings.OverlayBlurSize, 10, 100)) _doStuff = true;
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "High Pass Overlay", _settings.BlurOverlay,
                out _settings.BlurOverlay, -10.0f, 10.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Contrast", _settings.FinalContrast,
                out _settings.FinalContrast, -2.0f, 2.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Bias", _settings.FinalBias,
                out _settings.FinalBias, -0.5f, 0.5f);

            GUI.DragWindow();
        }

        private void OnGUI()
        {
            if (Hide) return;
            if (_settings.UseSample1) _windowRect.height += 110;

            if (_settings.UseSample2) _windowRect.height += 110;

            MainGui.MakeScaledWindow(_windowRect, _windowId, DoMyWindow, "Smoothness From Diffuse");
        }

        public void Close()
        {
            CleanUpTextures();
            gameObject.SetActive(false);
        }

        private void CleanUpTextures()
        {
            RenderTexture.ReleaseTemporary(_blurMap);
            RenderTexture.ReleaseTemporary(_overlayBlurMap);
            RenderTexture.ReleaseTemporary(_tempMap);
        }

        public void InitializeTextures()
        {
            TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

            CleanUpTextures();

            _diffuseMap = TextureManager.Instance.DiffuseMap;
            _diffuseMapOriginal = TextureManager.Instance.DiffuseMapOriginal;

            _metallicMap = TextureManager.Instance.MetallicMap;
            ThisMaterial.SetTexture(MetallicTex, _metallicMap != null ? _metallicMap : DefaultMetallicMap);

            if (_diffuseMap)
            {
                ThisMaterial.SetTexture(MainTex, _diffuseMap);
                _imageSizeX = _diffuseMap.width;
                _imageSizeY = _diffuseMap.height;
            }
            else
            {
                ThisMaterial.SetTexture(MainTex, _diffuseMapOriginal);
                _imageSizeX = _diffuseMapOriginal.width;
                _imageSizeY = _diffuseMapOriginal.height;

                _settings.UseAdjustedDiffuse = false;
                _settings.UseOriginalDiffuse = true;
            }

            General.Logger.Log("Initializing Textures of size: " + _imageSizeX + "x" + _imageSizeY);

            _tempMap = TextureManager.Instance.GetTempRenderTexture(_imageSizeX, _imageSizeY);
            _blurMap = TextureManager.Instance.GetTempRenderTexture(_imageSizeX, _imageSizeY);
            _overlayBlurMap = TextureManager.Instance.GetTempRenderTexture(_imageSizeX, _imageSizeY);
        }

        public IEnumerator Process()
        {
            while (!ProgramManager.Lock())
            {
                yield return null;
            }

            while (!_readyToProcess)
            {
                yield return null;
            }

            General.Logger.Log("Processing Height");
            var smoothnessKernel = SmoothnessCompute.FindKernel("CSSmoothness");

            SmoothnessCompute.SetVector("_ImageSize", new Vector2(_imageSizeX, _imageSizeY));

            SmoothnessCompute.SetTexture(smoothnessKernel, "_MetallicTex",
                _metallicMap != null ? _metallicMap : DefaultMetallicMap);


            SmoothnessCompute.SetTexture(smoothnessKernel, "_BlurTex", _blurMap);

            SmoothnessCompute.SetTexture(smoothnessKernel, "_OverlayBlurTex", _overlayBlurMap);

            SmoothnessCompute.SetFloat("_MetalSmoothness", _settings.MetalSmoothness);

            SmoothnessCompute.SetInt("_UseSample1", _settings.UseSample1 ? 1 : 0);
            SmoothnessCompute.SetVector("_SampleColor1", _settings.SampleColor1);
            SmoothnessCompute.SetVector("_SampleUV1",
                new Vector4(_settings.SampleUv1.x, _settings.SampleUv1.y, 0, 0));
            SmoothnessCompute.SetFloat("_HueWeight1", _settings.HueWeight1);
            SmoothnessCompute.SetFloat("_SatWeight1", _settings.SatWeight1);
            SmoothnessCompute.SetFloat("_LumWeight1", _settings.LumWeight1);
            SmoothnessCompute.SetFloat("_MaskLow1", _settings.MaskLow1);
            SmoothnessCompute.SetFloat("_MaskHigh1", _settings.MaskHigh1);
            SmoothnessCompute.SetFloat("_Sample1Smoothness", _settings.Sample1Smoothness);

            SmoothnessCompute.SetInt("_UseSample2", _settings.UseSample2 ? 1 : 0);
            SmoothnessCompute.SetVector("_SampleColor2", _settings.SampleColor2);
            SmoothnessCompute.SetVector("_SampleUV2",
                new Vector4(_settings.SampleUv2.x, _settings.SampleUv2.y, 0, 0));
            SmoothnessCompute.SetFloat("_HueWeight2", _settings.HueWeight2);
            SmoothnessCompute.SetFloat("_SatWeight2", _settings.SatWeight2);
            SmoothnessCompute.SetFloat("_LumWeight2", _settings.LumWeight2);
            SmoothnessCompute.SetFloat("_MaskLow2", _settings.MaskLow2);
            SmoothnessCompute.SetFloat("_MaskHigh2", _settings.MaskHigh2);
            SmoothnessCompute.SetFloat("_Sample2Smoothness", _settings.Sample2Smoothness);

            SmoothnessCompute.SetInt("_UseSample3", _settings.UseSample3 ? 1 : 0);
            SmoothnessCompute.SetVector("_SampleColor3", _settings.SampleColor3);
            SmoothnessCompute.SetVector("_SampleUV3",
                new Vector4(_settings.SampleUv3.x, _settings.SampleUv3.y, 0, 0));
            SmoothnessCompute.SetFloat("_HueWeight3", _settings.HueWeight3);
            SmoothnessCompute.SetFloat("_SatWeight3", _settings.SatWeight3);
            SmoothnessCompute.SetFloat("_LumWeight3", _settings.LumWeight3);
            SmoothnessCompute.SetFloat("_MaskLow3", _settings.MaskLow3);
            SmoothnessCompute.SetFloat("_MaskHigh3", _settings.MaskHigh3);
            SmoothnessCompute.SetFloat("_Sample3Smoothness", _settings.Sample3Smoothness);

            SmoothnessCompute.SetFloat("_BaseSmoothness", _settings.BaseSmoothness);

            SmoothnessCompute.SetFloat("_BlurOverlay", _settings.BlurOverlay);
            SmoothnessCompute.SetFloat("_FinalContrast", _settings.FinalContrast);
            SmoothnessCompute.SetFloat("_FinalBias", _settings.FinalBias);

            RenderTexture.ReleaseTemporary(_tempMap);
            _tempMap = TextureManager.Instance.GetTempRenderTexture(_imageSizeX, _imageSizeY);

            var groupsX = (int) Mathf.Ceil(_imageSizeX / 8f);
            var groupsY = (int) Mathf.Ceil(_imageSizeY / 8f);

            var source = _settings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal;
            SmoothnessCompute.SetTexture(smoothnessKernel, "ImageInput", source);
            SmoothnessCompute.SetTexture(smoothnessKernel, "Result", _tempMap);
            SmoothnessCompute.Dispatch(smoothnessKernel, groupsX, groupsY, 1);

            TextureManager.Instance.GetTextureFromRender(_tempMap, ProgramEnums.MapType.Smoothness);

            RenderTexture.ReleaseTemporary(_tempMap);

            yield return null;

            ProgramManager.Unlock();
        }

        public IEnumerator ProcessBlur()
        {
            while (!ProgramManager.Lock())
            {
                yield return null;
            }

            var groupsX = (int) Mathf.Ceil(_imageSizeX / 8f);
            var groupsY = (int) Mathf.Ceil(_imageSizeY / 8f);

            General.Logger.Log("Processing Blur");
            var blurKernel = BlurCompute.FindKernel("CSBlur");

            BlurCompute.SetVector("_ImageSize", new Vector4(_imageSizeX, _imageSizeY, 0, 0));
            BlurCompute.SetFloat("_BlurContrast", 1.0f);
            BlurCompute.SetFloat("_BlurSpread", 1.0f);

            // Blur the image for selection
            BlurCompute.SetInt("_BlurSamples", _settings.BlurSize);
            BlurCompute.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
            var diffuse = _settings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal;

            if (_settings.BlurSize == 0)
                Graphics.Blit(diffuse, _tempMap);
            else
            {
                BlurCompute.SetTexture(blurKernel, "ImageInput", diffuse);
                BlurCompute.SetTexture(blurKernel, "Result", _tempMap);
                BlurCompute.Dispatch(blurKernel, groupsX, groupsY, 1);
            }

            BlurCompute.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
            if (_settings.BlurSize == 0)
                Graphics.Blit(_tempMap, _blurMap);
            else
            {
                BlurCompute.SetTexture(blurKernel, "ImageInput", _tempMap);
                BlurCompute.SetTexture(blurKernel, "Result", _blurMap);
                BlurCompute.Dispatch(blurKernel, groupsX, groupsY, 1);
            }

            ThisMaterial.SetTexture("_BlurTex", _blurMap);

            // Blur the image for overlay
            BlurCompute.SetInt("_BlurSamples", _settings.OverlayBlurSize);
            BlurCompute.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
            var source = _settings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal;
            BlurCompute.SetTexture(blurKernel, "ImageInput", source);
            BlurCompute.SetTexture(blurKernel, "Result", _tempMap);
            BlurCompute.Dispatch(blurKernel, groupsX, groupsY, 1);

            BlurCompute.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
            BlurCompute.SetTexture(blurKernel, "ImageInput", _tempMap);
            BlurCompute.SetTexture(blurKernel, "Result", _overlayBlurMap);
            BlurCompute.Dispatch(blurKernel, groupsX, groupsY, 1);
            ThisMaterial.SetTexture(OverlayBlurTex, _overlayBlurMap);

            yield return null;
            yield return null;

            _readyToProcess = true;

            ProgramManager.Unlock();
        }

        public bool Hide { get; set; }
    }
}