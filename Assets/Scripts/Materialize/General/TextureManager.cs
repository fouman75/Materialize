#region

using System;
using System.Collections;
using JetBrains.Annotations;
using Materialize.Gui;
using UnityEngine;
using Utility;
using Logger = Utility.Logger;

#endregion

namespace Materialize.General
{
    public class TextureManager : MonoBehaviour
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const TextureFormat DefaultHdrTextureFormat = TextureFormat.RGBAHalf;

        // ReSharper disable once MemberCanBePrivate.Global
        public const TextureFormat DefaultLdrTextureFormat = TextureFormat.RGBA32;
        public static TextureManager Instance;

        private static readonly int DiffuseMapId = Shader.PropertyToID("_BaseColorMap");
        private static readonly int NormalMapId = Shader.PropertyToID("_NormalMap");
        private static readonly int HeightMapId = Shader.PropertyToID("_HeightMap");
        private static readonly int MetallicMapId = Shader.PropertyToID("_MetallicMap");
        private static readonly int SmoothnessMapId = Shader.PropertyToID("_SmoothnessMap");
        private static readonly int AoMapId = Shader.PropertyToID("_AoMap");
        private static readonly int DisplacementStrength = Shader.PropertyToID("_DisplacementStrength");
        private static readonly int DisplacementOffset = Shader.PropertyToID("_DisplacementOffset");
        private static readonly int FlipNormalYId = Shader.PropertyToID("_FlipNormalY");

        private Texture2D _blackTexture;
        private bool _displacementInitialized;

        private bool _flipNormalY;
        private Texture2D _packedNormal;

        [Range(0, 10)] public float DefaultDisplacement = 3.0f;
        public TextureFormat DefaultTextureFormat;
        public float DisplacementConstant = 0.25f;

        // ReSharper disable once RedundantDefaultMemberInitializer
        [UsedImplicitly] [SerializeField] private Material FullMaterial = null;
        public bool Hdr;
        public ComputeShader MaskMapCompute;
        public RenderTextureFormat RenderTextureFormat;

        [HideInInspector] public ProgramEnums.MapType TextureInClipboard;
        public ComputeShader TextureProcessingCompute;

        public bool FlipNormalY
        {
            set
            {
                Shader.SetGlobalInt(FlipNormalYId, value ? 1 : 0);
                _flipNormalY = value;
            }
            get => _flipNormalY;
        }

        public Material FullMaterialInstance { get; private set; }

        private void Awake()
        {
            Instance = this;
            HeightMap = null;
            HdHeightMap = null;
            DiffuseMap = null;
            DiffuseMapOriginal = null;
            NormalMap = null;
            MetallicMap = null;
            SmoothnessMap = null;
            MaskMap = null;
            AoMap = null;
            TextureInClipboard = ProgramEnums.MapType.None;
            FullMaterialInstance = new Material(FullMaterial);

            _blackTexture = GetStandardTexture(1, 1);
            _blackTexture.SetPixel(0, 0, Color.black);

            RenderTextureFormat = Hdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            DefaultTextureFormat = Hdr ? DefaultHdrTextureFormat : DefaultLdrTextureFormat;
        }

        private void Start()
        {
            ProgramManager.Instance.TestObject.GetComponent<Renderer>().material = FullMaterialInstance;
            Logger.Log("Starting " + name);
        }

        public bool NotNull(ProgramEnums.MapType mapType)
        {
            switch (mapType)
            {
                case ProgramEnums.MapType.Height:
                    return HeightMap;
                case ProgramEnums.MapType.Diffuse:
                    return DiffuseMap;
                case ProgramEnums.MapType.DiffuseOriginal:
                    return DiffuseMapOriginal;
                case ProgramEnums.MapType.Metallic:
                    return MetallicMap;
                case ProgramEnums.MapType.Smoothness:
                    return SmoothnessMap;
                case ProgramEnums.MapType.Normal:
                    return NormalMap;
                case ProgramEnums.MapType.Ao:
                    return AoMap;
                case ProgramEnums.MapType.Property:
                    return PropertyMap;
                case ProgramEnums.MapType.MaskMap:
                    return MaskMap;
                case ProgramEnums.MapType.AnyDiffuse:
                    return DiffuseMap || DiffuseMapOriginal;
                case ProgramEnums.MapType.None:
                    break;
                case ProgramEnums.MapType.Any:
                    return HeightMap || DiffuseMapOriginal || MetallicMap || SmoothnessMap || MaskMap || AoMap;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }

            return false;
        }

