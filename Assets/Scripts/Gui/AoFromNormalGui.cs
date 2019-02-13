#region

using System.Collections;
using UnityEngine;
using Utility;

#endregion

namespace Gui
{
    public class AoFromNormalGui : MonoBehaviour
    {
        private static readonly int FinalContrast = Shader.PropertyToID("_FinalContrast");
        private static readonly int FinalBias = Shader.PropertyToID("_FinalBias");
        private static readonly int AoBlend = Shader.PropertyToID("_AOBlend");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int ImageSize = Shader.PropertyToID("_ImageSize");
        private static readonly int Spread = Shader.PropertyToID("_Spread");
        private static readonly int HeightTex = Shader.PropertyToID("_HeightTex");
        private static readonly int BlendTex = Shader.PropertyToID("_BlendTex");
        private static readonly int Depth = Shader.PropertyToID("_Depth");
        private static readonly int BlendAmount = Shader.PropertyToID("_BlendAmount");
        private static readonly int Progress = Shader.PropertyToID("_Progress");
        private Texture2D _aoMap;

        private AoSettings _aos;
        private RenderTexture _blendedAoMap;
        private Material _blitMaterial;
        private bool _doStuff;

        private int _imageSizeX = 1024;
        private int _imageSizeY = 1024;
        private bool _newTexture;

        private bool _settingsInitialized;

        private Rect _windowRect = new Rect(30, 300, 300, 230);

        [HideInInspector] public bool Busy;
        public Texture2D DefaultHeight;

        private MainGui _mainGui;

        public GameObject TestObject;

        public Material ThisMaterial;
        private Coroutine _processingNormalCoroutine;
        private Renderer _testObjectRenderer;
        private Coroutine _processingAoCoroutine;

        private void Awake()
        {
            _testObjectRenderer = TestObject.GetComponent<Renderer>();
            ThisMaterial = new Material(ThisMaterial.shader);
            _mainGui = MainGui.Instance;
        }

        public void GetValues(ProjectObject projectObject)
        {
            InitializeSettings();
            projectObject.AoSettings = _aos;
        }

