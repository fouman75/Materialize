#region

using System;
using System.Collections;
using Materialize.General;
using Materialize.Settings;
using UnityEngine;
using Utility;
using Logger = Utility.Logger;

#endregion

namespace Materialize.Gui
{
    public class SmoothnessGui : TexturePanelGui
    {
        private RenderTexture _blurMap;
        private Camera _camera;

        private int _currentSelection;

        private Texture2D _diffuseMap;
        private Texture2D _diffuseMapOriginal;

        private bool _lastUseAdjustedDiffuse;

        private Texture2D _metallicMap;
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

        public ComputeShader SmoothnessCompute;

        protected override IEnumerator Process()
        {
            MessagePanel.ShowMessage("Processing Smoothness Map");

            var smoothnessKernel = SmoothnessCompute.FindKernel("CSSmoothness");
            SmoothnessCompute.SetVector(ImageSizeId, new Vector2(ImageSize.x, ImageSize.y));

            SmoothnessCompute.SetTexture(smoothnessKernel, MetallicTex,
                _metallicMap != null ? _metallicMap : Texture2D.blackTexture);

            SmoothnessCompute.SetTexture(smoothnessKernel, BlurTex, _blurMap);

            SmoothnessCompute.SetTexture(smoothnessKernel, OverlayBlurTex, _overlayBlurMap);

            SmoothnessCompute.SetFloat(MetalSmoothness, _settings.MetalSmoothness);

            SmoothnessCompute.SetInt(UseSample1, _settings.UseSample1 ? 1 : 0);
            SmoothnessCompute.SetVector(SampleColor1, _settings.SampleColor1);
            SmoothnessCompute.SetVector(SampleUv1,
                new Vector4(_settings.SampleUv1.x, _settings.SampleUv1.y, 0, 0));
            SmoothnessCompute.SetFloat(HueWeight1, _settings.HueWeight1);
            SmoothnessCompute.SetFloat(SatWeight1, _settings.SatWeight1);
            SmoothnessCompute.SetFloat(LumWeight1, _settings.LumWeight1);
            SmoothnessCompute.SetFloat(MaskLow1, _settings.MaskLow1);
            SmoothnessCompute.SetFloat(MaskHigh1, _settings.MaskHigh1);
            SmoothnessCompute.SetFloat(Sample1Smoothness, _settings.Sample1Smoothness);

            SmoothnessCompute.SetInt(UseSample2, _settings.UseSample2 ? 1 : 0);
            SmoothnessCompute.SetVector(SampleColor2, _settings.SampleColor2);
            SmoothnessCompute.SetVector(SampleUv2,
                new Vector4(_settings.SampleUv2.x, _settings.SampleUv2.y, 0, 0));
            SmoothnessCompute.SetFloat(HueWeight2, _settings.HueWeight2);
            SmoothnessCompute.SetFloat(SatWeight2, _settings.SatWeight2);
            SmoothnessCompute.SetFloat(LumWeight2, _settings.LumWeight2);
            SmoothnessCompute.SetFloat(MaskLow2, _settings.MaskLow2);
            SmoothnessCompute.SetFloat(MaskHigh2, _settings.MaskHigh2);
            SmoothnessCompute.SetFloat(Sample2Smoothness, _settings.Sample2Smoothness);

            SmoothnessCompute.SetInt(UseSample3, _settings.UseSample3 ? 1 : 0);
            SmoothnessCompute.SetVector(SampleColor3, _settings.SampleColor3);
            SmoothnessCompute.SetVector(SampleUv3,
                new Vector4(_settings.SampleUv3.x, _settings.SampleUv3.y, 0, 0));
            SmoothnessCompute.SetFloat(HueWeight3, _settings.HueWeight3);
            SmoothnessCompute.SetFloat(SatWeight3, _settings.SatWeight3);
            SmoothnessCompute.SetFloat(LumWeight3, _settings.LumWeight3);
            SmoothnessCompute.SetFloat(MaskLow3, _settings.MaskLow3);
            SmoothnessCompute.SetFloat(MaskHigh3, _settings.MaskHigh3);
            SmoothnessCompute.SetFloat(Sample3Smoothness, _settings.Sample3Smoothness);

            SmoothnessCompute.SetFloat(BaseSmoothness, _settings.BaseSmoothness);

            SmoothnessCompute.SetFloat(BlurOverlay, _settings.BlurOverlay);
            SmoothnessCompute.SetFloat(FinalContrast, _settings.FinalContrast);
            SmoothnessCompute.SetFloat(FinalBias, _settings.FinalBias);

            RenderTexture.ReleaseTemporary(_tempMap);
            _tempMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y);

