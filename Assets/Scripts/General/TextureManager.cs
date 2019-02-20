using System;
using Gui;
using JetBrains.Annotations;
using Plugins.Extension;
using UnityEngine;

namespace General
{
    public class TextureManager : MonoBehaviour
    {
        public static TextureManager Instance;
        public bool Hdr;
        public const TextureFormat DefaultHdrTextureFormat = TextureFormat.RGBAHalf;
        public const TextureFormat DefaultLdrTextureFormat = TextureFormat.RGBA32;
        public RenderTextureFormat RenderTextureFormat;

        [HideInInspector] public ProgramEnums.MapType TextureInClipboard;

        // ReSharper disable once RedundantDefaultMemberInitializer
        [UsedImplicitly] [SerializeField] private Material FullMaterial = null;
        public Material FullMaterialInstance { get; private set; }

        private static readonly int DiffuseMapId = Shader.PropertyToID("_BaseColorMap");
        private static readonly int NormalMapId = Shader.PropertyToID("_NormalMap");
        private static readonly int MaskMapId = Shader.PropertyToID("_MaskMap");
        private static readonly int HeightMapId = Shader.PropertyToID("_HeightMap");

        private Texture2D _blackTexture;

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
        }

        private void Start()
        {
            ProgramManager.Instance.TestObject.GetComponent<Renderer>().material = FullMaterialInstance;
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
                    return (DiffuseMap || DiffuseMapOriginal);
                case ProgramEnums.MapType.None:
                    break;
                case ProgramEnums.MapType.Any:
                    return (HeightMap || DiffuseMapOriginal || MetallicMap || SmoothnessMap || MaskMap || AoMap);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }

