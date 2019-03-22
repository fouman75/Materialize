#region

using System;
using System.Collections;
using General;
using Settings;
using UnityEngine;
using Logger = General.Logger;

#endregion

// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace Gui
{
    public class HeightFromDiffuseGui : TexturePanelGui
    {
        private RenderTexture _avgMap;

        private RenderTexture _avgTempMap;
        private Material _blitMaterialNormal;
        private RenderTexture _blurMap0;
        private RenderTexture _blurMap1;
        private RenderTexture _blurMap2;
        private RenderTexture _blurMap3;
        private RenderTexture _blurMap4;
        private RenderTexture _blurMap5;
        private RenderTexture _blurMap6;
        private Camera _camera;

        private int _currentSelection;

        private HeightFromDiffuseSettings _heightFromDiffuseSettings;
        private int _kernelBlur;

        private float _lastBlur0Contrast = 1.0f;

        private bool _lastUseDiffuse;
        private bool _lastUseNormal;
        private bool _lastUseOriginalDiffuse;
        private bool _mouseButtonDown;

        private Texture2D _sampleColorMap1;
        private Texture2D _sampleColorMap2;
        private bool _selectingColor;
        private bool _settingsInitialized;

        private float _slider = 0.5f;

        private RenderTexture _tempBlurMap;
        private RenderTexture _tempHeightMap;
        private int _windowId;

        private Rect _windowRect;
        public ComputeShader BlurCompute;
        public ComputeShader HeightCompute;
        public ComputeShader SampleCompute;

        protected override IEnumerator Process()
        {
            MessagePanel.ShowMessage("Processing Height Map");

            var kernelCombine = HeightCompute.FindKernel("CSCombineHeight");
            HeightCompute.SetVector(ImageSizeId, new Vector4(ImageSize.x, ImageSize.y, 0, 0));

            RenderTexture.ReleaseTemporary(_tempHeightMap);
            _tempHeightMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y);

            HeightCompute.SetFloat(FinalContrast, _heightFromDiffuseSettings.FinalContrast);
            HeightCompute.SetFloat(FinalBias, _heightFromDiffuseSettings.FinalBias);

            var realGain = _heightFromDiffuseSettings.FinalGain;
            if (realGain < 0.0f)
                realGain = Mathf.Abs(1.0f / (realGain - 1.0f));
            else
                realGain = realGain + 1.0f;

            HeightCompute.SetFloat(FinalGain, realGain);

            HeightCompute.SetFloat(Blur0Weight, _heightFromDiffuseSettings.Blur0Weight);
            HeightCompute.SetFloat(Blur1Weight, _heightFromDiffuseSettings.Blur1Weight);
            HeightCompute.SetFloat(Blur2Weight, _heightFromDiffuseSettings.Blur2Weight);
            HeightCompute.SetFloat(Blur3Weight, _heightFromDiffuseSettings.Blur3Weight);
            HeightCompute.SetFloat(Blur4Weight, _heightFromDiffuseSettings.Blur4Weight);
            HeightCompute.SetFloat(Blur5Weight, _heightFromDiffuseSettings.Blur5Weight);
            HeightCompute.SetFloat(Blur6Weight, _heightFromDiffuseSettings.Blur6Weight);

            HeightCompute.SetFloat(Blur0Contrast, _heightFromDiffuseSettings.Blur0Contrast);
            HeightCompute.SetFloat(Blur1Contrast, _heightFromDiffuseSettings.Blur1Contrast);
            HeightCompute.SetFloat(Blur2Contrast, _heightFromDiffuseSettings.Blur2Contrast);
            HeightCompute.SetFloat(Blur3Contrast, _heightFromDiffuseSettings.Blur3Contrast);
            HeightCompute.SetFloat(Blur4Contrast, _heightFromDiffuseSettings.Blur4Contrast);
            HeightCompute.SetFloat(Blur5Contrast, _heightFromDiffuseSettings.Blur5Contrast);
            HeightCompute.SetFloat(Blur6Contrast, _heightFromDiffuseSettings.Blur6Contrast);

            HeightCompute.SetTexture(kernelCombine, BlurTex0, _blurMap0);
            HeightCompute.SetTexture(kernelCombine, BlurTex1, _blurMap1);
            HeightCompute.SetTexture(kernelCombine, BlurTex2, _blurMap2);
            HeightCompute.SetTexture(kernelCombine, BlurTex3, _blurMap3);
            HeightCompute.SetTexture(kernelCombine, BlurTex4, _blurMap4);
            HeightCompute.SetTexture(kernelCombine, BlurTex5, _blurMap5);
            HeightCompute.SetTexture(kernelCombine, BlurTex6, _blurMap6);

            HeightCompute.SetTexture(kernelCombine, AvgTex, _avgMap);

            HeightCompute.SetBool(HeightFromNormal, _heightFromDiffuseSettings.UseNormal);

            // Save low fidelity for texture 2d
            HeightCompute.SetTexture(kernelCombine, "ImageInput", _blurMap0);
            HeightCompute.SetTexture(kernelCombine, "Result", _tempHeightMap);
            HeightCompute.Dispatch(kernelCombine, ImageSize.x / 8, ImageSize.y / 8, 1);

            if (TextureManager.Instance.HeightMap) Destroy(TextureManager.Instance.HeightMap);

            TextureManager.Instance.GetTextureFromRender(_tempHeightMap, ProgramEnums.MapType.Height);

            // Save high fidelity for normal making
            if (TextureManager.Instance.HdHeightMap)
            {
                TextureManager.Instance.HdHeightMap.Release();
                TextureManager.Instance.HdHeightMap = null;
            }

            TextureManager.Instance.HdHeightMap =
                TextureManager.Instance.GetTempRenderTexture(_tempHeightMap.width, _tempHeightMap.width);
            HeightCompute.SetTexture(kernelCombine, "ImageInput", _blurMap0);
            HeightCompute.SetTexture(kernelCombine, "Result", TextureManager.Instance.HdHeightMap);
            HeightCompute.Dispatch(kernelCombine, ImageSize.x / 8, ImageSize.y / 8, 1);

            RenderTexture.ReleaseTemporary(_tempHeightMap);

            yield break;
        }

        private void Awake()
        {
            _windowRect = new Rect(10.0f, 265.0f, 300f, 520f);
        }


        public void GetValues(ProjectObject projectObject)
        {
            InitializeSettings();
            projectObject.HeightFromDiffuseSettings = _heightFromDiffuseSettings;
        }

        public void SetValues(ProjectObject projectObject)
        {
            InitializeSettings();
            if (projectObject.HeightFromDiffuseSettings != null)
            {
                _heightFromDiffuseSettings = projectObject.HeightFromDiffuseSettings;
            }
            else
            {
                _settingsInitialized = false;
                InitializeSettings();
            }

            _sampleColorMap1.SetPixel(1, 1, _heightFromDiffuseSettings.SampleColor1);
            _sampleColorMap1.Apply(false);

            _sampleColorMap2.SetPixel(1, 1, _heightFromDiffuseSettings.SampleColor2);
            _sampleColorMap2.Apply(false);

            StuffToBeDone = true;
        }

        private void InitializeSettings()
        {
            if (_settingsInitialized) return;
            Logger.Log("Initializing Height From Diffuse Settings");

            _heightFromDiffuseSettings = new HeightFromDiffuseSettings();

            if (_sampleColorMap1) Destroy(_sampleColorMap1);
            _sampleColorMap1 = TextureManager.Instance.GetStandardTexture(1, 1);
            _sampleColorMap1.SetPixel(1, 1, _heightFromDiffuseSettings.SampleColor1);
            _sampleColorMap1.Apply(false);

            if (_sampleColorMap2) Destroy(_sampleColorMap2);
            _sampleColorMap2 = TextureManager.Instance.GetStandardTexture(1, 1);
            _sampleColorMap2.SetPixel(1, 1, _heightFromDiffuseSettings.SampleColor2);
            _sampleColorMap2.Apply(false);

            _settingsInitialized = true;
        }

        private void Start()
        {
            MessagePanel.ShowMessage("Initializing Height GUI");
            _camera = Camera.main;
            TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

            InitializeSettings();

            if (IsNewTexture)
            {
                InitializeTextures();
                IsNewTexture = false;
            }

            FixUseMaps();

            _lastUseDiffuse = _heightFromDiffuseSettings.UseAdjustedDiffuse;
            _lastUseOriginalDiffuse = _heightFromDiffuseSettings.UseOriginalDiffuse;
            _lastUseNormal = _heightFromDiffuseSettings.UseNormal;
            _lastBlur0Contrast = _heightFromDiffuseSettings.Blur0Contrast;

            SetMaterialValues();

            _kernelBlur = BlurCompute.FindKernel("CSBlur");
            _windowId = ProgramManager.Instance.GetWindowId;
        }

        private void FixUseMaps()
        {
            if (!TextureManager.Instance.DiffuseMapOriginal && _heightFromDiffuseSettings.UseOriginalDiffuse)
            {
                _heightFromDiffuseSettings.UseAdjustedDiffuse = true;
                _heightFromDiffuseSettings.UseOriginalDiffuse = false;
                _heightFromDiffuseSettings.UseNormal = false;
            }

            if (!TextureManager.Instance.DiffuseMap && _heightFromDiffuseSettings.UseAdjustedDiffuse)
            {
                _heightFromDiffuseSettings.UseAdjustedDiffuse = false;
                _heightFromDiffuseSettings.UseOriginalDiffuse = true;
                _heightFromDiffuseSettings.UseNormal = false;
            }

            if (!TextureManager.Instance.NormalMap && _heightFromDiffuseSettings.UseNormal)
            {
                _heightFromDiffuseSettings.UseAdjustedDiffuse = true;
                _heightFromDiffuseSettings.UseOriginalDiffuse = false;
                _heightFromDiffuseSettings.UseNormal = false;
            }

            if ((TextureManager.Instance.DiffuseMapOriginal == null) & (TextureManager.Instance.NormalMap == null))
            {
                _heightFromDiffuseSettings.UseAdjustedDiffuse = true;
                _heightFromDiffuseSettings.UseOriginalDiffuse = false;
                _heightFromDiffuseSettings.UseNormal = false;
            }

            if (TextureManager.Instance.DiffuseMap == null && TextureManager.Instance.NormalMap == null)
            {
                _heightFromDiffuseSettings.UseAdjustedDiffuse = false;
                _heightFromDiffuseSettings.UseOriginalDiffuse = true;
                _heightFromDiffuseSettings.UseNormal = false;
            }

            if (TextureManager.Instance.DiffuseMap != null ||
                TextureManager.Instance.DiffuseMapOriginal != null) return;

            _heightFromDiffuseSettings.UseAdjustedDiffuse = false;
            _heightFromDiffuseSettings.UseOriginalDiffuse = false;
            _heightFromDiffuseSettings.UseNormal = true;
        }

        private void SetMaterialValues()
        {
            ThisMaterial.SetFloat(BlurScaleId, BlurScale);
            ThisMaterial.SetVector(ImageSizeId, new Vector4(ImageSize.x, ImageSize.y, 0, 0));
        }

        private void SetWeightEqDefault()
        {
            _heightFromDiffuseSettings.Blur0Weight = 0.15f;
            _heightFromDiffuseSettings.Blur1Weight = 0.19f;
            _heightFromDiffuseSettings.Blur2Weight = 0.3f;
            _heightFromDiffuseSettings.Blur3Weight = 0.5f;
            _heightFromDiffuseSettings.Blur4Weight = 0.7f;
            _heightFromDiffuseSettings.Blur5Weight = 0.9f;
            _heightFromDiffuseSettings.Blur6Weight = 1.0f;
            StuffToBeDone = true;
        }

        private void SetWeightEqDetail()
        {
            _heightFromDiffuseSettings.Blur0Weight = 0.7f;
            _heightFromDiffuseSettings.Blur1Weight = 0.4f;
            _heightFromDiffuseSettings.Blur2Weight = 0.3f;
            _heightFromDiffuseSettings.Blur3Weight = 0.5f;
            _heightFromDiffuseSettings.Blur4Weight = 0.8f;
            _heightFromDiffuseSettings.Blur5Weight = 0.9f;
            _heightFromDiffuseSettings.Blur6Weight = 0.7f;
            StuffToBeDone = true;
        }

        private void SetWeightEqDisplace()
        {
            _heightFromDiffuseSettings.Blur0Weight = 0.02f;
            _heightFromDiffuseSettings.Blur1Weight = 0.03f;
            _heightFromDiffuseSettings.Blur2Weight = 0.1f;
            _heightFromDiffuseSettings.Blur3Weight = 0.35f;
            _heightFromDiffuseSettings.Blur4Weight = 0.7f;
            _heightFromDiffuseSettings.Blur5Weight = 0.9f;
            _heightFromDiffuseSettings.Blur6Weight = 1.0f;
            StuffToBeDone = true;
        }

        private void SetContrastEqDefault()
        {
            _heightFromDiffuseSettings.Blur0Contrast = 1.0f;
            _heightFromDiffuseSettings.Blur1Contrast = 1.0f;
            _heightFromDiffuseSettings.Blur2Contrast = 1.0f;
            _heightFromDiffuseSettings.Blur3Contrast = 1.0f;
            _heightFromDiffuseSettings.Blur4Contrast = 1.0f;
            _heightFromDiffuseSettings.Blur5Contrast = 1.0f;
            _heightFromDiffuseSettings.Blur6Contrast = 1.0f;
            StuffToBeDone = true;
        }

        private void SetContrastEqCrackedMud()
        {
            _heightFromDiffuseSettings.Blur0Contrast = 1.0f;
            _heightFromDiffuseSettings.Blur1Contrast = 1.0f;
            _heightFromDiffuseSettings.Blur2Contrast = 1.0f;
            _heightFromDiffuseSettings.Blur3Contrast = 1.0f;
            _heightFromDiffuseSettings.Blur4Contrast = -0.2f;
            _heightFromDiffuseSettings.Blur5Contrast = -2.0f;
            _heightFromDiffuseSettings.Blur6Contrast = -4.0f;
            StuffToBeDone = true;
        }

        private void SetContrastEqFunky()
        {
            _heightFromDiffuseSettings.Blur0Contrast = -3.0f;
            _heightFromDiffuseSettings.Blur1Contrast = -1.2f;
            _heightFromDiffuseSettings.Blur2Contrast = 0.30f;
            _heightFromDiffuseSettings.Blur3Contrast = 1.3f;
            _heightFromDiffuseSettings.Blur4Contrast = 2.0f;
            _heightFromDiffuseSettings.Blur5Contrast = 2.5f;
            _heightFromDiffuseSettings.Blur6Contrast = 2.0f;
            StuffToBeDone = true;
        }

        private void SelectColor()
        {
            if (Input.GetMouseButton(0))
            {
                _mouseButtonDown = true;
                if (!_camera) return;

                if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit))
                    return;

                var rend = hit.transform.GetComponent<Renderer>();
                var meshCollider = hit.collider as MeshCollider;
                if (!rend || !rend.sharedMaterial || !rend.sharedMaterial.mainTexture ||
                    !meshCollider)
                    return;

                var pixelUv = hit.textureCoord;

                var useAdjusted = _heightFromDiffuseSettings.UseAdjustedDiffuse;
                var sampledColor = useAdjusted
                    ? TextureManager.Instance.DiffuseMap.GetPixelBilinear(pixelUv.x, pixelUv.y)
                    : TextureManager.Instance.DiffuseMapOriginal.GetPixelBilinear(pixelUv.x, pixelUv.y);

                switch (_currentSelection)
                {
                    case 1:
                        _heightFromDiffuseSettings.SampleUv1 = pixelUv;
                        _heightFromDiffuseSettings.SampleColor1 = sampledColor;
                        _sampleColorMap1.SetPixel(1, 1, _heightFromDiffuseSettings.SampleColor1);
                        _sampleColorMap1.Apply(false);
                        break;
                    case 2:
                        _heightFromDiffuseSettings.SampleUv2 = pixelUv;
                        _heightFromDiffuseSettings.SampleColor2 = sampledColor;
                        _sampleColorMap2.SetPixel(1, 1, _heightFromDiffuseSettings.SampleColor2);
                        _sampleColorMap2.Apply(false);
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                StuffToBeDone = true;
            }

            if (!Input.GetMouseButtonUp(0) || !_mouseButtonDown) return;

            _mouseButtonDown = false;
            _selectingColor = false;
            _currentSelection = 0;
        }

        // Update is called once per frame
        private void Update()
        {
            if (ProgramManager.IsLocked) return;
            if (_selectingColor) SelectColor();

            if (_heightFromDiffuseSettings.UseAdjustedDiffuse != _lastUseDiffuse)
            {
                _lastUseDiffuse = _heightFromDiffuseSettings.UseAdjustedDiffuse;
                StuffToBeDone = true;
            }

            if (_heightFromDiffuseSettings.UseOriginalDiffuse != _lastUseOriginalDiffuse)
            {
                _lastUseOriginalDiffuse = _heightFromDiffuseSettings.UseOriginalDiffuse;
                StuffToBeDone = true;
            }

            if (_heightFromDiffuseSettings.UseNormal != _lastUseNormal)
            {
                _lastUseNormal = _heightFromDiffuseSettings.UseNormal;
                StuffToBeDone = true;
            }

            if (Math.Abs(_heightFromDiffuseSettings.Blur0Contrast - _lastBlur0Contrast) > 0.001f)
            {
                _lastBlur0Contrast = _heightFromDiffuseSettings.Blur0Contrast;
                StuffToBeDone = true;
            }

            if (IsNewTexture)
            {
                InitializeTextures();
                IsNewTexture = false;
            }

            if (StuffToBeDone)
            {
                if (_heightFromDiffuseSettings.UseNormal)
                {
                    StopAllCoroutines();
                    StartCoroutine(ProcessNormal());
                }
                else
                {
                    StopAllCoroutines();
                    StartCoroutine(ProcessDiffuse());
                }

                StuffToBeDone = false;
            }

            if (_heightFromDiffuseSettings.IsolateSample1 || _heightFromDiffuseSettings.IsolateSample2)
                ThisMaterial.SetInt(Isolate, 1);
            else
                ThisMaterial.SetInt(Isolate, 0);

            ThisMaterial.SetFloat(Blur0Weight, _heightFromDiffuseSettings.Blur0Weight);
            ThisMaterial.SetFloat(Blur1Weight, _heightFromDiffuseSettings.Blur1Weight);
            ThisMaterial.SetFloat(Blur2Weight, _heightFromDiffuseSettings.Blur2Weight);
            ThisMaterial.SetFloat(Blur3Weight, _heightFromDiffuseSettings.Blur3Weight);
            ThisMaterial.SetFloat(Blur4Weight, _heightFromDiffuseSettings.Blur4Weight);
            ThisMaterial.SetFloat(Blur5Weight, _heightFromDiffuseSettings.Blur5Weight);
            ThisMaterial.SetFloat(Blur6Weight, _heightFromDiffuseSettings.Blur6Weight);

            ThisMaterial.SetFloat(Blur0Contrast, _heightFromDiffuseSettings.Blur0Contrast);
            ThisMaterial.SetFloat(Blur1Contrast, _heightFromDiffuseSettings.Blur1Contrast);
            ThisMaterial.SetFloat(Blur2Contrast, _heightFromDiffuseSettings.Blur2Contrast);
            ThisMaterial.SetFloat(Blur3Contrast, _heightFromDiffuseSettings.Blur3Contrast);
            ThisMaterial.SetFloat(Blur4Contrast, _heightFromDiffuseSettings.Blur4Contrast);
            ThisMaterial.SetFloat(Blur5Contrast, _heightFromDiffuseSettings.Blur5Contrast);
            ThisMaterial.SetFloat(Blur6Contrast, _heightFromDiffuseSettings.Blur6Contrast);

            var realGain = _heightFromDiffuseSettings.FinalGain;
            if (realGain < 0.0f)
                realGain = Mathf.Abs(1.0f / (realGain - 1.0f));
            else
                realGain = realGain + 1.0f;

            ThisMaterial.SetFloat(FinalGain, realGain);
            ThisMaterial.SetFloat(FinalContrast, _heightFromDiffuseSettings.FinalContrast);
            ThisMaterial.SetFloat(FinalBias, _heightFromDiffuseSettings.FinalBias);

            ThisMaterial.SetFloat(Slider, _slider);
        }

        private void DoMyWindow(int windowId)
        {
            var offsetX = 10;
            var offsetY = 20;

            GUI.enabled = TextureManager.Instance.DiffuseMap != null;
            _heightFromDiffuseSettings.UseAdjustedDiffuse = GUI.Toggle(new Rect(offsetX, offsetY, 80, 30),
                _heightFromDiffuseSettings.UseAdjustedDiffuse, " Diffuse");
            if (_heightFromDiffuseSettings.UseAdjustedDiffuse)
            {
                _heightFromDiffuseSettings.UseOriginalDiffuse = false;
                _heightFromDiffuseSettings.UseNormal = false;
            }
            else if (!_heightFromDiffuseSettings.UseOriginalDiffuse && !_heightFromDiffuseSettings.UseNormal)
            {
                _heightFromDiffuseSettings.UseAdjustedDiffuse = true;
            }

            GUI.enabled = TextureManager.Instance.DiffuseMapOriginal != null;
            _heightFromDiffuseSettings.UseOriginalDiffuse = GUI.Toggle(new Rect(offsetX + 80, offsetY, 120, 30),
                _heightFromDiffuseSettings.UseOriginalDiffuse,
                "Original Diffuse");
            if (_heightFromDiffuseSettings.UseOriginalDiffuse)
            {
                _heightFromDiffuseSettings.UseAdjustedDiffuse = false;
                _heightFromDiffuseSettings.UseNormal = false;
            }
            else if (!_heightFromDiffuseSettings.UseAdjustedDiffuse && !_heightFromDiffuseSettings.UseNormal)
            {
                _heightFromDiffuseSettings.UseOriginalDiffuse = true;
            }

            GUI.enabled = TextureManager.Instance.NormalMap;
            _heightFromDiffuseSettings.UseNormal = GUI.Toggle(new Rect(offsetX + 210, offsetY, 80, 30),
                _heightFromDiffuseSettings.UseNormal, " Normal");
            if (_heightFromDiffuseSettings.UseNormal)
            {
                _heightFromDiffuseSettings.UseAdjustedDiffuse = false;
                _heightFromDiffuseSettings.UseOriginalDiffuse = false;
            }
            else if (!_heightFromDiffuseSettings.UseAdjustedDiffuse && !_heightFromDiffuseSettings.UseOriginalDiffuse)
            {
                _heightFromDiffuseSettings.UseNormal = true;
            }

            GUI.enabled = true;
            offsetY += 20;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Height Reveal Slider");
            _slider = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 20, 280, 10), _slider, 0.0f, 1.0f);
            offsetY += 35;

            if (_heightFromDiffuseSettings.UseNormal)
            {
                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 10), "Sample Spread",
                    _heightFromDiffuseSettings.Spread, out _heightFromDiffuseSettings.Spread, 10.0f, 200.0f))
                    StuffToBeDone = true;

                offsetY += 35;

                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 10), "Sample Spread Boost",
                    _heightFromDiffuseSettings.SpreadBoost, out _heightFromDiffuseSettings.SpreadBoost,
                    1.0f, 5.0f)) StuffToBeDone = true;

                offsetY += 35;
            }
            else
            {
                GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Frequency Weight Equalizer");
                GUI.Label(new Rect(offsetX + 225, offsetY, 100, 30), "Presets");
                if (GUI.Button(new Rect(offsetX + 215, offsetY + 30, 60, 20), "Default")) SetWeightEqDefault();
                if (GUI.Button(new Rect(offsetX + 215, offsetY + 60, 60, 20), "Details")) SetWeightEqDetail();
                if (GUI.Button(new Rect(offsetX + 215, offsetY + 90, 60, 20), "Displace")) SetWeightEqDisplace();

                offsetY += 30;
                offsetX += 10;
                _heightFromDiffuseSettings.Blur0Weight =
                    GUI.VerticalSlider(new Rect(offsetX + 180, offsetY, 10, 80), _heightFromDiffuseSettings.Blur0Weight,
                        1.0f, 0.0f);
                _heightFromDiffuseSettings.Blur1Weight =
                    GUI.VerticalSlider(new Rect(offsetX + 150, offsetY, 10, 80), _heightFromDiffuseSettings.Blur1Weight,
                        1.0f, 0.0f);
                _heightFromDiffuseSettings.Blur2Weight =
                    GUI.VerticalSlider(new Rect(offsetX + 120, offsetY, 10, 80), _heightFromDiffuseSettings.Blur2Weight,
                        1.0f, 0.0f);
                _heightFromDiffuseSettings.Blur3Weight =
                    GUI.VerticalSlider(new Rect(offsetX + 90, offsetY, 10, 80), _heightFromDiffuseSettings.Blur3Weight,
                        1.0f, 0.0f);
                _heightFromDiffuseSettings.Blur4Weight =
                    GUI.VerticalSlider(new Rect(offsetX + 60, offsetY, 10, 80), _heightFromDiffuseSettings.Blur4Weight,
                        1.0f, 0.0f);
                _heightFromDiffuseSettings.Blur5Weight =
                    GUI.VerticalSlider(new Rect(offsetX + 30, offsetY, 10, 80), _heightFromDiffuseSettings.Blur5Weight,
                        1.0f, 0.0f);
                _heightFromDiffuseSettings.Blur6Weight = GUI.VerticalSlider(new Rect(offsetX + 0, offsetY, 10, 80),
                    _heightFromDiffuseSettings.Blur6Weight, 1.0f, 0.0f);
                offsetX -= 10;
                offsetY += 100;


                GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Frequency Contrast Equalizer");
                GUI.Label(new Rect(offsetX + 225, offsetY, 100, 30), "Presets");
                if (GUI.Button(new Rect(offsetX + 215, offsetY + 30, 60, 20), "Default")) SetContrastEqDefault();
                if (GUI.Button(new Rect(offsetX + 215, offsetY + 60, 60, 20), "Cracks")) SetContrastEqCrackedMud();
                if (GUI.Button(new Rect(offsetX + 215, offsetY + 90, 60, 20), "Funky")) SetContrastEqFunky();
                offsetY += 30;
                offsetX += 10;
                _heightFromDiffuseSettings.Blur0Contrast =
                    GUI.VerticalSlider(new Rect(offsetX + 180, offsetY, 10, 80),
                        _heightFromDiffuseSettings.Blur0Contrast,
                        5.0f, -5.0f);
                _heightFromDiffuseSettings.Blur1Contrast =
                    GUI.VerticalSlider(new Rect(offsetX + 150, offsetY, 10, 80),
                        _heightFromDiffuseSettings.Blur1Contrast,
                        5.0f, -5.0f);
                _heightFromDiffuseSettings.Blur2Contrast =
                    GUI.VerticalSlider(new Rect(offsetX + 120, offsetY, 10, 80),
                        _heightFromDiffuseSettings.Blur2Contrast,
                        5.0f, -5.0f);
                _heightFromDiffuseSettings.Blur3Contrast =
                    GUI.VerticalSlider(new Rect(offsetX + 90, offsetY, 10, 80),
                        _heightFromDiffuseSettings.Blur3Contrast,
                        5.0f, -5.0f);
                _heightFromDiffuseSettings.Blur4Contrast =
                    GUI.VerticalSlider(new Rect(offsetX + 60, offsetY, 10, 80),
                        _heightFromDiffuseSettings.Blur4Contrast,
                        5.0f, -5.0f);
                _heightFromDiffuseSettings.Blur5Contrast =
                    GUI.VerticalSlider(new Rect(offsetX + 30, offsetY, 10, 80),
                        _heightFromDiffuseSettings.Blur5Contrast,
                        5.0f, -5.0f);
                _heightFromDiffuseSettings.Blur6Contrast =
                    GUI.VerticalSlider(new Rect(offsetX + 0, offsetY, 10, 80), _heightFromDiffuseSettings.Blur6Contrast,
                        5.0f, -5.0f);
                offsetX -= 10;
                GUI.Label(new Rect(offsetX + 210, offsetY + 21, 30, 30), "-");
                GUI.Label(new Rect(offsetX + 180, offsetY + 21, 30, 30), "-");
                GUI.Label(new Rect(offsetX + 150, offsetY + 21, 30, 30), "-");
                GUI.Label(new Rect(offsetX + 120, offsetY + 21, 30, 30), "-");
                GUI.Label(new Rect(offsetX + 90, offsetY + 21, 30, 30), "-");
                GUI.Label(new Rect(offsetX + 60, offsetY + 21, 30, 30), "-");
                GUI.Label(new Rect(offsetX + 30, offsetY + 21, 30, 30), "-");
                GUI.Label(new Rect(offsetX + 0, offsetY + 21, 30, 30), "-");
                offsetY += 100;


                StuffToBeDone = GuiHelper.Toggle(new Rect(offsetX, offsetY, 150, 20),
                    _heightFromDiffuseSettings.UseSample1,
                    out _heightFromDiffuseSettings.UseSample1,
                    "Use Color Sample 1", StuffToBeDone);
                if (_heightFromDiffuseSettings.UseSample1)
                {
                    StuffToBeDone = GuiHelper.Toggle(new Rect(offsetX + 180, offsetY, 150, 20),
                        _heightFromDiffuseSettings.IsolateSample1,
                        out _heightFromDiffuseSettings.IsolateSample1, "Isolate Mask", StuffToBeDone);
                    if (_heightFromDiffuseSettings.IsolateSample1) _heightFromDiffuseSettings.IsolateSample2 = false;
                    offsetY += 30;

                    if (GUI.Button(new Rect(offsetX, offsetY + 5, 80, 20), "Pick Color"))
                    {
                        _selectingColor = true;
                        _currentSelection = 1;
                    }

                    GUI.DrawTexture(new Rect(offsetX + 10, offsetY + 35, 60, 60), _sampleColorMap1);

                    GUI.Label(new Rect(offsetX + 90, offsetY, 250, 30), "Hue");
                    StuffToBeDone = GuiHelper.VerticalSlider(new Rect(offsetX + 95, offsetY + 30, 10, 70),
                        _heightFromDiffuseSettings.HueWeight1,
                        out _heightFromDiffuseSettings.HueWeight1, 1.0f, 0.0f, StuffToBeDone);

                    GUI.Label(new Rect(offsetX + 120, offsetY, 250, 30), "Sat");
                    StuffToBeDone = GuiHelper.VerticalSlider(new Rect(offsetX + 125, offsetY + 30, 10, 70),
                        _heightFromDiffuseSettings.SatWeight1,
                        out _heightFromDiffuseSettings.SatWeight1, 1.0f, 0.0f, StuffToBeDone);

                    GUI.Label(new Rect(offsetX + 150, offsetY, 250, 30), "Lum");
                    StuffToBeDone = GuiHelper.VerticalSlider(new Rect(offsetX + 155, offsetY + 30, 10, 70),
                        _heightFromDiffuseSettings.LumWeight1,
                        out _heightFromDiffuseSettings.LumWeight1, 1.0f, 0.0f, StuffToBeDone);

                    GUI.Label(new Rect(offsetX + 180, offsetY, 250, 30), "Low");
                    StuffToBeDone = GuiHelper.VerticalSlider(new Rect(offsetX + 185, offsetY + 30, 10, 70),
                        _heightFromDiffuseSettings.MaskLow1,
                        out _heightFromDiffuseSettings.MaskLow1, 1.0f, 0.0f, StuffToBeDone);

                    GUI.Label(new Rect(offsetX + 210, offsetY, 250, 30), "High");
                    StuffToBeDone = GuiHelper.VerticalSlider(new Rect(offsetX + 215, offsetY + 30, 10, 70),
                        _heightFromDiffuseSettings.MaskHigh1,
                        out _heightFromDiffuseSettings.MaskHigh1, 1.0f, 0.0f, StuffToBeDone);

                    GUI.Label(new Rect(offsetX + 240, offsetY, 250, 30), "Height");
                    StuffToBeDone = GuiHelper.VerticalSlider(new Rect(offsetX + 255, offsetY + 30, 10, 70),
                        _heightFromDiffuseSettings.Sample1Height,
                        out _heightFromDiffuseSettings.Sample1Height, 1.0f, 0.0f, StuffToBeDone);

                    offsetY += 110;
                }
                else
                {
                    offsetY += 30;
                    _heightFromDiffuseSettings.IsolateSample1 = false;
                }


                StuffToBeDone = GuiHelper.Toggle(new Rect(offsetX, offsetY, 150, 20),
                    _heightFromDiffuseSettings.UseSample2,
                    out _heightFromDiffuseSettings.UseSample2,
                    "Use Color Sample 2", StuffToBeDone);
                if (_heightFromDiffuseSettings.UseSample2)
                {
                    StuffToBeDone = GuiHelper.Toggle(new Rect(offsetX + 180, offsetY, 150, 20),
                        _heightFromDiffuseSettings.IsolateSample2,
                        out _heightFromDiffuseSettings.IsolateSample2, "Isolate Mask", StuffToBeDone);
                    if (_heightFromDiffuseSettings.IsolateSample2) _heightFromDiffuseSettings.IsolateSample1 = false;
                    offsetY += 30;

                    if (GUI.Button(new Rect(offsetX, offsetY + 5, 80, 20), "Pick Color"))
                    {
                        _selectingColor = true;
                        _currentSelection = 2;
                    }

                    GUI.DrawTexture(new Rect(offsetX + 10, offsetY + 35, 60, 60), _sampleColorMap2);

                    GUI.Label(new Rect(offsetX + 90, offsetY, 250, 30), "Hue");
                    StuffToBeDone = GuiHelper.VerticalSlider(new Rect(offsetX + 95, offsetY + 30, 10, 70),
                        _heightFromDiffuseSettings.HueWeight2,
                        out _heightFromDiffuseSettings.HueWeight2, 1.0f, 0.0f, StuffToBeDone);

                    GUI.Label(new Rect(offsetX + 120, offsetY, 250, 30), "Sat");
                    StuffToBeDone = GuiHelper.VerticalSlider(new Rect(offsetX + 125, offsetY + 30, 10, 70),
                        _heightFromDiffuseSettings.SatWeight2,
                        out _heightFromDiffuseSettings.SatWeight2, 1.0f, 0.0f, StuffToBeDone);

                    GUI.Label(new Rect(offsetX + 150, offsetY, 250, 30), "Lum");
                    StuffToBeDone = GuiHelper.VerticalSlider(new Rect(offsetX + 155, offsetY + 30, 10, 70),
                        _heightFromDiffuseSettings.LumWeight2,
                        out _heightFromDiffuseSettings.LumWeight2, 1.0f, 0.0f, StuffToBeDone);

                    GUI.Label(new Rect(offsetX + 180, offsetY, 250, 30), "Low");
                    StuffToBeDone = GuiHelper.VerticalSlider(new Rect(offsetX + 185, offsetY + 30, 10, 70),
                        _heightFromDiffuseSettings.MaskLow2,
                        out _heightFromDiffuseSettings.MaskLow2, 1.0f, 0.0f, StuffToBeDone);

                    GUI.Label(new Rect(offsetX + 210, offsetY, 250, 30), "High");
                    StuffToBeDone = GuiHelper.VerticalSlider(new Rect(offsetX + 215, offsetY + 30, 10, 70),
                        _heightFromDiffuseSettings.MaskHigh2,
                        out _heightFromDiffuseSettings.MaskHigh2, 1.0f, 0.0f, StuffToBeDone);

                    GUI.Label(new Rect(offsetX + 240, offsetY, 250, 30), "Height");
                    StuffToBeDone = GuiHelper.VerticalSlider(new Rect(offsetX + 255, offsetY + 30, 10, 70),
                        _heightFromDiffuseSettings.Sample2Height,
                        out _heightFromDiffuseSettings.Sample2Height, 1.0f, 0.0f, StuffToBeDone);

                    offsetY += 110;
                }
                else
                {
                    offsetY += 30;
                    _heightFromDiffuseSettings.IsolateSample2 = false;
                }

                if (_heightFromDiffuseSettings.UseSample1 || _heightFromDiffuseSettings.UseSample2)
                {
                    if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Sample Blend",
                        _heightFromDiffuseSettings.SampleBlend, out _heightFromDiffuseSettings.SampleBlend,
                        0.0f, 1.0f)) StuffToBeDone = true;
                    offsetY += 40;
                }
            }


            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Gain", _heightFromDiffuseSettings.FinalGain,
                out _heightFromDiffuseSettings.FinalGain, -0.5f, 0.5f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Contrast",
                _heightFromDiffuseSettings.FinalContrast, out _heightFromDiffuseSettings.FinalContrast,
                -10.0f, 10.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Bias", _heightFromDiffuseSettings.FinalBias,
                out _heightFromDiffuseSettings.FinalBias, -1.0f, 1.0f);
            GUI.enabled = true;

            GUI.DragWindow();
        }

        private void OnGUI()
        {
            if (Hide) return;

            var rect = _windowRect;
            if (_heightFromDiffuseSettings.UseSample1 && !_heightFromDiffuseSettings.UseNormal)
                rect.height += 110;

            if (_heightFromDiffuseSettings.UseSample2 && !_heightFromDiffuseSettings.UseNormal)
                rect.height += 110;

            if ((_heightFromDiffuseSettings.UseSample1 || _heightFromDiffuseSettings.UseSample2) &&
                !_heightFromDiffuseSettings.UseNormal) rect.height += 40;


            MainGui.MakeScaledWindow(rect, _windowId, DoMyWindow, "Height From Diffuse");
        }

        public void InitializeTextures()
        {
            TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

            CleanupTextures();

            FixUseMaps();

            if (_heightFromDiffuseSettings.UseAdjustedDiffuse)
            {
                ImageSize.x = TextureManager.Instance.DiffuseMap.width;
                ImageSize.y = TextureManager.Instance.DiffuseMap.height;
            }
            else if (_heightFromDiffuseSettings.UseOriginalDiffuse)
            {
                ImageSize.x = TextureManager.Instance.DiffuseMapOriginal.width;
                ImageSize.y = TextureManager.Instance.DiffuseMapOriginal.height;
            }
            else if (_heightFromDiffuseSettings.UseNormal)
            {
                ImageSize.x = TextureManager.Instance.NormalMap.width;
                ImageSize.y = TextureManager.Instance.NormalMap.height;
            }


//        General.Logger.Log("Initializing Textures of size: " + ImageSize.x + "x" + ImageSize.y);

            _tempBlurMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y, false, true);
            _blurMap0 = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y, false, true);
            _blurMap1 = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y, false, true);
            _blurMap2 = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y, false, true);
            _blurMap3 = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y, false, true);
            _blurMap4 = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y, false, true);
            _blurMap5 = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y, false, true);
            _blurMap6 = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y, false, true);
            _avgMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y, false, true);
            _avgTempMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y, false, true);
            SetMaterialValues();
        }

        protected override void CleanupTextures()
        {
            RenderTexture.ReleaseTemporary(_tempBlurMap);
            RenderTexture.ReleaseTemporary(_blurMap0);
            RenderTexture.ReleaseTemporary(_blurMap1);
            RenderTexture.ReleaseTemporary(_blurMap2);
            RenderTexture.ReleaseTemporary(_blurMap3);
            RenderTexture.ReleaseTemporary(_blurMap4);
            RenderTexture.ReleaseTemporary(_blurMap5);
            RenderTexture.ReleaseTemporary(_blurMap6);
            RenderTexture.ReleaseTemporary(_tempHeightMap);
            RenderTexture.ReleaseTemporary(_avgMap);
            RenderTexture.ReleaseTemporary(_avgTempMap);
        }

        private IEnumerator ProcessNormal()
        {
            while (!ProgramManager.Lock()) yield return null;

            var kernelNormal = HeightCompute.FindKernel("CSHeightFromNormal");

            MessagePanel.ShowMessage("Processing Normal for Height Map");

            HeightCompute.SetVector(ImageSizeId, new Vector4(ImageSize.x, ImageSize.y, 0, 0));
            HeightCompute.SetFloat(Spread, _heightFromDiffuseSettings.Spread);
            HeightCompute.SetFloat(SpreadBoost, _heightFromDiffuseSettings.SpreadBoost);
            HeightCompute.SetInt(Samples, (int) _heightFromDiffuseSettings.Spread);
            HeightCompute.SetTexture(kernelNormal, BlendTex, _blurMap1);
            HeightCompute.SetTexture(kernelNormal, "ImageInput", TextureManager.Instance.NormalMap);
            HeightCompute.SetTexture(kernelNormal, "Result", _blurMap0);

            ThisMaterial.SetFloat(IsNormal, 1.0f);
            ThisMaterial.SetTexture(BlurTex0, _blurMap0);
            ThisMaterial.SetTexture(BlurTex1, _blurMap1);
            ThisMaterial.SetTexture(MainTex, TextureManager.Instance.NormalMap);
            var groupsX = (int) Mathf.Ceil(ImageSize.x / 8f);
            var groupsY = (int) Mathf.Ceil(ImageSize.y / 8f);

            for (var i = 1; i < 100; i++)
            {
                HeightCompute.SetFloat(BlendAmount, 1.0f / i);
                HeightCompute.SetFloat(Progress, i / 100.0f);
                HeightCompute.Dispatch(kernelNormal, groupsX, groupsY, 1);

                Graphics.Blit(_blurMap0, _blurMap1);

                if (i % 10 == 0) yield return new WaitForSeconds(0.01f);
            }

            yield return null;

            IsReadyToProcess = true;
            MessagePanel.HideMessage();

            ProgramManager.Unlock();
        }

        public IEnumerator ProcessDiffuse()
        {
            while (!ProgramManager.Lock()) yield return null;

            MessagePanel.ShowMessage("Processing Diffuse for Height Map");

            var kernelSample = SampleCompute.FindKernel("CSSample");

            ThisMaterial.SetFloat(IsNormal, 0.0f);
            SampleCompute.SetVector(ImageSizeId, new Vector2(ImageSize.x, ImageSize.y));

            SampleCompute.SetInt(IsolateSample1, _heightFromDiffuseSettings.IsolateSample1 ? 1 : 0);
            SampleCompute.SetInt(UseSample1, _heightFromDiffuseSettings.UseSample1 ? 1 : 0);
            SampleCompute.SetVector(SampleColor1, _heightFromDiffuseSettings.SampleColor1);
            SampleCompute.SetVector(SampleUv1,
                new Vector4(_heightFromDiffuseSettings.SampleUv1.x, _heightFromDiffuseSettings.SampleUv1.y, 0, 0));
            SampleCompute.SetFloat(HueWeight1, _heightFromDiffuseSettings.HueWeight1);
            SampleCompute.SetFloat(SatWeight1, _heightFromDiffuseSettings.SatWeight1);
            SampleCompute.SetFloat(LumWeight1, _heightFromDiffuseSettings.LumWeight1);
            SampleCompute.SetFloat(MaskLow1, _heightFromDiffuseSettings.MaskLow1);
            SampleCompute.SetFloat(MaskHigh1, _heightFromDiffuseSettings.MaskHigh1);
            SampleCompute.SetFloat(Sample1Height, _heightFromDiffuseSettings.Sample1Height);

            SampleCompute.SetInt(IsolateSample2, _heightFromDiffuseSettings.IsolateSample2 ? 1 : 0);
            SampleCompute.SetInt(UseSample2, _heightFromDiffuseSettings.UseSample2 ? 1 : 0);
            SampleCompute.SetVector(SampleColor2, _heightFromDiffuseSettings.SampleColor2);
            SampleCompute.SetVector(SampleUv2,
                new Vector4(_heightFromDiffuseSettings.SampleUv2.x, _heightFromDiffuseSettings.SampleUv2.y, 0, 0));
            SampleCompute.SetFloat(HueWeight2, _heightFromDiffuseSettings.HueWeight2);
            SampleCompute.SetFloat(SatWeight2, _heightFromDiffuseSettings.SatWeight2);
            SampleCompute.SetFloat(LumWeight2, _heightFromDiffuseSettings.LumWeight2);
            SampleCompute.SetFloat(MaskLow2, _heightFromDiffuseSettings.MaskLow2);
            SampleCompute.SetFloat(MaskHigh2, _heightFromDiffuseSettings.MaskHigh2);
            SampleCompute.SetFloat(Sample2Height, _heightFromDiffuseSettings.Sample2Height);

            if (_heightFromDiffuseSettings.UseSample1 == false && _heightFromDiffuseSettings.UseSample2 == false)
                SampleCompute.SetFloat(SampleBlend, 0.0f);
            else
                SampleCompute.SetFloat(SampleBlend, _heightFromDiffuseSettings.SampleBlend);

            SampleCompute.SetFloat(FinalContrast, _heightFromDiffuseSettings.FinalContrast);
            SampleCompute.SetFloat(FinalBias, _heightFromDiffuseSettings.FinalBias);

            var source = _heightFromDiffuseSettings.UseOriginalDiffuse
                ? TextureManager.Instance.DiffuseMapOriginal
                : TextureManager.Instance.DiffuseMap;

            SampleCompute.SetTexture(kernelSample, "ImageInput", source);
            SampleCompute.SetTexture(kernelSample, "Result", _blurMap0);
            var groupsX = (int) Mathf.Ceil(ImageSize.x / 8f);
            var groupsY = (int) Mathf.Ceil(ImageSize.y / 8f);
            SampleCompute.Dispatch(kernelSample, groupsX, groupsY, 1);

            BlurCompute.SetVector(ImageSizeId, new Vector2(ImageSize.x, ImageSize.y));
//            BlurCompute.SetInt("_Desaturate", 1);
            BlurCompute.SetFloat(BlurContrast, 1.0f);

            var extraSpread = (_blurMap0.width + _blurMap0.height) * (0.5f / 1024.0f);
            var spread = 1.0f;

            BlurCompute.SetInt(BlurSamples, 4);

            // Blur the image 1
            BlurImage(spread, _blurMap0, _blurMap1);

            spread += extraSpread;

            // Blur the image 2
            BlurImage(spread, _blurMap1, _blurMap2);

            spread += 2 * extraSpread;

            // Blur the image 3
            BlurImage(spread, _blurMap2, _blurMap3);

            spread += 4 * extraSpread;

            // Blur the image 4
            BlurImage(spread, _blurMap3, _blurMap4);

            spread += 8 * extraSpread;

            // Blur the image 5
            BlurImage(spread, _blurMap4, _blurMap5);

            spread += 16 * extraSpread;

            // Blur the image 6
            BlurImage(spread, _blurMap5, _blurMap6);

            // Average Color
            BlurCompute.SetInt(BlurSamples, 32);
            BlurCompute.SetFloat(BlurSpread, 64.0f * extraSpread);
            BlurCompute.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            BlurCompute.SetTexture(_kernelBlur, "ImageInput", _blurMap6);
            BlurCompute.SetTexture(_kernelBlur, "Result", _avgTempMap);

            BlurCompute.Dispatch(_kernelBlur, groupsX, groupsY, 1);
            BlurCompute.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            BlurCompute.SetTexture(_kernelBlur, "ImageInput", _avgTempMap);
            BlurCompute.SetTexture(_kernelBlur, "Result", _avgMap);
            BlurCompute.Dispatch(_kernelBlur, groupsX, groupsY, 1);

            ThisMaterial.SetTexture(MainTex,
                _heightFromDiffuseSettings.UseOriginalDiffuse
                    ? TextureManager.Instance.DiffuseMapOriginal
                    : TextureManager.Instance.DiffuseMap);

            ThisMaterial.SetTexture(BlurTex0, _blurMap0);
            ThisMaterial.SetTexture(BlurTex1, _blurMap1);
            ThisMaterial.SetTexture(BlurTex2, _blurMap2);
            ThisMaterial.SetTexture(BlurTex3, _blurMap3);
            ThisMaterial.SetTexture(BlurTex4, _blurMap4);
            ThisMaterial.SetTexture(BlurTex5, _blurMap5);
            ThisMaterial.SetTexture(BlurTex6, _blurMap6);
            ThisMaterial.SetTexture(AvgTex, _avgMap);

            yield return null;
            yield return null;

            IsReadyToProcess = true;

            MessagePanel.HideMessage();

            ProgramManager.Unlock();
        }

        private void BlurImage(float spread, Texture source, Texture dest)
        {
            BlurCompute.SetFloat(BlurSpread, spread);
            BlurCompute.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            BlurCompute.SetTexture(_kernelBlur, "ImageInput", source);
            BlurCompute.SetTexture(_kernelBlur, "Result", _tempBlurMap);
            var groupsX = (int) Mathf.Ceil(ImageSize.x / 8f);
            var groupsY = (int) Mathf.Ceil(ImageSize.y / 8f);
            BlurCompute.Dispatch(_kernelBlur, groupsX, groupsY, 1);
            BlurCompute.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            BlurCompute.SetTexture(_kernelBlur, "ImageInput", _tempBlurMap);
            BlurCompute.SetTexture(_kernelBlur, "Result", dest);
            BlurCompute.Dispatch(_kernelBlur, groupsX, groupsY, 1);
        }
    }
}