        public Material GetMaterialInstance()
        {
            return new Material(FullMaterial);
        }

        public bool GetCreationCondition(ProgramEnums.MapType mapType)
        {
            switch (mapType)
            {
                case ProgramEnums.MapType.Height:
                    return DiffuseMapOriginal || DiffuseMap || NormalMap;
                case ProgramEnums.MapType.None:
                case ProgramEnums.MapType.Any:
                case ProgramEnums.MapType.Diffuse:
                case ProgramEnums.MapType.DiffuseOriginal:
                    Logger.Log("A mistake?");
                    Debug.DebugBreak();
                    return false;
                case ProgramEnums.MapType.Metallic:
                case ProgramEnums.MapType.Smoothness:
                case ProgramEnums.MapType.Normal:
                    return HeightMap;
                case ProgramEnums.MapType.Ao:
                    return NormalMap;
                case ProgramEnums.MapType.Property:
                    return PropertyMap;
                case ProgramEnums.MapType.MaskMap:
                    return MetallicMap || SmoothnessMap || AoMap;
                case ProgramEnums.MapType.AnyDiffuse:
                    return DiffuseMap || DiffuseMapOriginal;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }
        }

        public Texture2D GetTexture(ProgramEnums.MapType mapType)
        {
            switch (mapType)
            {
                case ProgramEnums.MapType.Height:
                    return HeightMap;
                case ProgramEnums.MapType.Diffuse:
                    return DiffuseMap;
                case ProgramEnums.MapType.DiffuseOriginal:
                    return DiffuseMapOriginal;
                case ProgramEnums.MapType.Metallic:
                    return MetallicMap;
                case ProgramEnums.MapType.Smoothness:
                    return SmoothnessMap;
                case ProgramEnums.MapType.Normal:
                    return NormalMap;
                case ProgramEnums.MapType.Ao:
                    return AoMap;
                case ProgramEnums.MapType.Property:
                    return PropertyMap;
                case ProgramEnums.MapType.MaskMap:
                    return MaskMap;
                case ProgramEnums.MapType.AnyDiffuse:
                    return DiffuseMap ? DiffuseMap : DiffuseMapOriginal;
                case ProgramEnums.MapType.None:
                    break;
                case ProgramEnums.MapType.Any:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }

            return null;
        }

        public void SetFullMaterialAndUpdate()
        {
            MessagePanel.ShowMessage("Setting Material");

            if (HeightMap)
            {
                FullMaterialInstance.SetTexture(HeightMapId, HeightMap);
                if (!_displacementInitialized)
                {
                    SetDisplacement(DefaultDisplacement);
                    _displacementInitialized = true;
                }
            }
            else
            {
                FullMaterialInstance.SetTexture(HeightMapId, null);
            }

            FullMaterialInstance.SetTexture(DiffuseMapId,
                DiffuseMap ? DiffuseMap : DiffuseMapOriginal ? DiffuseMapOriginal : null);

            FullMaterialInstance.SetTexture(NormalMapId, NormalMap ? NormalMap : null);
            FullMaterialInstance.SetTexture(MetallicMapId, MetallicMap ? MetallicMap : null);
            FullMaterialInstance.SetTexture(SmoothnessMapId, SmoothnessMap ? SmoothnessMap : null);
            FullMaterialInstance.SetTexture(AoMapId, AoMap ? AoMap : null);

            ProgramManager.Instance.TestObject.GetComponent<Renderer>().material = FullMaterialInstance;
            ProgramManager.Instance.MaterialGuiObject.GetComponent<MaterialGui>().Initialize();

            MessagePanel.HideMessage();
        }

