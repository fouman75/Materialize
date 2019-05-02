#region

using System.Collections;
using System.IO;
using System.Xml.Serialization;
using Materialize.General;
using Materialize.Settings;
using StandaloneFileBrowser;
using UnityEngine;
using Utility;
using SFB = StandaloneFileBrowser.StandaloneFileBrowser;

#endregion

namespace Materialize.Gui
{
    public abstract class TexturePanelGui : MonoBehaviour, IHideable
    {
        private TexturePanelSettings _settings;
        public ComputeShader BlurCompute;
        protected float GuiScale = 1.0f;
        protected Vector2Int ImageSize = new Vector2Int(1024, 1024);
        protected bool IsNewTexture;
        protected bool IsReadyToProcess;
        protected int KernelBlur;
        protected bool StuffToBeDone;


        public GameObject TestObject;

        public Material ThisMaterial;
        protected int WindowId;
        protected Rect WindowRect;

        public bool Active
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }

        public bool Hide { get; set; }

        public void DoStuff()
        {
            StuffToBeDone = true;
        }

        public void NewTexture()
        {
            IsNewTexture = true;
        }

        public void Close()
        {
            CleanupTextures();
            gameObject.SetActive(false);
        }

        protected abstract void CleanupTextures();

        protected void PostAwake()
        {
            var windowRectSize = WindowRect.size;
            windowRectSize.y += 40;
            WindowRect.size = windowRectSize;
            GuiScale -= 0.1f;
            WindowId = ProgramManager.Instance.GetWindowId;
            KernelBlur = BlurCompute.FindKernel("CSBlur");
        }

        public IEnumerator StartProcessing()
        {
            while (!ProgramManager.Lock()) yield return null;

            while (!IsReadyToProcess) yield return null;

            StartCoroutine(Process());

            yield return new WaitForSeconds(0.1f);

            MessagePanel.HideMessage();

            ProgramManager.Unlock();
        }

        protected abstract IEnumerator Process();

        protected void OnDisable()
        {
            CleanupTextures();
            IsReadyToProcess = false;
        }

        protected void DrawGuiExtras(int offsetX, int offsetY)
        {
            offsetY += 10;
            if (GUI.Button(new Rect(offsetX + 10, offsetY, 91, 25), "Defaults")) ResetSettings();

            if (GUI.Button(new Rect(offsetX + 113, offsetY, 70, 25), "Save")) SaveSettings();

            if (GUI.Button(new Rect(offsetX + 195, offsetY, 70, 25), "Load")) LoadSettings();
        }

        protected abstract void ResetSettings();
        protected abstract TexturePanelSettings GetSettings();
        protected abstract void SetSettings(TexturePanelSettings settings);

        private void SaveSettings()
        {
            _settings = GetSettings();
            var typeName = _settings.GetType().Name;
            var ext = $"{typeName[0]}{typeName[1]}{'s'}".ToLower();
            var extFilter = new[] {new ExtensionFilter(typeName, ext)};
            var defaultName = typeName + '.' + ext;
            SFB.SaveFilePanelAsync("Save Profile", PrefsManager.LastPath, defaultName, extFilter,
                SaveSettingsCallback);
        }

        private void SaveSettingsCallback(string path)
        {
            if (path.IsNullOrEmpty()) return;

            var lastBar = path.LastIndexOf(ProgramManager.PathChar);
            PrefsManager.LastPath = path.Substring(0, lastBar + 1);
            var serializer = new XmlSerializer(_settings.GetType());
            using (var stream = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(stream, _settings);
                stream.Close();
            }
        }

        private void LoadSettings()
        {
            _settings = GetSettings();
            var typeName = _settings.GetType().Name;
            var ext = $"{typeName[0]}{typeName[1]}{'s'}".ToLower();
            var extFilter = new[] {new ExtensionFilter(typeName, ext)};
            SFB.OpenFilePanelAsync("Load Profile", PrefsManager.LastPath, extFilter, false,
                LoadSettingsCallback);
        }

        private void LoadSettingsCallback(string[] pathArray)
        {
            var path = pathArray[0];
            if (path.IsNullOrEmpty()) return;
            if (!File.Exists(path)) return;

            var lastBar = path.LastIndexOf(ProgramManager.PathChar);
            PrefsManager.LastPath = path.Substring(0, lastBar + 1);
            var serializer = new XmlSerializer(_settings.GetType());
            object settings;
            using (var stream = new FileStream(path, FileMode.Open))
            {
                settings = serializer.Deserialize(stream);
                stream.Close();
            }

            SetSettings(settings as TexturePanelSettings);
            StuffToBeDone = true;
        }

