#region

using System.Collections;
using General;
using Settings;
using UnityEngine;
using Logger = General.Logger;

#endregion

namespace Gui
{
    public class MetallicGui : TexturePanelGui
    {
        private RenderTexture _blurMap;
        private Camera _camera;

        private Texture2D _diffuseMap;
        private Texture2D _diffuseMapOriginal;

        private bool _lastUseAdjustedDiffuse;
        private Texture2D _metalColorMap;
        private Texture2D _metallicMap;

        private MetallicSettings _metallicSettings;
        private bool _mouseButtonDown;
        private RenderTexture _overlayBlurMap;
        private bool _selectingColor;

        private bool _settingsInitialized;

        private float _slider = 0.5f;

        private RenderTexture _tempMap;
        private int _windowId;

        private Rect _windowRect;
        public ComputeShader BlurCompute;

        public ComputeShader MetallicCompute;

        protected override IEnumerator Process()
        {
            MessagePanel.ShowMessage("Processing Metallic Map");
            var metallicKernel = MetallicCompute.FindKernel("CSMetallic");

            MetallicCompute.SetVector(ImageSizeId, new Vector2(ImageSize.x, ImageSize.y));

            MetallicCompute.SetVector("_MetalColor", _metallicSettings.MetalColor);
            MetallicCompute.SetVector("_SampleUV", _metallicSettings.SampleUv);
            MetallicCompute.SetFloat("_HueWeight", _metallicSettings.HueWeight);
            MetallicCompute.SetFloat("_SatWeight", _metallicSettings.SatWeight);
            MetallicCompute.SetFloat("_LumWeight", _metallicSettings.LumWeight);

            MetallicCompute.SetFloat("_MaskLow", _metallicSettings.MaskLow);
            MetallicCompute.SetFloat("_MaskHigh", _metallicSettings.MaskHigh);

            MetallicCompute.SetFloat("_BlurOverlay", _metallicSettings.BlurOverlay);

            MetallicCompute.SetFloat("_FinalContrast", _metallicSettings.FinalContrast);

            MetallicCompute.SetFloat("_FinalBias", _metallicSettings.FinalBias);

            MetallicCompute.SetTexture(metallicKernel, "_BlurTex", _blurMap);

            MetallicCompute.SetTexture(metallicKernel, "_OverlayBlurTex", _overlayBlurMap);

            RenderTexture.ReleaseTemporary(_tempMap);
            _tempMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y);
            var source = _metallicSettings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal;

            MetallicCompute.SetTexture(metallicKernel, "ImageInput", source);
            MetallicCompute.SetTexture(metallicKernel, "Result", _tempMap);
            var groupsX = (int) Mathf.Ceil(ImageSize.x / 8f);
            var groupsY = (int) Mathf.Ceil(ImageSize.y / 8f);
            MetallicCompute.Dispatch(metallicKernel, groupsX, groupsY, 1);

            TextureManager.Instance.GetTextureFromRender(_tempMap, ProgramEnums.MapType.Metallic);

            RenderTexture.ReleaseTemporary(_tempMap);

            yield break;
        }

        private void Awake()
        {
            _windowRect = new Rect(10.0f, 265.0f, 300f, 460f);
        }

        public void GetValues(ProjectObject projectObject)
        {
            InitializeSettings();
            projectObject.MetallicSettings = _metallicSettings;
        }

        public void SetValues(ProjectObject projectObject)
        {
            InitializeSettings();
            if (projectObject.MetallicSettings != null)
            {
                _metallicSettings = projectObject.MetallicSettings;
            }
            else
            {
                _settingsInitialized = false;
                InitializeSettings();
            }

            _metalColorMap.SetPixel(1, 1, _metallicSettings.MetalColor);
            _metalColorMap.Apply(false);

            StuffToBeDone = true;
        }

        private void InitializeSettings()
        {
            if (_settingsInitialized) return;
            Logger.Log("Initializing Metallic Settings");
            _metallicSettings = new MetallicSettings();

            _metalColorMap = TextureManager.Instance.GetStandardTexture(1, 1);
            _metalColorMap.SetPixel(1, 1, _metallicSettings.MetalColor);
            _metalColorMap.Apply(false);

            _settingsInitialized = true;
        }