            return false;
        }

        public bool GetCreationCondition(ProgramEnums.MapType mapType)
        {
            switch (mapType)
            {
                case ProgramEnums.MapType.Height:
                    if (DiffuseMapOriginal == null && DiffuseMap == null && NormalMap == null) return false;
                    return true;
                case ProgramEnums.MapType.None:
                case ProgramEnums.MapType.Any:
                case ProgramEnums.MapType.Diffuse:
                case ProgramEnums.MapType.DiffuseOriginal:
                    Debug.Log("A mistake?");
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

        public void SetFullMaterial()
        {
            if (HeightMap)
            {
                FullMaterialInstance.EnableKeyword("_TESSELLATION_DISPLACEMENT");
                FullMaterialInstance.EnableKeyword("_HEIGHTMAP");
                FullMaterialInstance.SetTexture(HeightMapId, HeightMap);
            }
            else
            {
                FullMaterialInstance.SetTexture(HeightMapId, Texture2D.blackTexture);
            }

            if (DiffuseMap != null)
                FullMaterialInstance.SetTexture(DiffuseMapId, DiffuseMap);
            else if (DiffuseMapOriginal != null)
                FullMaterialInstance.SetTexture(DiffuseMapId, DiffuseMapOriginal);
            else
            {
                FullMaterialInstance.SetTexture(DiffuseMapId, Texture2D.whiteTexture);
            }


            if (NormalMap)
            {
                // ReSharper disable once StringLiteralTypo
                FullMaterialInstance.EnableKeyword("_NORMALMAP");
                FullMaterialInstance.SetTexture(NormalMapId, NormalMap);
            }
            else
            {
                FullMaterialInstance.SetTexture(NormalMapId, Texture2D.normalTexture);
            }

            if (MaskMap)
            {
                // ReSharper disable once StringLiteralTypo
                FullMaterialInstance.EnableKeyword("_MASKMAP");
                FullMaterialInstance.SetTexture(MaskMapId, MaskMap);
            }
            else
            {
                FullMaterialInstance.SetTexture(MaskMapId, _blackTexture);
            }

            ProgramManager.Instance.TestObject.GetComponent<Renderer>().material = FullMaterialInstance;
        }

        public Texture2D GetStandardTexture(int width, int height, bool linear = true)
        {
            if (Hdr)
            {
                return GetStandardHdrTexture(width, height, linear);
            }
            else
            {
                return GetStandardLdrTexture(width, height, linear);
            }
        }

        public static Texture2D GetStandardHdrTexture(int width, int height, bool linear = true)
        {
            var texture = new Texture2D(width, height, DefaultHdrTextureFormat, false, linear)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point
            };
            return texture;
        }


        public static Texture2D GetStandardLdrTexture(int width, int height, bool linear = true)
        {
            var texture = new Texture2D(width, height, DefaultLdrTextureFormat, false, linear)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point
            };
            return texture;
        }

        public RenderTexture GetTempRenderTexture(int width, int height, bool forceGama = false)
        {
            var rt = forceGama
                ? RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat, RenderTextureReadWrite.sRGB)
                : RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat, RenderTextureReadWrite.Default);


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

        public void GetTextureFromRender(RenderTexture input, ProgramEnums.MapType mapType, out Texture2D outTexture)
        {
            RenderTexture.active = input;
            var texture = GetStandardTexture(input.width, input.height);
            texture.ReadPixels(new Rect(0, 0, input.width, input.height), 0, 0, false);
            texture.Apply(false);

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

            SetFullMaterial();
        }

        public void ClearTexture(ProgramEnums.MapType mapType)
        {
            switch (mapType)
            {
                case ProgramEnums.MapType.Height:
                    if (HeightMap)
                    {
                        FullMaterialInstance.SetTexture(HeightMapId, Texture2D.whiteTexture);
                        Destroy(HeightMap);
                        HeightMap = null;
                    }

                    if (HdHeightMap)
                    {
                        RenderTexture.ReleaseTemporary(HdHeightMap);
                    }

                    break;
                case ProgramEnums.MapType.Diffuse:
                    if (DiffuseMap)
                    {
                        FullMaterialInstance.SetTexture(DiffuseMapId, Texture2D.whiteTexture);
                        Destroy(DiffuseMap);
                        DiffuseMap = null;
                    }

                    break;
                case ProgramEnums.MapType.Normal:
                    if (NormalMap)
                    {
                        FullMaterialInstance.SetTexture(NormalMapId, Texture2D.normalTexture);
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
                        FullMaterialInstance.SetTexture(MaskMapId, Texture2D.whiteTexture);
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
                        FullMaterialInstance.SetTexture(DiffuseMapId, Texture2D.whiteTexture);
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

            Resources.UnloadUnusedAssets();
            var panels = FindObjectsOfType<TexturePanel>();
            foreach (var panel in panels)
            {
                if (panel.MapType == mapType) panel.TextureFrame.texture = null;
            }
        }

        public void ClearAllButtonCallback()
        {
            ClearAllTextures();
            MainGui.Instance.CloseWindows();
            FixSizeSize(1024.0f, 1024.0f);
        }

        public void MakeMaskMap()
        {
            MaskMap = TextureProcessing.BlitMaskMap(MetallicMap, SmoothnessMap, AoMap);
            SetFullMaterial();
        }

        public void FlipNormalY()
        {
            NormalMap = TextureProcessing.FlipNormalMapY(NormalMap);
        }

        public void SaveMap(ProgramEnums.MapType mapType)
        {
            var defaultName = "_" + mapType + ".png";
            _textureToSave = mapType;
            var lastPath = ProgramManager.Instance.LastPath;
            StandaloneFileBrowser.StandaloneFileBrowser.SaveFilePanelAsync("Save Height Map", lastPath, defaultName,
                ProgramManager.ImageSaveFilter, SaveTextureFileCallback);
        }

        private void SaveTextureFileCallback(string path)
        {
            if (path.IsNullOrEmpty()) return;

            var lastBar = path.LastIndexOf(ProgramManager.Instance.PathChar);
            ProgramManager.Instance.LastPath = path.Substring(0, lastBar + 1);
            var textureToSave = GetTexture(_textureToSave);
            SaveLoadProject.Instance.SaveFile(path, textureToSave);
        }

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

        public static void FixSizeMap(Texture mapToUse)
        {
            FixSizeSize(mapToUse.width, mapToUse.height);
        }

        public static void FixSizeMap(RenderTexture mapToUse)
        {
            FixSizeSize(mapToUse.width, mapToUse.height);
        }

        public static void FixSizeSize(float width, float height)
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
            {
                ProgramManager.Instance.TestObject.transform.parent.localScale.Scale(testObjectScale);
            }
            else
            {
                ProgramManager.Instance.TestObject.transform.localScale.Scale(testObjectScale);
            }
        }

        #endregion
    }
}