            var groupsX = (int) Mathf.Ceil(ImageSize.x / 8f);
            var groupsY = (int) Mathf.Ceil(ImageSize.y / 8f);

            var source = _settings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal;
            SmoothnessCompute.SetTexture(smoothnessKernel, "ImageInput", source);
            SmoothnessCompute.SetTexture(smoothnessKernel, "Result", _tempMap);
            SmoothnessCompute.Dispatch(smoothnessKernel, groupsX, groupsY, 1);

            TextureManager.Instance.GetTextureFromRender(_tempMap, ProgramEnums.MapType.Smoothness);

            RenderTexture.ReleaseTemporary(_tempMap);
            yield break;
        }

        protected override void ResetSettings()
        {
            _settings.Reset();
            PostSetSettings();

            StuffToBeDone = true;
        }

        protected override TexturePanelSettings GetSettings()
        {
            return _settings;
        }

        protected override void SetSettings(TexturePanelSettings settings)
        {
            _settings = settings as SmoothnessSettings;
        }

        private void Awake()
        {
            _camera = Camera.main;
            WindowRect = new Rect(10.0f, 265.0f, 300f, 450f);
            PostAwake();
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

            PostSetSettings();

            StuffToBeDone = true;
        }

        private void PostSetSettings()
        {
            _sampleColorMap1.SetPixel(1, 1, _settings.SampleColor1);
            _sampleColorMap1.Apply(false);

            _sampleColorMap2.SetPixel(1, 1, _settings.SampleColor2);
            _sampleColorMap2.Apply(false);

            _sampleColorMap3.SetPixel(1, 1, _settings.SampleColor3);
            _sampleColorMap3.Apply(false);
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
            MessagePanel.ShowMessage("Initializing Smoothness GUI");
            TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

            InitializeSettings();
        }