        // Use this for initialization
        private void Start()
        {
            MessagePanel.ShowMessage("Initializing Metallic GUI");
            _camera = Camera.main;
            TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

            InitializeSettings();
            _windowId = ProgramManager.Instance.GetWindowId;
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

                _metallicSettings.SampleUv = pixelUv;

                _metallicSettings.MetalColor = _metallicSettings.UseAdjustedDiffuse
                    ? _diffuseMap.GetPixelBilinear(pixelUv.x, pixelUv.y)
                    : _diffuseMapOriginal.GetPixelBilinear(pixelUv.x, pixelUv.y);

                _metalColorMap.SetPixel(1, 1, _metallicSettings.MetalColor);
                _metalColorMap.Apply(false);
            }

            if (!Input.GetMouseButtonUp(0) || !_mouseButtonDown) return;

            _mouseButtonDown = false;
            _selectingColor = false;
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

            if (_metallicSettings.UseAdjustedDiffuse != _lastUseAdjustedDiffuse)
            {
                _lastUseAdjustedDiffuse = _metallicSettings.UseAdjustedDiffuse;
                StuffToBeDone = true;
            }

            if (StuffToBeDone)
            {
                StartCoroutine(ProcessBlur());
                StuffToBeDone = false;
            }

            //thisMaterial.SetFloat ("_BlurWeight", BlurWeight);

            ThisMaterial.SetVector(MetalColor, _metallicSettings.MetalColor);
            ThisMaterial.SetVector(SampleUv,
                new Vector4(_metallicSettings.SampleUv.x, _metallicSettings.SampleUv.y, 0, 0));

            ThisMaterial.SetFloat(HueWeight, _metallicSettings.HueWeight);
            ThisMaterial.SetFloat(SatWeight, _metallicSettings.SatWeight);
            ThisMaterial.SetFloat(LumWeight, _metallicSettings.LumWeight);
            ThisMaterial.SetFloat(MaskLow, _metallicSettings.MaskLow);
            ThisMaterial.SetFloat(MaskHigh, _metallicSettings.MaskHigh);

            ThisMaterial.SetFloat(Slider, _slider);
            ThisMaterial.SetFloat(BlurOverlay, _metallicSettings.BlurOverlay);
            ThisMaterial.SetFloat(FinalContrast, _metallicSettings.FinalContrast);
            ThisMaterial.SetFloat(FinalBias, _metallicSettings.FinalBias);

