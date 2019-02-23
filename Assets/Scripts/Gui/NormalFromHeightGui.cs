#region

using System.Collections;
using General;
using Settings;
using UnityEngine;

#endregion


namespace Gui
{
    public class NormalFromHeightGui : MonoBehaviour, IProcessor
    {
        public bool Active
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }

        private static readonly int Blur0Weight = Shader.PropertyToID("_Blur0Weight");
        private static readonly int Blur1Weight = Shader.PropertyToID("_Blur1Weight");
        private static readonly int Blur2Weight = Shader.PropertyToID("_Blur2Weight");
        private static readonly int Blur3Weight = Shader.PropertyToID("_Blur3Weight");
        private static readonly int Blur4Weight = Shader.PropertyToID("_Blur4Weight");
        private static readonly int Blur5Weight = Shader.PropertyToID("_Blur5Weight");
        private static readonly int Blur6Weight = Shader.PropertyToID("_Blur6Weight");
        private static readonly int Slider = Shader.PropertyToID("_Slider");
        private static readonly int Angularity = Shader.PropertyToID("_Angularity");
        private static readonly int AngularIntensity = Shader.PropertyToID("_AngularIntensity");
        private static readonly int FinalContrast = Shader.PropertyToID("_FinalContrast");
        private static readonly int LightDir = Shader.PropertyToID("_LightDir");
        private static readonly int HeightTex = Shader.PropertyToID("_HeightTex");
        private Material _blitMaterial;
        private RenderTexture _blurMap0;
        private RenderTexture _blurMap1;
        private RenderTexture _blurMap2;
        private RenderTexture _blurMap3;
        private RenderTexture _blurMap4;
        private RenderTexture _blurMap5;
        private RenderTexture _blurMap6;
        private bool _doStuff;
        private int _imageSizeX = 1024;
        private int _imageSizeY = 1024;
        private bool _newTexture;

        private NormalFromHeightSettings _settings;
        private bool _settingsInitialized;

        private float _slider = 0.5f;

        private RenderTexture _tempBlurMap;

        private Rect _windowRect;
        private int _windowId;

        [HideInInspector] public bool Busy;

        [HideInInspector] public RenderTexture HeightBlurMap;

        public Light MainLight;

        public GameObject TestObject;

        public Material ThisMaterial;
        private Coroutine _processingCoroutine;
        private Renderer _testMaterialRenderer;
        private static readonly int ImageSize = Shader.PropertyToID("_ImageSize");
        private static readonly int HeightBlurTex = Shader.PropertyToID("_HeightBlurTex");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int BlurTex0 = Shader.PropertyToID("_BlurTex0");
        private static readonly int BlurTex1 = Shader.PropertyToID("_BlurTex1");
        private static readonly int BlurTex2 = Shader.PropertyToID("_BlurTex2");
        private static readonly int BlurTex3 = Shader.PropertyToID("_BlurTex3");
        private static readonly int BlurTex4 = Shader.PropertyToID("_BlurTex4");
        private static readonly int BlurTex5 = Shader.PropertyToID("_BlurTex5");
        private static readonly int BlurTex6 = Shader.PropertyToID("_BlurTex6");
        private static readonly int BlurSpread = Shader.PropertyToID("_BlurSpread");
        private static readonly int BlurSamples = Shader.PropertyToID("_BlurSamples");
        private static readonly int BlurDirection = Shader.PropertyToID("_BlurDirection");
        private static readonly int BlurContrast = Shader.PropertyToID("_BlurContrast");
        private static readonly int Desaturate = Shader.PropertyToID("_Desaturate");
        private static readonly int LightTex = Shader.PropertyToID("_LightTex");
        private static readonly int LightBlurTex = Shader.PropertyToID("_LightBlurTex");
        private static readonly int LightRotation = Shader.PropertyToID("_LightRotation");
        private static readonly int ShapeRecognition = Shader.PropertyToID("_ShapeRecognition");
        private static readonly int ShapeBias = Shader.PropertyToID("_ShapeBias");
        private static readonly int DiffuseTex = Shader.PropertyToID("_DiffuseTex");
        private Material _previewMaterial;

        private void Awake()
        {
            _testMaterialRenderer = TestObject.GetComponent<Renderer>();
            _windowRect = new Rect(10.0f, 265.0f, 300f, 535f);
        }

        private void OnDisable()
        {
            CleanupTextures();
        }

        public void GetValues(ProjectObject projectObject)
        {
            InitializeSettings();
            projectObject.NormalFromHeightSettings = _settings;
        }