        // Update is called once per frame
        private void Update()
        {
            if (ProgramManager.IsLocked) return;

            if (_selectingColor) SelectColor();

            if (IsNewTexture)
            {
                InitializeTextures();
                IsNewTexture = false;
            }

            if (_settings.UseAdjustedDiffuse != _lastUseAdjustedDiffuse)
            {
                _lastUseAdjustedDiffuse = _settings.UseAdjustedDiffuse;
                StuffToBeDone = true;
            }

            if (StuffToBeDone)
            {
                StartCoroutine(ProcessBlur());
                StuffToBeDone = false;
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
            var selectSuccess = false;
            if (Input.GetMouseButton(0))
            {
                if (!_camera) return;
                const int mask = 1 << 11;
                var wasHit = Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit,
                    Mathf.Infinity, mask, QueryTriggerInteraction.UseGlobal);

                if (!wasHit) return;

                var rend = hit.transform.GetComponent<Renderer>();
                var hasMeshCollider = hit.collider is MeshCollider;
                if (!rend || !rend.sharedMaterial || !rend.sharedMaterial.mainTexture || !hasMeshCollider) return;

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

                StuffToBeDone = true;
                selectSuccess = true;
            }

            if (!selectSuccess) return;

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

                if (GUI.Toggle(new Rect(offsetX, offsetY + 5, 80, 25), _currentSelection == 1, "Pick Color", "Button"))
                {
                    _selectingColor = true;
                    _currentSelection = 1;
                }


                GUI.DrawTexture(new Rect(offsetX + 20, offsetY + 35, 40, 40), _sampleColorMap1);

                GUI.Label(new Rect(offsetX + 90, offsetY, 250, 30), "Hue");
                _settings.HueWeight1 = GUI.VerticalSlider(new Rect(offsetX + 95, offsetY + 30, 10, 50),
                    _settings.HueWeight1, 1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 120, offsetY, 250, 30), "Sat");
                _settings.SatWeight1 =
                    GUI.VerticalSlider(new Rect(offsetX + 125, offsetY + 30, 10, 50), _settings.SatWeight1, 1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 150, offsetY, 250, 30), "Lum");
                _settings.LumWeight1 =
                    GUI.VerticalSlider(new Rect(offsetX + 155, offsetY + 30, 10, 50), _settings.LumWeight1, 1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 180, offsetY, 250, 30), "Low");
                _settings.MaskLow1 = GUI.VerticalSlider(new Rect(offsetX + 185, offsetY + 30, 10, 50),
                    _settings.MaskLow1,
                    1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 210, offsetY, 250, 30), "High");
                _settings.MaskHigh1 = GUI.VerticalSlider(new Rect(offsetX + 215, offsetY + 30, 10, 50),
                    _settings.MaskHigh1,
                    1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 240, offsetY, 250, 30), "Smooth");
                _settings.Sample1Smoothness = GUI.VerticalSlider(new Rect(offsetX + 255, offsetY + 30, 10, 50),
                    _settings.Sample1Smoothness, 1.0f, 0.0f);

                offsetY += 90;
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

                if (GUI.Toggle(new Rect(offsetX, offsetY + 5, 80, 25), _currentSelection == 2, "Pick Color", "Button"))
                {
                    _selectingColor = true;
                    _currentSelection = 2;
                }

                GUI.DrawTexture(new Rect(offsetX + 20, offsetY + 35, 40, 40), _sampleColorMap2);

                GUI.Label(new Rect(offsetX + 90, offsetY, 250, 30), "Hue");
                _settings.HueWeight2 = GUI.VerticalSlider(new Rect(offsetX + 95, offsetY + 30, 10, 50),
                    _settings.HueWeight2, 1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 120, offsetY, 250, 30), "Sat");
                _settings.SatWeight2 =
                    GUI.VerticalSlider(new Rect(offsetX + 125, offsetY + 30, 10, 50), _settings.SatWeight2, 1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 150, offsetY, 250, 30), "Lum");
                _settings.LumWeight2 =
                    GUI.VerticalSlider(new Rect(offsetX + 155, offsetY + 30, 10, 50), _settings.LumWeight2, 1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 180, offsetY, 250, 30), "Low");
                _settings.MaskLow2 = GUI.VerticalSlider(new Rect(offsetX + 185, offsetY + 30, 10, 50),
                    _settings.MaskLow2,
                    1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 210, offsetY, 250, 30), "High");
                _settings.MaskHigh2 = GUI.VerticalSlider(new Rect(offsetX + 215, offsetY + 30, 10, 50),
                    _settings.MaskHigh2,
                    1.0f, 0.0f);

                GUI.Label(new Rect(offsetX + 240, offsetY, 250, 30), "Smooth");
                _settings.Sample2Smoothness = GUI.VerticalSlider(new Rect(offsetX + 255, offsetY + 30, 10, 50),
                    _settings.Sample2Smoothness, 1.0f, 0.0f);

                offsetY += 90;
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
                out _settings.BlurSize, 0, 100)) StuffToBeDone = true;
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "High Pass Blur Size", _settings.OverlayBlurSize,
                out _settings.OverlayBlurSize, 10, 100)) StuffToBeDone = true;
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "High Pass Overlay", _settings.BlurOverlay,
                out _settings.BlurOverlay, -10.0f, 10.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Contrast", _settings.FinalContrast,
                out _settings.FinalContrast, -2.0f, 2.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Bias", _settings.FinalBias,
                out _settings.FinalBias, -0.5f, 0.5f);
            offsetY += 40;


            DrawGuiExtras(offsetX, offsetY);
        }

        private void OnGUI()
        {
            if (Hide) return;
            var rect = WindowRect;
            var guiScale = GuiScale;
            if (_settings.UseSample1)
                rect.height += 90;
//                guiScale -= 0.02f * guiScale;

            if (_settings.UseSample2)
                rect.height += 90;
//                guiScale -= 0.02f * guiScale;

            if (_settings.UseSample1 && _settings.UseSample2) guiScale -= 0.15f * guiScale;

            MainGui.MakeScaledWindow(rect, WindowId, DoMyWindow, "Smoothness From Diffuse", guiScale);
        }

        protected override void CleanupTextures()
        {
            RenderTexture.ReleaseTemporary(_blurMap);
            RenderTexture.ReleaseTemporary(_overlayBlurMap);
            RenderTexture.ReleaseTemporary(_tempMap);
        }

        public void InitializeTextures()
        {
            TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

            CleanupTextures();

            _diffuseMap = TextureManager.Instance.DiffuseMap;
            _diffuseMapOriginal = TextureManager.Instance.DiffuseMapOriginal;

            _metallicMap = TextureManager.Instance.MetallicMap;
            ThisMaterial.SetTexture(MetallicTex, _metallicMap ? _metallicMap : Texture2D.blackTexture);

            if (_diffuseMap)
            {
                ThisMaterial.SetTexture(MainTex, _diffuseMap);
                ImageSize.x = _diffuseMap.width;
                ImageSize.y = _diffuseMap.height;
            }
            else
            {
                ThisMaterial.SetTexture(MainTex, _diffuseMapOriginal);
                ImageSize.x = _diffuseMapOriginal.width;
                ImageSize.y = _diffuseMapOriginal.height;

                _settings.UseAdjustedDiffuse = false;
                _settings.UseOriginalDiffuse = true;
            }

            Logger.Log("Initializing Textures of size: " + ImageSize.x + "x" + ImageSize.y);

            _tempMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y);
            _blurMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y);
            _overlayBlurMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y);
        }

        public IEnumerator ProcessBlur()
        {
            while (!ProgramManager.Lock()) yield return null;

            MessagePanel.ShowMessage("Processing Blur for Smoothness Map");
            
            var blurKernel = BlurCompute.FindKernel("CSBlur");

            BlurCompute.SetVector(ImageSizeId, new Vector2(ImageSize.x, ImageSize.y));
            BlurCompute.SetFloat("_BlurContrast", 1.0f);
            BlurCompute.SetFloat("_BlurSpread", 1.0f);

            // Blur the image for selection
            BlurCompute.SetInt("_BlurSamples", _settings.BlurSize);
            BlurCompute.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
            var diffuse = _settings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal;
            
            var groupsX = (int) Mathf.Ceil(ImageSize.x / 8f);
            var groupsY = (int) Mathf.Ceil(ImageSize.y / 8f);
            _tempMap = TextureManager.Instance.GetTempRenderTexture(diffuse.width, diffuse.height);
            if (_settings.BlurSize == 0)
            {
                Graphics.Blit(diffuse, _tempMap);
            }
            else
            {
                BlurCompute.SetTexture(blurKernel, "ImageInput", diffuse);
                BlurCompute.SetTexture(blurKernel, "Result", _tempMap);
                BlurCompute.Dispatch(blurKernel, groupsX, groupsY, 1);
            }

            BlurCompute.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
            if (_settings.BlurSize == 0)
            {
                Graphics.Blit(_tempMap, _blurMap);
            }
            else
            {
                BlurCompute.SetTexture(blurKernel, "ImageInput", _tempMap);
                BlurCompute.SetTexture(blurKernel, "Result", _blurMap);
                BlurCompute.Dispatch(blurKernel, groupsX, groupsY, 1);
            }

            ThisMaterial.SetTexture(BlurTex, _blurMap);
            var source = _settings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal;

            BlurCompute.SetInt("_BlurSamples", _settings.OverlayBlurSize);
            BlurImage(1.0f, source, _overlayBlurMap);

            ThisMaterial.SetTexture(OverlayBlurTex, _overlayBlurMap);

            IsReadyToProcess = true;

            MessagePanel.HideMessage();

            yield return new WaitForSeconds(0.1f);

            ProgramManager.Unlock();
        }
    }
}