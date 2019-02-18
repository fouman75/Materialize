#region

using System.Collections;
using General;
using Settings;
using UnityEngine;

#endregion

namespace Gui
{
    public class EditDiffuseGui : MonoBehaviour
    {
        private static readonly int Slider = Shader.PropertyToID("_Slider");
        private static readonly int BlurContrast = Shader.PropertyToID("_BlurContrast");
        private static readonly int LightMaskPow = Shader.PropertyToID("_LightMaskPow");
        private static readonly int LightPow = Shader.PropertyToID("_LightPow");
        private static readonly int DarkMaskPow = Shader.PropertyToID("_DarkMaskPow");
        private static readonly int DarkPow = Shader.PropertyToID("_DarkPow");
        private static readonly int HotSpot = Shader.PropertyToID("_HotSpot");
        private static readonly int DarkSpot = Shader.PropertyToID("_DarkSpot");
        private static readonly int FinalContrast = Shader.PropertyToID("_FinalContrast");
        private static readonly int FinalBias = Shader.PropertyToID("_FinalBias");
        private static readonly int ColorLerp = Shader.PropertyToID("_ColorLerp");
        private static readonly int Saturation = Shader.PropertyToID("_Saturation");
        private static readonly int ImageSize = Shader.PropertyToID("_ImageSize");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int BlurTex = Shader.PropertyToID("_BlurTex");
        private static readonly int AvgTex = Shader.PropertyToID("_AvgTex");
        private static readonly int BlurSpread = Shader.PropertyToID("_BlurSpread");
        private static readonly int BlurSamples = Shader.PropertyToID("_BlurSamples");
        private static readonly int BlurDirection = Shader.PropertyToID("_BlurDirection");
        private readonly RenderTexture _avgTempMap;
        private RenderTexture _avgMap;
        private Material _blitMaterial;
        private RenderTexture _blurMap;

        private Texture2D _diffuseMap;
        private Texture2D _diffuseMapOriginal;
        private bool _doStuff;

        private EditDiffuseSettings _eds;

        private int _imageSizeX;
        private int _imageSizeY;
        private bool _newTexture;
        private bool _settingsInitialized;

        private float _slider = 0.5f;

        private RenderTexture _tempMap;

        private Rect _windowRect = new Rect(30, 300, 300, 450);

        public GameObject TestObject;

        public Material ThisMaterial;
        private Material _material;

        public EditDiffuseGui(RenderTexture avgTempMap)
        {
            _avgTempMap = avgTempMap;
        }

        public void GetValues(ProjectObject projectObject)
        {
            InitializeSettings();
            projectObject.EditDiffuseSettings = _eds;
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

            _doStuff = true;
        }

        private void InitializeSettings()
        {
            if (_settingsInitialized) return;
            Debug.Log("Initializing Edit Diffuse Settings");
            _eds = new EditDiffuseSettings();
            _settingsInitialized = true;
        }

        private void OnDisable()
        {
            _blitMaterial = new Material(Shader.Find("Hidden/Blit_Shader"));
        }

