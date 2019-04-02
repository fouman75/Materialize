#region

using System.Collections;
using Materialize.General;
using Materialize.Settings;
using UnityEngine;
using Utility;
using Logger = Utility.Logger;

#endregion

namespace Materialize.Gui
{
    public class EditDiffuseGui : TexturePanelGui
    {
        private RenderTexture _avgMap;
        private RenderTexture _blurMap;

        private Texture2D _diffuseMap;
        private Texture2D _diffuseMapOriginal;

        private EditDiffuseSettings _eds;

        private Material _material;
        private bool _settingsInitialized;

        private float _slider = 0.5f;

        private RenderTexture _tempMap;
        public ComputeShader EditCompute;

        protected override IEnumerator Process()
        {
            Logger.Log("Processing Diffuse");
            var kernel = EditCompute.FindKernel("CSEditDiffuse");

            EditCompute.SetVector(ImageSizeId, (Vector2) ImageSize);

            EditCompute.SetTexture(kernel, BlurTex, _blurMap);
            EditCompute.SetFloat(BlurContrast, _eds.BlurContrast);

            EditCompute.SetTexture(kernel, AvgTex, _avgMap);

            EditCompute.SetFloat(LightMaskPow, _eds.LightMaskPow);
            EditCompute.SetFloat(LightPow, _eds.LightPow);

            EditCompute.SetFloat(DarkMaskPow, _eds.DarkMaskPow);
            EditCompute.SetFloat(DarkPow, _eds.DarkPow);

            EditCompute.SetFloat(HotSpot, _eds.HotSpot);
            EditCompute.SetFloat(DarkSpot, _eds.DarkSpot);

            EditCompute.SetFloat(FinalContrast, _eds.FinalContrast);

            EditCompute.SetFloat(FinalBias, _eds.FinalBias);

            EditCompute.SetFloat(ColorLerp, _eds.ColorLerp);

            EditCompute.SetFloat(Saturation, _eds.Saturation);

            RenderTexture.ReleaseTemporary(_tempMap);
            _tempMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y);

            RunKernel(EditCompute, kernel, _diffuseMapOriginal, _tempMap);

            TextureManager.Instance.GetTextureFromRender(_tempMap, ProgramEnums.MapType.Diffuse);

            RenderTexture.ReleaseTemporary(_tempMap);

            yield break;
        }

        protected override void ResetSettings()
        {
            _eds.Reset();
            StuffToBeDone = true;
        }

        protected override TexturePanelSettings GetSettings()
        {
            return _eds;
        }

        protected override void SetSettings(TexturePanelSettings settings)
        {
            _eds = settings as EditDiffuseSettings;
        }

        public void GetValues(ProjectObject projectObject)
        {
            InitializeSettings();
            projectObject.EditDiffuseSettings = _eds;
        }

        private void Awake()
        {
            WindowRect = new Rect(10.0f, 265.0f, 300f, 585f);
            GuiScale -= 0.1f;
            PostAwake();
        }

        public void SetValues(ProjectObject projectObject)
        {
            InitializeSettings();
            if (projectObject.EditDiffuseSettings != null)
            {
                _eds = projectObject.EditDiffuseSettings;
            }
            else
            {
                _settingsInitialized = false;
                InitializeSettings();
            }

            StuffToBeDone = true;
        }

        private void InitializeSettings()
        {
            if (_settingsInitialized) return;
            Logger.Log("Initializing Edit Diffuse Settings");
            _eds = new EditDiffuseSettings();
            _settingsInitialized = true;
        }

        // Use this for initialization
        private void Start()
        {
            _material = new Material(ThisMaterial);
            TestObject.GetComponent<Renderer>().material = _material;

            InitializeSettings();
        }

        // Update is called once per frame
        private void Update()
        {
            if (ProgramManager.IsLocked) return;

            if (IsNewTexture)
            {
                InitializeTextures();
                IsNewTexture = false;
            }

            if (StuffToBeDone)
            {
                StartCoroutine(ProcessBlur());
                StuffToBeDone = false;
            }


            _material.SetFloat(Slider, _slider);

            _material.SetFloat(BlurContrast, _eds.BlurContrast);

            _material.SetFloat(LightMaskPow, _eds.LightMaskPow);
            _material.SetFloat(LightPow, _eds.LightPow);

            _material.SetFloat(DarkMaskPow, _eds.DarkMaskPow);
            _material.SetFloat(DarkPow, _eds.DarkPow);

            _material.SetFloat(HotSpot, _eds.HotSpot);
            _material.SetFloat(DarkSpot, _eds.DarkSpot);

            _material.SetFloat(FinalContrast, _eds.FinalContrast);
            _material.SetFloat(FinalBias, _eds.FinalBias);

            _material.SetFloat(ColorLerp, _eds.ColorLerp);

            _material.SetFloat(Saturation, _eds.Saturation);
        }