        public void SetValues(ProjectObject projectObject)
        {
            if (projectObject.AoSettings != null)
            {
                _aos = projectObject.AoSettings;
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
            _aos = new AoSettings {Blend = _mainGui && _mainGui.HeightMap ? 1.0f : 0.0f};

            _settingsInitialized = true;
        }

        private void Start()
        {
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

        private void Update()
        {
            if (_newTexture)
            {
                InitializeTextures();
                _newTexture = false;
            }

            if (_doStuff)
            {
                if (_processingNormalCoroutine != null) StopCoroutine(_processingNormalCoroutine);
                if (_processingAoCoroutine != null) StopCoroutine(_processingAoCoroutine);
                _processingNormalCoroutine = StartCoroutine(ProcessNormalDepth());
                _doStuff = false;
            }

            ThisMaterial.SetFloat(FinalContrast, _aos.FinalContrast);
            ThisMaterial.SetFloat(FinalBias, _aos.FinalBias);
            ThisMaterial.SetFloat(AoBlend, _aos.Blend);
        }

        private void DoMyWindow(int windowId)
        {
            const int offsetX = 10;
            var offsetY = 30;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "AO pixel Spread", _aos.Spread,
                out _aos.Spread, 10.0f, 100.0f)) _doStuff = true;
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Pixel Depth", _aos.Depth,
                out _aos.Depth, 0.0f, 256.0f)) _doStuff = true;
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Blend Normal AO and Depth AO", _aos.Blend,
                out _aos.Blend, 0.0f, 1.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "AO Power", _aos.FinalContrast,
                out _aos.FinalContrast, 0.1f, 10.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "AO Bias", _aos.FinalBias,
                out _aos.FinalBias, -1.0f, 1.0f);
            offsetY += 50;

            GUI.enabled = !Busy;
            if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Set as AO Map"))
            {
                _processingAoCoroutine = StartCoroutine(ProcessAo());
            }

            GUI.enabled = true;
            GUI.DragWindow();
        }

        private void OnGUI()
        {
            _windowRect.width = 300;
            _windowRect.height = 280;

            _windowRect = GUI.Window(10, _windowRect, DoMyWindow, "Normal + Depth to AO");
        }

        public void InitializeTextures()
        {
            _testObjectRenderer.sharedMaterial = ThisMaterial;

            CleanupTextures();

            _imageSizeX = _mainGui.NormalMap.width;
            _imageSizeY = _mainGui.NormalMap.height;

            Debug.Log("Initializing Textures of size: " + _imageSizeX + "x" + _imageSizeY);

            _blendedAoMap = RenderTexture.GetTemporary(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RGHalf,
                RenderTextureReadWrite.Linear);
        }

        public void Close()
        {
            CleanupTextures();
            gameObject.SetActive(false);
        }

        private static void CleanupTexture(RenderTexture texture)
        {
            if (!texture) return;
            texture.Release();
            // ReSharper disable once RedundantAssignment
            texture = null;
        }

        private void CleanupTextures()
        {
            RenderTexture.ReleaseTemporary(_blendedAoMap);
        }

        public IEnumerator ProcessAo()
        {
            yield return _processingNormalCoroutine;
            Busy = true;

            Debug.Log("Processing AO Map");

            var tempAoMap = RenderTexture.GetTemporary(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear);

            _blitMaterial.SetFloat(FinalBias, _aos.FinalBias);
            _blitMaterial.SetFloat(FinalContrast, _aos.FinalContrast);
            _blitMaterial.SetTexture(MainTex, _blendedAoMap);
            _blitMaterial.SetFloat(AoBlend, _aos.Blend);

            Graphics.Blit(_blendedAoMap, tempAoMap, _blitMaterial, 8);


            if (_mainGui.AoMap) Destroy(_mainGui.AoMap);

            RenderTexture.active = tempAoMap;
            _mainGui.AoMap = new Texture2D(tempAoMap.width, tempAoMap.height, TextureFormat.ARGB32, false, true);
            _mainGui.AoMap.ReadPixels(new Rect(0, 0, tempAoMap.width, tempAoMap.height), 0, 0);
            _mainGui.AoMap.Apply(false);
            RenderTexture.ReleaseTemporary(tempAoMap);

            yield return new WaitForSeconds(0.1f);

            Busy = false;
        }

        public IEnumerator ProcessNormalDepth()
        {
            Busy = true;
            Debug.Log("Processing Normal Depth to AO");

            _blitMaterial.SetVector(ImageSize, new Vector4(_imageSizeX, _imageSizeY, 0, 0));
            _blitMaterial.SetFloat(Spread, _aos.Spread);

            _blitMaterial.SetTexture(MainTex, _mainGui.NormalMap);

            if (_mainGui.HdHeightMap)
                _blitMaterial.SetTexture(HeightTex, _mainGui.HdHeightMap);
            else if (_mainGui.HeightMap)
                _blitMaterial.SetTexture(HeightTex, _mainGui.HeightMap);
            else
                _blitMaterial.SetTexture(HeightTex, DefaultHeight);

            _blitMaterial.SetTexture(BlendTex, _blendedAoMap);
            _blitMaterial.SetFloat(Depth, _aos.Depth);
            ThisMaterial.SetTexture(MainTex, _blendedAoMap);

            for (var i = 1; i < 100; i++)
            {
                _blitMaterial.SetFloat(BlendAmount, 1.0f / i);
                _blitMaterial.SetFloat(Progress, i / 100.0f);


                Graphics.Blit(_mainGui.NormalMap, _blendedAoMap, _blitMaterial, 7);

                if (i % 10 == 0) yield return null;
            }

            Busy = false;
        }
    }
}