        protected static void RunKernel(ComputeShader computeShader, int kernel, Texture source, Texture destiny)
        {
            var imageSize = new Vector2(source.width, source.height);
            computeShader.SetVector(ImageSizeId, imageSize);
            computeShader.SetTexture(kernel, "ImageInput", source);
            computeShader.SetTexture(kernel, "Result", destiny);
            var groupsX = (int) Mathf.Ceil(imageSize.x / 8f);
            var groupsY = (int) Mathf.Ceil(imageSize.y / 8f);
            computeShader.Dispatch(kernel, groupsX, groupsY, 1);
        }

        protected void BlurImage(float spread, Texture source, Texture dest)
        {
            var tempBlurMap = TextureManager.Instance.GetTempRenderTexture(source.width, source.height);
            var imageSize = new Vector2(source.width, source.height);
            BlurCompute.SetVector(ImageSizeId, imageSize);
            BlurCompute.SetFloat(BlurSpread, spread);
            BlurCompute.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            BlurCompute.SetTexture(KernelBlur, "ImageInput", source);
            BlurCompute.SetTexture(KernelBlur, "Result", tempBlurMap);
            var groupsX = (int) Mathf.Ceil(imageSize.x / 8f);
            var groupsY = (int) Mathf.Ceil(imageSize.y / 8f);
            BlurCompute.Dispatch(KernelBlur, groupsX, groupsY, 1);
            BlurCompute.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            BlurCompute.SetTexture(KernelBlur, "ImageInput", tempBlurMap);
            BlurCompute.SetTexture(KernelBlur, "Result", dest);
            BlurCompute.Dispatch(KernelBlur, groupsX, groupsY, 1);
            RenderTexture.ReleaseTemporary(tempBlurMap);
        }

        #region TextureIDs