        private void DoMyWindow(int windowId)
        {
            var offsetX = 10;
            var offsetY = 20;

            GUI.enabled = true;
            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Diffuse Reveal Slider");
            _slider = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 20, 280, 10), _slider, 0.0f, 1.0f);
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Average Color Blur Size", _eds.AvgColorBlurSize,
                out _eds.AvgColorBlurSize, 5, 100))
                StuffToBeDone = true;
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Overlay Blur Size", _eds.BlurSize,
                out _eds.BlurSize, 5, 100)) StuffToBeDone = true;
            offsetY += 40;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Overlay Blur Contrast", _eds.BlurContrast,
                out _eds.BlurContrast, -1.0f, 1.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Light Mask Power", _eds.LightMaskPow,
                out _eds.LightMaskPow, 0.0f, 1.0f);
            offsetY += 40;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Remove Light", _eds.LightPow,
                out _eds.LightPow, 0.0f, 1.0f);
            offsetY += 40;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Shadow Mask Power", _eds.DarkMaskPow,
                out _eds.DarkMaskPow, 0.0f, 1.0f);
            offsetY += 40;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Remove Shadow", _eds.DarkPow,
                out _eds.DarkPow, 0.0f, 1.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Hot Spot Removal", _eds.HotSpot,
                out _eds.HotSpot, 0.0f, 1.0f);
            offsetY += 40;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Dark Spot Removal", _eds.DarkSpot,
                out _eds.DarkSpot, 0.0f, 1.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Contrast", _eds.FinalContrast,
                out _eds.FinalContrast, -2.0f, 2.0f);
            offsetY += 40;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Bias", _eds.FinalBias,
                out _eds.FinalBias, -0.5f, 0.5f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Keep Original Color", _eds.ColorLerp,
                out _eds.ColorLerp, 0.0f, 1.0f);
            offsetY += 40;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Saturation", _eds.Saturation,
                out _eds.Saturation, 0.0f, 1.0f);
            offsetY += 40;

            DrawGuiExtras(offsetX, offsetY);
        }

        private void OnGUI()
        {
            if (Hide) return;
            MainGui.MakeScaledWindow(WindowRect, WindowId, DoMyWindow, "Edit Diffuse", GuiScale);
        }

        protected override void CleanupTextures()
        {
            RenderTexture.ReleaseTemporary(_blurMap);
            RenderTexture.ReleaseTemporary(_tempMap);
            RenderTexture.ReleaseTemporary(_avgMap);
        }

        private void InitializeTextures()
        {
            TestObject.GetComponent<Renderer>().sharedMaterial = _material;

            CleanupTextures();

            _diffuseMapOriginal = TextureManager.Instance.DiffuseMapOriginal;

            _material.SetTexture(MainTex, _diffuseMapOriginal);

            ImageSize.x = _diffuseMapOriginal.width;
            ImageSize.y = _diffuseMapOriginal.height;

            Logger.Log("Initializing Textures of size: " + ImageSize.x + "x" + ImageSize.y);

            _blurMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y);
            _avgMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y);
        }

        private IEnumerator ProcessBlur()
        {
            while (!ProgramManager.Lock()) yield return null;

            MessagePanel.ShowMessage("Processing Blur for Diffuse");

            BlurCompute.SetFloat(BlurContrast, 1.0f);
            BlurCompute.SetFloat(BlurSpread, 1.0f);

            BlurCompute.SetInt(BlurSamples, _eds.BlurSize);
            BlurImage(1.0f, _diffuseMapOriginal, _blurMap);

            BlurCompute.SetInt(BlurSamples, _eds.AvgColorBlurSize);
            BlurImage(1.0f, _diffuseMapOriginal, _avgMap);
            BlurImage(_eds.AvgColorBlurSize / 5.0f, _avgMap, _avgMap);

            _material.SetTexture(AvgTex, _avgMap);

            IsReadyToProcess = true;

            MessagePanel.HideMessage();

            yield return new WaitForSeconds(0.1f);

            ProgramManager.Unlock();
        }
    }
}