        public void SetDisplacement(float displacementAmplitude)
        {
            FullMaterialInstance.SetFloat(DisplacementStrength, displacementAmplitude);
            var offset = DisplacementConstant * (displacementAmplitude + 1.0f);
            FullMaterialInstance.SetFloat(DisplacementOffset, offset);
        }

        public void SetFullMaterial()
        {
            ProgramManager.Instance.TestObject.GetComponent<Renderer>().material = FullMaterialInstance;
            ProgramManager.Instance.MaterialGuiObject.GetComponent<MaterialGui>().Initialize();
        }

        // ReSharper disable once UnusedMember.Local
        private IEnumerator PackNormalAndSet()
        {
            while (!ProgramManager.Lock()) yield return null;

            var tempRenderTexture = GetTempRenderTexture(NormalMap.width, NormalMap.height);
            var kernel = TextureProcessingCompute.FindKernel("CSPackNormal");
            var size = new Vector2Int(NormalMap.width, NormalMap.height);
            TextureProcessingCompute.SetVector("_ImageSize", (Vector2) size);
            TextureProcessingCompute.SetTexture(kernel, "Input", NormalMap);
            TextureProcessingCompute.SetTexture(kernel, "Result", tempRenderTexture);
            TextureProcessingCompute.Dispatch(kernel, size.x / 8, size.y / 8, 1);

            yield return new WaitForSeconds(0.5f);

            GetTextureFromRender(tempRenderTexture, out _packedNormal);
            FullMaterialInstance.SetTexture(NormalMapId, _packedNormal);

            ProgramManager.Unlock();
        }

        public Texture2D GetStandardTexture(int width, int height, bool linear = true)
        {
            return Hdr ? GetStandardHdrTexture(width, height, linear) : GetStandardLdrTexture(width, height, linear);
        }