        protected const float BlurScale = 1.0f;
        protected static readonly int BlurScaleId = Shader.PropertyToID("_BlurScale");
        protected static readonly int ImageSizeId = Shader.PropertyToID("_ImageSize");
        protected static readonly int Isolate = Shader.PropertyToID("_Isolate");
        protected static readonly int Blur0Weight = Shader.PropertyToID("_Blur0Weight");
        protected static readonly int Blur1Weight = Shader.PropertyToID("_Blur1Weight");
        protected static readonly int Blur2Weight = Shader.PropertyToID("_Blur2Weight");
        protected static readonly int Blur3Weight = Shader.PropertyToID("_Blur3Weight");
        protected static readonly int Blur4Weight = Shader.PropertyToID("_Blur4Weight");
        protected static readonly int Blur5Weight = Shader.PropertyToID("_Blur5Weight");
        protected static readonly int Blur6Weight = Shader.PropertyToID("_Blur6Weight");
        protected static readonly int Blur0Contrast = Shader.PropertyToID("_Blur0Contrast");
        protected static readonly int Blur1Contrast = Shader.PropertyToID("_Blur1Contrast");
        protected static readonly int Blur2Contrast = Shader.PropertyToID("_Blur2Contrast");
        protected static readonly int Blur3Contrast = Shader.PropertyToID("_Blur3Contrast");
        protected static readonly int Blur4Contrast = Shader.PropertyToID("_Blur4Contrast");
        protected static readonly int Blur5Contrast = Shader.PropertyToID("_Blur5Contrast");
        protected static readonly int Blur6Contrast = Shader.PropertyToID("_Blur6Contrast");
        protected static readonly int FinalGain = Shader.PropertyToID("_FinalGain");
        protected static readonly int FinalContrast = Shader.PropertyToID("_FinalContrast");
        protected static readonly int FinalBias = Shader.PropertyToID("_FinalBias");
        protected static readonly int Slider = Shader.PropertyToID("_Slider");
        protected static readonly int BlurTex0 = Shader.PropertyToID("_BlurTex0");
        protected static readonly int HeightFromNormal = Shader.PropertyToID("_HeightFromNormal");
        protected static readonly int BlurTex1 = Shader.PropertyToID("_BlurTex1");
        protected static readonly int BlurTex2 = Shader.PropertyToID("_BlurTex2");
        protected static readonly int BlurTex3 = Shader.PropertyToID("_BlurTex3");
        protected static readonly int BlurTex4 = Shader.PropertyToID("_BlurTex4");
        protected static readonly int BlurTex5 = Shader.PropertyToID("_BlurTex5");
        protected static readonly int BlurTex6 = Shader.PropertyToID("_BlurTex6");
        protected static readonly int AvgTex = Shader.PropertyToID("_AvgTex");
        protected static readonly int Spread = Shader.PropertyToID("_Spread");
        protected static readonly int SpreadBoost = Shader.PropertyToID("_SpreadBoost");
        protected static readonly int Samples = Shader.PropertyToID("_Samples");
        protected static readonly int MainTex = Shader.PropertyToID("_MainTex");
        protected static readonly int BlendTex = Shader.PropertyToID("_BlendTex");
        protected static readonly int IsNormal = Shader.PropertyToID("_IsNormal");
        protected static readonly int BlendAmount = Shader.PropertyToID("_BlendAmount");
        protected static readonly int Progress = Shader.PropertyToID("_Progress");
        protected static readonly int IsolateSample1 = Shader.PropertyToID("_IsolateSample1");
        protected static readonly int UseSample1 = Shader.PropertyToID("_UseSample1");
        protected static readonly int SampleColor1 = Shader.PropertyToID("_SampleColor1");
        protected static readonly int SampleUv1 = Shader.PropertyToID("_SampleUV1");
        protected static readonly int HueWeight1 = Shader.PropertyToID("_HueWeight1");
        protected static readonly int SatWeight1 = Shader.PropertyToID("_SatWeight1");
        protected static readonly int LumWeight1 = Shader.PropertyToID("_LumWeight1");
        protected static readonly int MaskLow1 = Shader.PropertyToID("_MaskLow1");
        protected static readonly int MaskHigh1 = Shader.PropertyToID("_MaskHigh1");
        protected static readonly int Sample1Height = Shader.PropertyToID("_Sample1Height");
        protected static readonly int IsolateSample2 = Shader.PropertyToID("_IsolateSample2");
        protected static readonly int UseSample2 = Shader.PropertyToID("_UseSample2");
        protected static readonly int SampleColor2 = Shader.PropertyToID("_SampleColor2");
        protected static readonly int SampleUv2 = Shader.PropertyToID("_SampleUV2");
        protected static readonly int HueWeight2 = Shader.PropertyToID("_HueWeight2");
        protected static readonly int SatWeight2 = Shader.PropertyToID("_SatWeight2");
        protected static readonly int LumWeight2 = Shader.PropertyToID("_LumWeight2");
        protected static readonly int MaskLow2 = Shader.PropertyToID("_MaskLow2");
        protected static readonly int MaskHigh2 = Shader.PropertyToID("_MaskHigh2");
        protected static readonly int Sample2Height = Shader.PropertyToID("_Sample2Height");
        protected static readonly int SampleBlend = Shader.PropertyToID("_SampleBlend");
        protected static readonly int BlurContrast = Shader.PropertyToID("_BlurContrast");
        protected static readonly int BlurSamples = Shader.PropertyToID("_BlurSamples");
        protected static readonly int BlurSpread = Shader.PropertyToID("_BlurSpread");
        protected static readonly int BlurDirection = Shader.PropertyToID("_BlurDirection");
        protected static readonly int AoBlend = Shader.PropertyToID("_AOBlend");
        protected static readonly int ImageInput = Shader.PropertyToID("ImageInput");
        protected static readonly int HeightTex = Shader.PropertyToID("_HeightTex");
        protected static readonly int Depth = Shader.PropertyToID("_Depth");
        protected static readonly int NormalTex = Shader.PropertyToID("_NormalTex");
        protected static readonly int LightMaskPow = Shader.PropertyToID("_LightMaskPow");
        protected static readonly int LightPow = Shader.PropertyToID("_LightPow");
        protected static readonly int DarkMaskPow = Shader.PropertyToID("_DarkMaskPow");
        protected static readonly int DarkPow = Shader.PropertyToID("_DarkPow");
        protected static readonly int HotSpot = Shader.PropertyToID("_HotSpot");
        protected static readonly int DarkSpot = Shader.PropertyToID("_DarkSpot");
        protected static readonly int ColorLerp = Shader.PropertyToID("_ColorLerp");
        protected static readonly int Saturation = Shader.PropertyToID("_Saturation");
        protected static readonly int BlurTex = Shader.PropertyToID("_BlurTex");
        protected static readonly int MetalColor = Shader.PropertyToID("_MetalColor");
        protected static readonly int SampleUv = Shader.PropertyToID("_SampleUV");
        protected static readonly int HueWeight = Shader.PropertyToID("_HueWeight");
        protected static readonly int SatWeight = Shader.PropertyToID("_SatWeight");
        protected static readonly int LumWeight = Shader.PropertyToID("_LumWeight");
        protected static readonly int MaskLow = Shader.PropertyToID("_MaskLow");
        protected static readonly int MaskHigh = Shader.PropertyToID("_MaskHigh");
        protected static readonly int BlurOverlay = Shader.PropertyToID("_BlurOverlay");
        protected static readonly int OverlayBlurTex = Shader.PropertyToID("_OverlayBlurTex");
        protected static readonly int Angularity = Shader.PropertyToID("_Angularity");
        protected static readonly int AngularIntensity = Shader.PropertyToID("_AngularIntensity");
        protected static readonly int HeightBlurTex = Shader.PropertyToID("_HeightBlurTex");
        protected static readonly int Desaturate = Shader.PropertyToID("_Desaturate");
        protected static readonly int LightTex = Shader.PropertyToID("_LightTex");
        protected static readonly int LightBlurTex = Shader.PropertyToID("_LightBlurTex");
        protected static readonly int LightRotation = Shader.PropertyToID("_LightRotation");
        protected static readonly int ShapeRecognition = Shader.PropertyToID("_ShapeRecognition");
        protected static readonly int ShapeBias = Shader.PropertyToID("_ShapeBias");
        protected static readonly int DiffuseTex = Shader.PropertyToID("_DiffuseTex");
        protected static readonly int MetalSmoothness = Shader.PropertyToID("_MetalSmoothness");
        protected static readonly int Sample1Smoothness = Shader.PropertyToID("_Sample1Smoothness");
        protected static readonly int Sample2Smoothness = Shader.PropertyToID("_Sample2Smoothness");
        protected static readonly int IsolateSample3 = Shader.PropertyToID("_IsolateSample3");
        protected static readonly int UseSample3 = Shader.PropertyToID("_UseSample3");
        protected static readonly int SampleColor3 = Shader.PropertyToID("_SampleColor3");
        protected static readonly int SampleUv3 = Shader.PropertyToID("_SampleUV3");
        protected static readonly int HueWeight3 = Shader.PropertyToID("_HueWeight3");
        protected static readonly int SatWeight3 = Shader.PropertyToID("_SatWeight3");
        protected static readonly int LumWeight3 = Shader.PropertyToID("_LumWeight3");
        protected static readonly int MaskLow3 = Shader.PropertyToID("_MaskLow3");
        protected static readonly int MaskHigh3 = Shader.PropertyToID("_MaskHigh3");
        protected static readonly int Sample3Smoothness = Shader.PropertyToID("_Sample3Smoothness");
        protected static readonly int BaseSmoothness = Shader.PropertyToID("_BaseSmoothness");
        protected static readonly int MetallicTex = Shader.PropertyToID("_MetallicTex");
        protected static readonly int ObjectScale = Shader.PropertyToID("_ObjectScale");
        protected static readonly int FlipY = Shader.PropertyToID("_FlipY");
        protected static readonly int SplatScale = Shader.PropertyToID("_SplatScale");
        protected static readonly int AspectRatio = Shader.PropertyToID("_AspectRatio");
        protected static readonly int TargetAspectRatio = Shader.PropertyToID("_TargetAspectRatio");
        protected static readonly int SplatRotation = Shader.PropertyToID("_SplatRotation");
        protected static readonly int SplatRotationRandom = Shader.PropertyToID("_SplatRotationRandom");
        protected static readonly int SplatKernel = Shader.PropertyToID("_SplatKernel");
        protected static readonly int Wobble = Shader.PropertyToID("_Wobble");
        protected static readonly int SplatRandomize = Shader.PropertyToID("_SplatRandomize");
        protected static readonly int TargetTex = Shader.PropertyToID("_TargetTex");
        protected static readonly int Falloff = Shader.PropertyToID("_Falloff");
        protected static readonly int OverlapX = Shader.PropertyToID("_OverlapX");
        protected static readonly int OverlapY = Shader.PropertyToID("_OverlapY");
        protected static readonly int IsHeight = Shader.PropertyToID("_IsHeight");
        protected static readonly int DiffuseMap = Shader.PropertyToID("_BaseColorMap");
        protected static readonly int MetallicMap = Shader.PropertyToID("_MetallicMap");
        protected static readonly int SmoothnessMap = Shader.PropertyToID("_SmoothnessMap");
        protected static readonly int NormalMap = Shader.PropertyToID("_NormalMap");
        protected static readonly int AoMap = Shader.PropertyToID("_AOMap");
        protected static readonly int TargetPoint = Shader.PropertyToID("_TargetPoint");
        protected static readonly int CorrectTex = Shader.PropertyToID("_CorrectTex");
        protected static readonly int PointScale = Shader.PropertyToID("_PointScale");
        protected static readonly int PointTl = Shader.PropertyToID("_PointTL");
        protected static readonly int PointTr = Shader.PropertyToID("_PointTR");
        protected static readonly int PointBl = Shader.PropertyToID("_PointBL");
        protected static readonly int PointBr = Shader.PropertyToID("_PointBR");
        protected static readonly int Lens = Shader.PropertyToID("_Lens");
        protected static readonly int PerspectiveX = Shader.PropertyToID("_PerspectiveX");
        protected static readonly int PerspectiveY = Shader.PropertyToID("_PerspectiveY");
        protected static readonly int DiffuseMapId = Shader.PropertyToID("_BaseColorMap");
        protected static readonly int NormalMapId = Shader.PropertyToID("_NormalMap");
        protected static readonly int MaskMapId = Shader.PropertyToID("_MaskMap");
        protected static readonly int HeightMapId = Shader.PropertyToID("_HeightMap");

        #endregion
    }
}