        public void SetValues(ProjectObject projectObject)
        {
            InitializeSettings();
            if (projectObject.NormalFromHeightSettings != null)
            {
                _settings = projectObject.NormalFromHeightSettings;
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
            Debug.Log("Initializing Normal From Height Settings");
            _settings = new NormalFromHeightSettings();
            _settingsInitialized = true;
        }

        private void Start()
        {
            _previewMaterial = new Material(ThisMaterial.shader);

            _blitMaterial = new Material(Shader.Find("Hidden/Blit_Shader"));

            InitializeSettings();
            InitializeTextures();

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

        private void SetWeightEqDefault()
        {
            _settings.Blur0Weight = 0.3f;
            _settings.Blur1Weight = 0.35f;
            _settings.Blur2Weight = 0.5f;
            _settings.Blur3Weight = 0.8f;
            _settings.Blur4Weight = 1.0f;
            _settings.Blur5Weight = 0.95f;
            _settings.Blur6Weight = 0.8f;
            _doStuff = true;
        }

        private void SetWeightEqSmooth()
        {
            _settings.Blur0Weight = 0.1f;
            _settings.Blur1Weight = 0.15f;
            _settings.Blur2Weight = 0.25f;
            _settings.Blur3Weight = 0.6f;
            _settings.Blur4Weight = 0.9f;
            _settings.Blur5Weight = 1.0f;
            _settings.Blur6Weight = 1.0f;
            _doStuff = true;
        }

        private void SetWeightEqCrisp()
        {
            _settings.Blur0Weight = 1.0f;
            _settings.Blur1Weight = 0.9f;
            _settings.Blur2Weight = 0.6f;
            _settings.Blur3Weight = 0.4f;
            _settings.Blur4Weight = 0.25f;
            _settings.Blur5Weight = 0.15f;
            _settings.Blur6Weight = 0.1f;
            _doStuff = true;
        }

        // ReSharper disable once IdentifierTypo
        private void SetWeightEqMids()
        {
            _settings.Blur0Weight = 0.15f;
            _settings.Blur1Weight = 0.5f;
            _settings.Blur2Weight = 0.85f;
            _settings.Blur3Weight = 1.0f;
            _settings.Blur4Weight = 0.85f;
            _settings.Blur5Weight = 0.5f;
            _settings.Blur6Weight = 0.15f;
            _doStuff = true;
        }

        private void Update()
        {
            InitializeTextures();

            if (_doStuff)
            {
                if (_processingCoroutine != null) StopCoroutine(_processingCoroutine);
                _processingCoroutine = StartCoroutine(ProcessHeight());
                _doStuff = false;
            }

            _previewMaterial.SetFloat(Blur0Weight, _settings.Blur0Weight);
            _previewMaterial.SetFloat(Blur1Weight, _settings.Blur1Weight);
            _previewMaterial.SetFloat(Blur2Weight, _settings.Blur2Weight);
            _previewMaterial.SetFloat(Blur3Weight, _settings.Blur3Weight);
            _previewMaterial.SetFloat(Blur4Weight, _settings.Blur4Weight);
            _previewMaterial.SetFloat(Blur5Weight, _settings.Blur5Weight);
            _previewMaterial.SetFloat(Blur6Weight, _settings.Blur6Weight);

            _previewMaterial.SetFloat(Slider, _slider);

            _previewMaterial.SetFloat(Angularity, _settings.Angularity);
            _previewMaterial.SetFloat(AngularIntensity, _settings.AngularIntensity);

            _previewMaterial.SetFloat(FinalContrast, _settings.FinalContrast);

            _previewMaterial.SetVector(LightDir, MainLight.transform.forward);
        }

        private void DoMyWindow(int windowId)
        {
            var offsetX = 10;
            var offsetY = 30;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Normal Reveal Slider");
            _slider = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 20, 280, 10), _slider, 0.0f, 1.0f);

            offsetY += 35;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Pre Contrast", _settings.Blur0Contrast,
                out _settings.Blur0Contrast, 0.0f, 50.0f)) _doStuff = true;
            offsetY += 45;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Frequency Equalizer");
            GUI.Label(new Rect(offsetX + 225, offsetY, 100, 30), "Presets");
            if (GUI.Button(new Rect(offsetX + 215, offsetY + 30, 60, 20), "Default")) SetWeightEqDefault();
            if (GUI.Button(new Rect(offsetX + 215, offsetY + 60, 60, 20), "Smooth")) SetWeightEqSmooth();
            if (GUI.Button(new Rect(offsetX + 215, offsetY + 90, 60, 20), "Crisp")) SetWeightEqCrisp();
            if (GUI.Button(new Rect(offsetX + 215, offsetY + 120, 60, 20), "Mids")) SetWeightEqMids();
            offsetY += 25;
            offsetX += 10;
            _settings.Blur0Weight =
                GUI.VerticalSlider(new Rect(offsetX + 180, offsetY, 10, 100), _settings.Blur0Weight, 1.0f, 0.0f);
            _settings.Blur1Weight =
                GUI.VerticalSlider(new Rect(offsetX + 150, offsetY, 10, 100), _settings.Blur1Weight, 1.0f, 0.0f);
            _settings.Blur2Weight =
                GUI.VerticalSlider(new Rect(offsetX + 120, offsetY, 10, 100), _settings.Blur2Weight, 1.0f, 0.0f);
            _settings.Blur3Weight =
                GUI.VerticalSlider(new Rect(offsetX + 90, offsetY, 10, 100), _settings.Blur3Weight, 1.0f, 0.0f);
            _settings.Blur4Weight =
                GUI.VerticalSlider(new Rect(offsetX + 60, offsetY, 10, 100), _settings.Blur4Weight, 1.0f, 0.0f);
            _settings.Blur5Weight =
                GUI.VerticalSlider(new Rect(offsetX + 30, offsetY, 10, 100), _settings.Blur5Weight, 1.0f, 0.0f);
            _settings.Blur6Weight =
                GUI.VerticalSlider(new Rect(offsetX + 0, offsetY, 10, 100), _settings.Blur6Weight, 1.0f, 0.0f);
            offsetX -= 10;
            offsetY += 115;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Angular Intensity", _settings.AngularIntensity,
                out _settings.AngularIntensity, 0.0f, 1.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Angularity Amount", _settings.Angularity,
                out _settings.Angularity, 0.0f, 1.0f);
            offsetY += 45;

            if (TextureManager.Instance.DiffuseMapOriginal)
            {
                GUI.enabled = true;
            }
            else
            {
                GUI.enabled = false;
                _settings.UseDiffuse = false;
            }

            var tempBool = _settings.UseDiffuse;
            _settings.UseDiffuse = GUI.Toggle(new Rect(offsetX, offsetY, 280, 30), _settings.UseDiffuse,
                " Shape from Diffuse (Unchecked from Height)");
            if (tempBool != _settings.UseDiffuse) _doStuff = true;
            offsetY += 35;

            GUI.enabled = true;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), " Shape Recognition, Rotation, Spread, Bias",
                _settings.ShapeRecognition,
                out _settings.ShapeRecognition, 0.0f, 1.0f)) _doStuff = true;
            offsetY += 25;
            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), _settings.LightRotation,
                out _settings.LightRotation, -3.14f, 3.14f)) _doStuff = true;
            offsetY += 25;
            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), _settings.SlopeBlur,
                out _settings.SlopeBlur, 5, 100)) _doStuff = true;
            offsetY += 25;
            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), _settings.ShapeBias,
                out _settings.ShapeBias, 0.0f, 1.0f)) _doStuff = true;
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Contrast", _settings.FinalContrast,
                out _settings.FinalContrast, 0.0f, 10.0f);

            GUI.DragWindow();
        }

        private void OnGUI()
        {
            MainGui.MakeScaledWindow(_windowRect, _windowId, DoMyWindow, "Normal From Height");
        }

        public void InitializeTextures()
        {
            if (!_newTexture) return;

            _testMaterialRenderer.material = _previewMaterial;

            CleanupTextures();

            if (!TextureManager.Instance.HdHeightMap)
                _previewMaterial.SetTexture(HeightTex, TextureManager.Instance.HeightMap);
            else
                _previewMaterial.SetTexture(HeightTex, TextureManager.Instance.HdHeightMap);

            _imageSizeX = TextureManager.Instance.HeightMap.width;
            _imageSizeY = TextureManager.Instance.HeightMap.height;

            Debug.Log("Initializing Textures of size: " + _imageSizeX + "x" + _imageSizeY);

            _tempBlurMap = RenderTexture.GetTemporary(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear);
            HeightBlurMap = RenderTexture.GetTemporary(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RHalf,
                RenderTextureReadWrite.Linear);
            _blurMap0 = RenderTexture.GetTemporary(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear);
            _blurMap1 = RenderTexture.GetTemporary(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear);
            _blurMap2 = RenderTexture.GetTemporary(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear);
            _blurMap3 = RenderTexture.GetTemporary(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear);
            _blurMap4 = RenderTexture.GetTemporary(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear);
            _blurMap5 = RenderTexture.GetTemporary(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear);
            _blurMap6 = RenderTexture.GetTemporary(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear);

            _newTexture = false;
        }

        public void Close()
        {
            CleanupTextures();
            gameObject.SetActive(false);
        }

        private void CleanupTextures()
        {
            RenderTexture.ReleaseTemporary(_tempBlurMap);
            RenderTexture.ReleaseTemporary(HeightBlurMap);
            RenderTexture.ReleaseTemporary(_blurMap0);
            RenderTexture.ReleaseTemporary(_blurMap1);
            RenderTexture.ReleaseTemporary(_blurMap2);
            RenderTexture.ReleaseTemporary(_blurMap3);
            RenderTexture.ReleaseTemporary(_blurMap4);
            RenderTexture.ReleaseTemporary(_blurMap5);
            RenderTexture.ReleaseTemporary(_blurMap6);
        }

        public IEnumerator Process()
        {
            Busy = true;

            Debug.Log("Processing Normal");

            _blitMaterial.SetVector(ImageSize, new Vector4(_imageSizeX, _imageSizeY, 0, 0));

            _blitMaterial.SetFloat(Blur0Weight, _settings.Blur0Weight);
            _blitMaterial.SetFloat(Blur1Weight, _settings.Blur1Weight);
            _blitMaterial.SetFloat(Blur2Weight, _settings.Blur2Weight);
            _blitMaterial.SetFloat(Blur3Weight, _settings.Blur3Weight);
            _blitMaterial.SetFloat(Blur4Weight, _settings.Blur4Weight);
            _blitMaterial.SetFloat(Blur5Weight, _settings.Blur5Weight);
            _blitMaterial.SetFloat(Blur6Weight, _settings.Blur6Weight);
            _blitMaterial.SetFloat(FinalContrast, _settings.FinalContrast);

            _blitMaterial.SetTexture(HeightBlurTex, HeightBlurMap);

            _blitMaterial.SetTexture(MainTex, _blurMap0);
            _blitMaterial.SetTexture(BlurTex0, _blurMap0);
            _blitMaterial.SetTexture(BlurTex1, _blurMap1);
            _blitMaterial.SetTexture(BlurTex2, _blurMap2);
            _blitMaterial.SetTexture(BlurTex3, _blurMap3);
            _blitMaterial.SetTexture(BlurTex4, _blurMap4);
            _blitMaterial.SetTexture(BlurTex5, _blurMap5);
            _blitMaterial.SetTexture(BlurTex6, _blurMap6);

            _blitMaterial.SetFloat(Angularity, _settings.Angularity);
            _blitMaterial.SetFloat(AngularIntensity, _settings.AngularIntensity);


            var tempNormalMap = TextureManager.Instance.GetTempRenderTexture(_imageSizeX, _imageSizeY);
            Graphics.Blit(_blurMap0, tempNormalMap, _blitMaterial, 4);

            TextureManager.Instance.GetTextureFromRender(tempNormalMap, ProgramEnums.MapType.Normal);

            RenderTexture.ReleaseTemporary(tempNormalMap);

            Busy = false;
            yield break;
        }

        public IEnumerator ProcessHeight()
        {
            Busy = true;

            Debug.Log("Processing Height");

            _blitMaterial.SetVector(ImageSize, new Vector4(_imageSizeX, _imageSizeY, 0, 0));

            // Blur the height map for normal slope
            _blitMaterial.SetFloat(BlurSpread, 1.0f);
            _blitMaterial.SetInt(BlurSamples, _settings.SlopeBlur);
            _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            _blitMaterial.SetFloat(BlurContrast, 1.0f);

            if (TextureManager.Instance.DiffuseMapOriginal && _settings.UseDiffuse)
            {
                _blitMaterial.SetInt(Desaturate, 1);
                Graphics.Blit(TextureManager.Instance.DiffuseMapOriginal, _tempBlurMap, _blitMaterial, 1);
                _blitMaterial.SetTexture(LightTex, TextureManager.Instance.DiffuseMapOriginal);
            }
            else
            {
                if (!TextureManager.Instance.HdHeightMap)
                {
                    _blitMaterial.SetInt(Desaturate, 0);
                    Graphics.Blit(TextureManager.Instance.HeightMap, _tempBlurMap, _blitMaterial, 1);
                    _blitMaterial.SetTexture(LightTex, TextureManager.Instance.HeightMap);
                }
                else
                {
                    _blitMaterial.SetInt(Desaturate, 0);
                    Graphics.Blit(TextureManager.Instance.HdHeightMap, _tempBlurMap, _blitMaterial, 1);
                    _blitMaterial.SetTexture(LightTex, TextureManager.Instance.HdHeightMap);
                }
            }

            _blitMaterial.SetInt(Desaturate, 0);

            _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            Graphics.Blit(_tempBlurMap, HeightBlurMap, _blitMaterial, 1);

            _blitMaterial.SetFloat(BlurSpread, 3.0f);
            _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            Graphics.Blit(HeightBlurMap, _tempBlurMap, _blitMaterial, 1);

            _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            Graphics.Blit(_tempBlurMap, HeightBlurMap, _blitMaterial, 1);

            _blitMaterial.SetTexture(LightBlurTex, HeightBlurMap);

            // Make normal from height
            _blitMaterial.SetFloat(LightRotation, _settings.LightRotation);
            _blitMaterial.SetFloat(ShapeRecognition, _settings.ShapeRecognition);
            _blitMaterial.SetFloat(ShapeBias, _settings.ShapeBias);
            _blitMaterial.SetTexture(DiffuseTex, TextureManager.Instance.DiffuseMapOriginal);

            _blitMaterial.SetFloat(BlurContrast, _settings.Blur0Contrast);

            if (!TextureManager.Instance.HdHeightMap)
                Graphics.Blit(TextureManager.Instance.HeightMap, _blurMap0, _blitMaterial, 3);
            else
                Graphics.Blit(TextureManager.Instance.HdHeightMap, _blurMap0, _blitMaterial, 3);

            var extraSpread = (_blurMap0.width + _blurMap0.height) * 0.5f / 1024.0f;
            var spread = 1.0f;

            _blitMaterial.SetFloat(BlurContrast, 1.0f);
            _blitMaterial.SetInt(Desaturate, 0);

            // Blur the image 1
            _blitMaterial.SetInt(BlurSamples, 4);
            _blitMaterial.SetFloat(BlurSpread, spread);
            _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            Graphics.Blit(_blurMap0, _tempBlurMap, _blitMaterial, 1);
            _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            Graphics.Blit(_tempBlurMap, _blurMap1, _blitMaterial, 1);

            spread += extraSpread;

            // Blur the image 2
            _blitMaterial.SetFloat(BlurSpread, spread);
            _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            Graphics.Blit(_blurMap1, _tempBlurMap, _blitMaterial, 1);
            _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            Graphics.Blit(_tempBlurMap, _blurMap2, _blitMaterial, 1);

            spread += 2 * extraSpread;

            // Blur the image 3
            _blitMaterial.SetFloat(BlurSpread, spread);
            _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            Graphics.Blit(_blurMap2, _tempBlurMap, _blitMaterial, 1);
            _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            Graphics.Blit(_tempBlurMap, _blurMap3, _blitMaterial, 1);

            spread += 4 * extraSpread;

            // Blur the image 4
            _blitMaterial.SetFloat(BlurSpread, spread);
            _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            Graphics.Blit(_blurMap3, _tempBlurMap, _blitMaterial, 1);
            _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            Graphics.Blit(_tempBlurMap, _blurMap4, _blitMaterial, 1);

            spread += 8 * extraSpread;

            // Blur the image 5
            _blitMaterial.SetFloat(BlurSpread, spread);
            _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            Graphics.Blit(_blurMap4, _tempBlurMap, _blitMaterial, 1);
            _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            Graphics.Blit(_tempBlurMap, _blurMap5, _blitMaterial, 1);

            spread += 16 * extraSpread;

            // Blur the image 6
            _blitMaterial.SetFloat(BlurSpread, spread);
            _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            Graphics.Blit(_blurMap5, _tempBlurMap, _blitMaterial, 1);
            _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            Graphics.Blit(_tempBlurMap, _blurMap6, _blitMaterial, 1);

            if (TextureManager.Instance.HdHeightMap)
            {
                _previewMaterial.SetTexture(MainTex, TextureManager.Instance.HdHeightMap);
            }
            else
            {
                _previewMaterial.SetTexture(MainTex, TextureManager.Instance.HeightMap);
            }


            _previewMaterial.SetTexture(BlurTex0, _blurMap0);
            _previewMaterial.SetTexture(BlurTex1, _blurMap1);
            _previewMaterial.SetTexture(BlurTex2, _blurMap2);
            _previewMaterial.SetTexture(BlurTex3, _blurMap3);
            _previewMaterial.SetTexture(BlurTex4, _blurMap4);
            _previewMaterial.SetTexture(BlurTex5, _blurMap5);
            _previewMaterial.SetTexture(BlurTex6, _blurMap6);

            yield return new WaitForSeconds(0.01f);

            Busy = false;
        }
    }
}