        private static Texture2D GetStandardHdrTexture(int width, int height, bool linear = true)
        {
            var texture = new Texture2D(width, height, DefaultHdrTextureFormat, true, linear)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat
            };
            return texture;
        }


        private static Texture2D GetStandardLdrTexture(int width, int height, bool linear = true)
        {
            var texture = new Texture2D(width, height, DefaultLdrTextureFormat, true, linear)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat
            };
            return texture;
        }

        public RenderTexture GetTempRenderTexture(int width, int height, bool forceGama = false, bool mono = false)
        {
            var format = mono ? RenderTextureFormat.RFloat : RenderTextureFormat;

            var rt = forceGama
                ? RenderTexture.GetTemporary(width, height, 24, format, RenderTextureReadWrite.sRGB)
                : RenderTexture.GetTemporary(width, height, 24, format, RenderTextureReadWrite.Linear);

            rt.enableRandomWrite = true;
            rt.wrapMode = TextureWrapMode.Repeat;

            rt.Create();

            return rt;
        }

        public void GetTextureFromRender(RenderTexture input, ProgramEnums.MapType mapType)
        {
            GetTextureFromRender(input, mapType, out _);
        }

        public void GetTextureFromRender(RenderTexture input, out Texture2D outTexture)
        {
            GetTextureFromRender(input, ProgramEnums.MapType.None, out outTexture);
        }

        private void GetTextureFromRender(RenderTexture input, ProgramEnums.MapType mapType, out Texture2D outTexture)
        {
            RenderTexture.active = input;
            var texture = GetStandardTexture(input.width, input.height);
            texture.ReadPixels(new Rect(0, 0, input.width, input.height), 0, 0, false);
            texture.Apply(true);

            switch (mapType)
            {
                case ProgramEnums.MapType.None:
                    break;
                case ProgramEnums.MapType.Any:
                    break;
                case ProgramEnums.MapType.Height:
                    HeightMap = texture;
                    break;
                case ProgramEnums.MapType.Diffuse:
                    DiffuseMap = texture;
                    break;
                case ProgramEnums.MapType.DiffuseOriginal:
                    DiffuseMapOriginal = texture;
                    break;
                case ProgramEnums.MapType.AnyDiffuse:
                    break;
                case ProgramEnums.MapType.Metallic:
                    MetallicMap = texture;
                    break;
                case ProgramEnums.MapType.Smoothness:
                    SmoothnessMap = texture;
                    break;
                case ProgramEnums.MapType.Normal:
                    NormalMap = texture;
                    break;
                case ProgramEnums.MapType.Ao:
                    AoMap = texture;
                    break;
                case ProgramEnums.MapType.Property:
                    break;
                case ProgramEnums.MapType.MaskMap:
                    MaskMap = texture;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }

            outTexture = texture;
        }

        public void ClearAllTextures()
        {
            ClearTexture(ProgramEnums.MapType.Height);
            ClearTexture(ProgramEnums.MapType.AnyDiffuse);
            ClearTexture(ProgramEnums.MapType.Normal);
            ClearTexture(ProgramEnums.MapType.Metallic);
            ClearTexture(ProgramEnums.MapType.Smoothness);
            ClearTexture(ProgramEnums.MapType.MaskMap);
            ClearTexture(ProgramEnums.MapType.Ao);

            SetFullMaterialAndUpdate();
        }

        public void ClearTexture(ProgramEnums.MapType mapType)
        {
            switch (mapType)
            {
                case ProgramEnums.MapType.Height:
                    if (HeightMap)
                    {
                        FullMaterialInstance.SetTexture(HeightMapId, null);
                        Destroy(HeightMap);
                        HeightMap = null;
                    }

                    if (HdHeightMap) RenderTexture.ReleaseTemporary(HdHeightMap);

                    break;
                case ProgramEnums.MapType.Diffuse:
                    if (DiffuseMap)
                    {
                        FullMaterialInstance.SetTexture(DiffuseMapId, null);
                        Destroy(DiffuseMap);
                        DiffuseMap = null;
                    }

                    break;
                case ProgramEnums.MapType.Normal:
                    if (NormalMap)
                    {
                        FullMaterialInstance.SetTexture(NormalMapId, null);
                        Destroy(NormalMap);
                        NormalMap = null;
                    }

                    break;
                case ProgramEnums.MapType.Metallic:
                    if (MetallicMap)
                    {
                        Destroy(MetallicMap);
                        MetallicMap = null;
                    }

                    break;
                case ProgramEnums.MapType.Smoothness:
                    if (SmoothnessMap)
                    {
                        Destroy(SmoothnessMap);
                        SmoothnessMap = null;
                    }

                    break;
                case ProgramEnums.MapType.MaskMap:
                    if (MaskMap)
                    {
                        Destroy(MaskMap);
                        MaskMap = null;
                    }

                    break;
                case ProgramEnums.MapType.Ao:
                    if (AoMap)
                    {
                        Destroy(AoMap);
                        AoMap = null;
                    }

                    break;
                case ProgramEnums.MapType.DiffuseOriginal:
                    if (DiffuseMapOriginal)
                    {
                        FullMaterialInstance.SetTexture(DiffuseMapId, null);
                        Destroy(DiffuseMapOriginal);
                        DiffuseMapOriginal = null;
                    }

                    break;
                case ProgramEnums.MapType.Property:
                    break;
                case ProgramEnums.MapType.None:
                    break;
                case ProgramEnums.MapType.Any:
                    break;
                case ProgramEnums.MapType.AnyDiffuse:
                    ClearTexture(ProgramEnums.MapType.Diffuse);
                    ClearTexture(ProgramEnums.MapType.DiffuseOriginal);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }

            var panels = FindObjectsOfType<TexturePanel>();
            foreach (var panel in panels)
                if (panel.MapType == mapType)
                    panel.TextureFrame.texture = null;
        }

        [UsedImplicitly]
        public void ClearAllButtonCallback()
        {
            ClearAllTextures();
            MainGui.Instance.CloseWindows();
            FixSizeSize(1024.0f, 1024.0f);
        }

        public IEnumerator MakeMaskMap()
        {
            while (!ProgramManager.Lock()) yield return null;

            MessagePanel.ShowMessage("Processing Mask Map");

            MaskMap = TextureProcessing.BlitMaskMap(MetallicMap, SmoothnessMap, AoMap);

            yield return new WaitForSeconds(0.5f);

            MessagePanel.HideMessage();

            ProgramManager.Unlock();

            SetFullMaterialAndUpdate();
        }

        public void FlipNormalYCallback()
        {
            NormalMap = TextureProcessing.FlipNormalMapY(NormalMap);
            FlipNormalY = !FlipNormalY;
            SetFullMaterialAndUpdate();
        }

        public void SaveMap(ProgramEnums.MapType mapType)
        {
            var defaultName = "_" + mapType + ".png";
            _textureToSave = mapType;
            var lastPath = PrefsManager.LastPath;
            StandaloneFileBrowser.StandaloneFileBrowser.SaveFilePanelAsync("Save Height Map", lastPath, defaultName,
                ProgramManager.ImageSaveFilter, SaveTextureFileCallback);
        }

        private void SaveTextureFileCallback(string path)
        {
            if (path.IsNullOrEmpty()) return;

            var lastBar = path.LastIndexOf(ProgramManager.PathChar);
            PrefsManager.LastPath = path.Substring(0, lastBar + 1);
            var textureToSave = GetTexture(_textureToSave);
            SaveLoadProject.Instance.SaveFile(path, textureToSave);
        }

        public void SetUvScale(Vector2 scale)
        {
            FullMaterialInstance.SetTextureScale(DiffuseMapId, scale);
        }

        public void SetUvOffset(Vector2 offset)
        {
            FullMaterialInstance.SetTextureOffset(DiffuseMapId, offset);
        }

        #region Map Variables

        public RenderTexture HdHeightMap;
        public Texture2D HeightMap;
        public Texture2D DiffuseMap;
        public Texture2D DiffuseMapOriginal;
        public Texture2D NormalMap;
        public Texture2D MetallicMap;
        public Texture2D SmoothnessMap;
        public Texture2D AoMap;
        public Texture2D MaskMap;
        public Texture2D PropertyMap;
        private ProgramEnums.MapType _textureToSave;

        #endregion

        #region Fix Size

        public Vector2Int GetSize()
        {
            Texture2D mapToUse = null;

            var size = new Vector2Int(1024, 1024);

            if (HeightMap != null)
                mapToUse = HeightMap;
            else if (DiffuseMap != null)
                mapToUse = DiffuseMap;
            else if (DiffuseMapOriginal != null)
                mapToUse = DiffuseMapOriginal;
            else if (NormalMap != null)
                mapToUse = NormalMap;
            else if (MetallicMap != null)
                mapToUse = MetallicMap;
            else if (SmoothnessMap != null)
                mapToUse = SmoothnessMap;
            else if (MaskMap != null)
                mapToUse = MaskMap;
            else if (AoMap != null) mapToUse = AoMap;

            if (mapToUse == null) return size;
            size.x = mapToUse.width;
            size.y = mapToUse.height;

            return size;
        }

        public void FixSize()
        {
            var size = GetSize();
            FixSizeSize(size.x, size.y);
        }

        [UsedImplicitly]
        public static void FixSizeMap(Texture mapToUse)
        {
            FixSizeSize(mapToUse.width, mapToUse.height);
        }

        [UsedImplicitly]
        public static void FixSizeMap(RenderTexture mapToUse)
        {
            FixSizeSize(mapToUse.width, mapToUse.height);
        }

        private static void FixSizeSize(float width, float height)
        {
            var testObjectScale = new Vector3(1, 1, 1);
            const float area = 1.0f;

            testObjectScale.x = width / height;

            var newArea = testObjectScale.x * testObjectScale.y;
            var areaScale = Mathf.Sqrt(area / newArea);

            testObjectScale.x *= areaScale;
            testObjectScale.y *= areaScale;

            if (ProgramManager.Instance.TestObject.transform.parent &&
                ProgramManager.Instance.TestObject.GetComponentInParent<MeshFilter>() != null)
                ProgramManager.Instance.TestObject.transform.parent.localScale.Scale(testObjectScale);
            else
                ProgramManager.Instance.TestObject.transform.localScale.Scale(testObjectScale);
        }

        #endregion
    }
}