        // Use this for initialization
        private void Start()
        {
            _material = new Material(ThisMaterial);
            TestObject.GetComponent<Renderer>().material = _material;

            _blitMaterial = new Material(Shader.Find("Hidden/Blit_Shader"));

            InitializeSettings();
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
            if (_newTexture)
            {
                InitializeTextures();
                _newTexture = false;
            }

            if (_doStuff)
            {
                StartCoroutine(ProcessBlur());
                _doStuff = false;
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
            const int offsetX = 10;
            var offsetY = 30;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Diffuse Reveal Slider");
            _slider = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 20, 280, 10), _slider, 0.0f, 1.0f);
            offsetY += 50;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Average Color Blur Size", _eds.AvgColorBlurSize,
                out _eds.AvgColorBlurSize, 5, 100))
                _doStuff = true;
            offsetY += 50;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Overlay Blur Size", _eds.BlurSize,
                out _eds.BlurSize, 5, 100)) _doStuff = true;
            offsetY += 30;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Overlay Blur Contrast", _eds.BlurContrast,
                out _eds.BlurContrast, -1.0f, 1.0f);
            offsetY += 50;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Light Mask Power", _eds.LightMaskPow,
                out _eds.LightMaskPow, 0.0f, 1.0f);
            offsetY += 30;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Remove Light", _eds.LightPow,
                out _eds.LightPow, 0.0f, 1.0f);
            offsetY += 50;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Shadow Mask Power", _eds.DarkMaskPow,
                out _eds.DarkMaskPow, 0.0f, 1.0f);
            offsetY += 30;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Remove Shadow", _eds.DarkPow,
                out _eds.DarkPow, 0.0f, 1.0f);
            offsetY += 50;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Hot Spot Removal", _eds.HotSpot,
                out _eds.HotSpot, 0.0f, 1.0f);
            offsetY += 30;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Dark Spot Removal", _eds.DarkSpot,
                out _eds.DarkSpot, 0.0f, 1.0f);
            offsetY += 50;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Contrast", _eds.FinalContrast,
                out _eds.FinalContrast, -2.0f, 2.0f);
            offsetY += 30;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Bias", _eds.FinalBias,
                out _eds.FinalBias, -0.5f, 0.5f);
            offsetY += 50;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Keep Original Color", _eds.ColorLerp,
                out _eds.ColorLerp, 0.0f, 1.0f);
            offsetY += 30;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Saturation", _eds.Saturation,
                out _eds.Saturation, 0.0f, 1.0f);
            offsetY += 50;

            if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Set as Diffuse"))
                ProcessDiffuse();

            GUI.DragWindow();
        }

        private void OnGUI()
        {
            _windowRect.width = 300;
            _windowRect.height = 650;

            _windowRect = GUI.Window(12, _windowRect, DoMyWindow, "Edit Diffuse");
        }

        public void Close()
        {
            CleanupTextures();
            gameObject.SetActive(false);
        }

        private void CleanupTextures()
        {
            RenderTexture.ReleaseTemporary(_blurMap);
            RenderTexture.ReleaseTemporary(_tempMap);
            RenderTexture.ReleaseTemporary(_avgMap);
            RenderTexture.ReleaseTemporary(_avgTempMap);
        }

        private void InitializeTextures()
        {
            TestObject.GetComponent<Renderer>().sharedMaterial = _material;

            CleanupTextures();

            _diffuseMapOriginal = TextureManager.Instance.DiffuseMapOriginal;

            _material.SetTexture(MainTex, _diffuseMapOriginal);

            _imageSizeX = _diffuseMapOriginal.width;
            _imageSizeY = _diffuseMapOriginal.height;

            Debug.Log("Initializing Textures of size: " + _imageSizeX + "x" + _imageSizeY);

            _blurMap = TextureManager.Instance.GetTempRenderTexture(_imageSizeX, _imageSizeY);
            _avgMap = TextureManager.Instance.GetTempRenderTexture(_imageSizeX, _imageSizeY);
        }

        private void ProcessDiffuse()
        {
            Debug.Log("Processing Diffuse");

            _blitMaterial.SetVector(ImageSize, new Vector4(_imageSizeX, _imageSizeY, 0, 0));

            _blitMaterial.SetTexture(MainTex, _diffuseMapOriginal);

            _blitMaterial.SetTexture(BlurTex, _blurMap);
            _blitMaterial.SetFloat(BlurContrast, _eds.BlurContrast);

            _blitMaterial.SetTexture(AvgTex, _avgMap);

            _blitMaterial.SetFloat(LightMaskPow, _eds.LightMaskPow);
            _blitMaterial.SetFloat(LightPow, _eds.LightPow);

            _blitMaterial.SetFloat(DarkMaskPow, _eds.DarkMaskPow);
            _blitMaterial.SetFloat(DarkPow, _eds.DarkPow);

            _blitMaterial.SetFloat(HotSpot, _eds.HotSpot);
            _blitMaterial.SetFloat(DarkSpot, _eds.DarkSpot);

            _blitMaterial.SetFloat(FinalContrast, _eds.FinalContrast);

            _blitMaterial.SetFloat(FinalBias, _eds.FinalBias);

            _blitMaterial.SetFloat(ColorLerp, _eds.ColorLerp);

            _blitMaterial.SetFloat(Saturation, _eds.Saturation);

            RenderTexture.ReleaseTemporary(_tempMap);
            _tempMap = TextureManager.Instance.GetTempRenderTexture(_imageSizeX, _imageSizeY);

            Graphics.Blit(_diffuseMapOriginal, _tempMap, _blitMaterial, 11);

            TextureManager.Instance.GetTextureFromRender(_tempMap, ProgramEnums.MapType.Diffuse);

            RenderTexture.ReleaseTemporary(_tempMap);
        }

        private IEnumerator ProcessBlur()
        {
            Debug.Log("Processing Blur");

            RenderTexture.ReleaseTemporary(_tempMap);
            _tempMap = TextureManager.Instance.GetTempRenderTexture(_imageSizeX, _imageSizeY);

            _blitMaterial.SetVector(ImageSize, new Vector4(_imageSizeX, _imageSizeY, 0, 0));
            _blitMaterial.SetFloat(BlurContrast, 1.0f);
            _blitMaterial.SetFloat(BlurSpread, 1.0f);

            // Blur the image 1
            _blitMaterial.SetInt(BlurSamples, _eds.BlurSize);
            _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            Graphics.Blit(_diffuseMapOriginal, _tempMap, _blitMaterial, 1);
            _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            Graphics.Blit(_tempMap, _blurMap, _blitMaterial, 1);
            _material.SetTexture(BlurTex, _blurMap);


            _blitMaterial.SetTexture(MainTex, _diffuseMapOriginal);
            _blitMaterial.SetInt(BlurSamples, _eds.AvgColorBlurSize);
            _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            Graphics.Blit(_diffuseMapOriginal, _tempMap, _blitMaterial, 1);
            _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            Graphics.Blit(_tempMap, _avgMap, _blitMaterial, 1);

            _blitMaterial.SetFloat(BlurSpread, _eds.AvgColorBlurSize / 5.0f);
            _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            Graphics.Blit(_avgMap, _tempMap, _blitMaterial, 1);
            _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            Graphics.Blit(_tempMap, _avgMap, _blitMaterial, 1);

            _material.SetTexture(AvgTex, _avgMap);

            RenderTexture.ReleaseTemporary(_tempMap);
            RenderTexture.ReleaseTemporary(_avgTempMap);

            yield return new WaitForSeconds(0.01f);
        }
    }
}