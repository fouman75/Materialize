#region

using System.Collections;
using General;
using Settings;
using UnityEngine;
using Logger = General.Logger;

#endregion

namespace Gui
{
    public class AoFromNormalGui : TexturePanelGui
    {
        private Texture2D _aoMap;

        private AoSettings _aos;
        private RenderTexture _blendedAoMap;

        private int _imageSizeX;
        private int _imageSizeY;
        private int _kernelAo;
        private int _kernelCombine;

        private bool _settingsInitialized;
        private float _slider = 0.5f;
        private Renderer _testObjectRenderer;
        private int _windowId;

        private Rect _windowRect;

        public ComputeShader AoCompute;


        protected override IEnumerator Process()
        {
            MessagePanel.ShowMessage("Processing Ambient Occlusion");

            var tempAoMap = TextureManager.Instance.GetTempRenderTexture(_imageSizeX, _imageSizeY);

            AoCompute.SetFloat(FinalBias, _aos.FinalBias);
            AoCompute.SetFloat(FinalContrast, _aos.FinalContrast);
            AoCompute.SetTexture(_kernelCombine, ImageInput, _blendedAoMap);
            AoCompute.SetFloat(AoBlend, _aos.Blend);
            AoCompute.SetVector(ImageSize, new Vector2(_imageSizeX, _imageSizeY));

            AoCompute.SetTexture(_kernelCombine, "ImageInput", _blendedAoMap);
            AoCompute.SetTexture(_kernelCombine, "Result", tempAoMap);
            var groupsX = (int) Mathf.Ceil(_imageSizeX / 8f);
            var groupsY = (int) Mathf.Ceil(_imageSizeY / 8f);
            AoCompute.Dispatch(_kernelCombine, groupsX, groupsY, 1);

            TextureManager.Instance.GetTextureFromRender(tempAoMap, ProgramEnums.MapType.Ao);
            RenderTexture.ReleaseTemporary(tempAoMap);
            yield break;
        }

        private void Awake()
        {
            _testObjectRenderer = TestObject.GetComponent<Renderer>();
            ThisMaterial = new Material(ThisMaterial.shader);
            _windowRect = new Rect(10.0f, 265.0f, 300f, 280f);
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

            StuffToBeDone = true;
        }

        private void InitializeSettings()
        {
            if (_settingsInitialized) return;
            _aos = new AoSettings {Blend = TextureManager.Instance && TextureManager.Instance.HeightMap ? 1.0f : 0.0f};

            _settingsInitialized = true;
        }

        private void Start()
        {
            MessagePanel.ShowMessage("Initializing AO GUI");
            _windowId = ProgramManager.Instance.GetWindowId;
            InitializeSettings();

            _kernelAo = AoCompute.FindKernel("CSAo");
            _kernelCombine = AoCompute.FindKernel("CSCombineAo");
        }

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
                StopAllCoroutines();
                StartCoroutine(ProcessNormalDepth());
                StuffToBeDone = false;
            }

            ThisMaterial.SetFloat(FinalContrast, _aos.FinalContrast);
            ThisMaterial.SetFloat(FinalBias, _aos.FinalBias);
            ThisMaterial.SetFloat(AoBlend, _aos.Blend);
            ThisMaterial.SetFloat(Slider, _slider);
        }

        private void DoMyWindow(int windowId)
        {
            const int offsetX = 10;
            var offsetY = 30;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "AO Reveal Slider");
            _slider = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 20, 280, 10), _slider, 0.0f, 1.0f);
            offsetY += 35;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "AO pixel Spread", _aos.Spread,
                out _aos.Spread, 10.0f, 100.0f)) StuffToBeDone = true;
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Pixel Depth", _aos.Depth,
                out _aos.Depth, 0.0f, 256.0f)) StuffToBeDone = true;
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Blend Normal AO and Depth AO", _aos.Blend,
                out _aos.Blend, 0.0f, 1.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "AO Power", _aos.FinalContrast,
                out _aos.FinalContrast, 0.1f, 10.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "AO Bias", _aos.FinalBias,
                out _aos.FinalBias, -1.0f, 1.0f);

            GUI.enabled = true;
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void OnGUI()
        {
            if (Hide) return;
            MainGui.MakeScaledWindow(_windowRect, _windowId, DoMyWindow, "Normal + Depth to AO");
        }

        public void InitializeTextures()
        {
            _testObjectRenderer.sharedMaterial = ThisMaterial;

            CleanupTextures();

            _imageSizeX = TextureManager.Instance.NormalMap.width;
            _imageSizeY = TextureManager.Instance.NormalMap.height;

            Logger.Log("Initializing Textures of size: " + _imageSizeX + "x" + _imageSizeY);

            _blendedAoMap = TextureManager.Instance.GetTempRenderTexture(_imageSizeX, _imageSizeY);
            ThisMaterial.SetTexture(NormalTex, TextureManager.Instance.NormalMap);
        }

        protected override void CleanupTextures()
        {
            RenderTexture.ReleaseTemporary(_blendedAoMap);
        }


        public IEnumerator ProcessNormalDepth()
        {
            while (!ProgramManager.Lock()) yield return null;

            MessagePanel.ShowMessage("Processing Normal Depth");

            AoCompute.SetVector(ImageSize, new Vector4(_imageSizeX, _imageSizeY, 0, 0));
            AoCompute.SetFloat(Spread, _aos.Spread);

            AoCompute.SetTexture(_kernelAo, ImageInput, TextureManager.Instance.NormalMap);

            Texture heightMap;
            if (TextureManager.Instance.HdHeightMap)
                heightMap = TextureManager.Instance.HdHeightMap;
            else if (TextureManager.Instance.HeightMap)
                heightMap = TextureManager.Instance.HeightMap;
            else
                heightMap = Texture2D.blackTexture;

            AoCompute.SetTexture(_kernelAo, HeightTex, heightMap);

            AoCompute.SetTexture(_kernelAo, BlendTex, _blendedAoMap);
            AoCompute.SetFloat(Depth, _aos.Depth);
            ThisMaterial.SetTexture(MainTex, _blendedAoMap);

            AoCompute.SetTexture(_kernelAo, "ImageInput", TextureManager.Instance.NormalMap);
            AoCompute.SetTexture(_kernelAo, "Result", _blendedAoMap);

            for (var i = 1; i < 100; i++)
            {
                AoCompute.SetFloat(BlendAmount, 1.0f / i);
                AoCompute.SetFloat(Progress, i / 100.0f);
                var groupsX = (int) Mathf.Ceil(_imageSizeX / 8f);
                var groupsY = (int) Mathf.Ceil(_imageSizeY / 8f);
                AoCompute.Dispatch(_kernelAo, groupsX, groupsY, 1);


                if (i % 25 == 0) yield return null;
            }

            yield return null;
            yield return null;

            IsReadyToProcess = true;

            MessagePanel.HideMessage();

            ProgramManager.Unlock();
        }
    }
}