            ThisMaterial.SetTexture(MainTex, _metallicSettings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal);
        }

        private void DoMyWindow(int windowId)
        {
            const int offsetX = 10;
            var offsetY = 30;

            GUI.enabled = _diffuseMap != null;
            if (GUI.Toggle(new Rect(offsetX, offsetY, 140, 30), _metallicSettings.UseAdjustedDiffuse,
                " Use Edited Diffuse"))
            {
                _metallicSettings.UseAdjustedDiffuse = true;
                _metallicSettings.UseOriginalDiffuse = false;
            }

            GUI.enabled = true;
            if (GUI.Toggle(new Rect(offsetX + 150, offsetY, 140, 30), _metallicSettings.UseOriginalDiffuse,
                " Use Original Diffuse"))
            {
                _metallicSettings.UseAdjustedDiffuse = false;
                _metallicSettings.UseOriginalDiffuse = true;
            }

            offsetY += 30;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Metallic Reveal Slider");
            _slider = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 20, 280, 10), _slider, 0.0f, 1.0f);

            offsetY += 40;

            if (GUI.Button(new Rect(offsetX, offsetY + 10, 80, 30), "Pick Color")) _selectingColor = true;

            GUI.DrawTexture(new Rect(offsetX, offsetY + 50, 80, 80), _metalColorMap);

            GUI.Label(new Rect(offsetX + 90, offsetY, 250, 30), "Hue");
            _metallicSettings.HueWeight = GUI.VerticalSlider(new Rect(offsetX + 95, offsetY + 30, 10, 100),
                _metallicSettings.HueWeight, 1.0f, 0.0f);

            GUI.Label(new Rect(offsetX + 130, offsetY, 250, 30), "Sat");
            _metallicSettings.SatWeight = GUI.VerticalSlider(new Rect(offsetX + 135, offsetY + 30, 10, 100),
                _metallicSettings.SatWeight, 1.0f, 0.0f);

            GUI.Label(new Rect(offsetX + 170, offsetY, 250, 30), "Lum");
            _metallicSettings.LumWeight = GUI.VerticalSlider(new Rect(offsetX + 175, offsetY + 30, 10, 100),
                _metallicSettings.LumWeight, 1.0f, 0.0f);

            GUI.Label(new Rect(offsetX + 220, offsetY, 250, 30), "Low");
            _metallicSettings.MaskLow = GUI.VerticalSlider(new Rect(offsetX + 225, offsetY + 30, 10, 100),
                _metallicSettings.MaskLow, 1.0f, 0.0f);

            GUI.Label(new Rect(offsetX + 250, offsetY, 250, 30), "High");
            _metallicSettings.MaskHigh = GUI.VerticalSlider(new Rect(offsetX + 255, offsetY + 30, 10, 100),
                _metallicSettings.MaskHigh, 1.0f, 0.0f);

            offsetY += 150;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Blur Size", _metallicSettings.BlurSize,
                out _metallicSettings.BlurSize, 0, 100)) StuffToBeDone = true;
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Overlay Blur Size",
                _metallicSettings.OverlayBlurSize, out _metallicSettings.OverlayBlurSize,
                10, 100)) StuffToBeDone = true;
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "High Pass Overlay", _metallicSettings.BlurOverlay,
                out _metallicSettings.BlurOverlay, -10.0f, 10.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Contrast", _metallicSettings.FinalContrast,
                out _metallicSettings.FinalContrast, -2.0f, 2.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Bias", _metallicSettings.FinalBias,
                out _metallicSettings.FinalBias, -0.5f, 0.5f);

            GUI.DragWindow();
        }

        private void OnGUI()
        {
            if (Hide) return;
            MainGui.MakeScaledWindow(_windowRect, _windowId, DoMyWindow, "Metallic From Diffuse");
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

                _metallicSettings.UseAdjustedDiffuse = false;
                _metallicSettings.UseOriginalDiffuse = true;
            }

            Logger.Log("Initializing Textures of size: " + ImageSize.x + "x" + ImageSize.y);

            _tempMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y);
            _blurMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y);
            _overlayBlurMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y);
        }

        public IEnumerator ProcessBlur()
        {
            while (!ProgramManager.Lock()) yield return null;


            MessagePanel.ShowMessage("Processing Blur for Metallic Map");

            var blurKernel = BlurCompute.FindKernel("CSBlur");

            BlurCompute.SetVector(ImageSizeId, new Vector2(ImageSize.x, ImageSize.y));
            BlurCompute.SetFloat("_BlurContrast", 1.0f);
            BlurCompute.SetFloat("_BlurSpread", 1.0f);

            // Blur the image 1
            BlurCompute.SetInt("_BlurSamples", _metallicSettings.BlurSize);
            BlurCompute.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
            var diffuse = _metallicSettings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal;

            var groupsX = (int) Mathf.Ceil(ImageSize.x / 8f);
            var groupsY = (int) Mathf.Ceil(ImageSize.y / 8f);

            if (_metallicSettings.BlurSize == 0)
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
            if (_metallicSettings.BlurSize == 0)
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

            // Blur the image for overlay
            BlurCompute.SetInt("_BlurSamples", _metallicSettings.OverlayBlurSize);
            BlurCompute.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
            var source = _metallicSettings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal;
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

            IsReadyToProcess = true;

            MessagePanel.HideMessage();

            ProgramManager.Unlock();
        }
    }
}