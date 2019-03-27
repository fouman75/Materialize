#region

using System;
using System.Collections;
using General;
using UnityEngine;
using Logger = General.Logger;
using Random = UnityEngine.Random;

#endregion

namespace Gui
{
    public class TilingTextureMakerGui : MonoBehaviour, IHideable
    {
        private static readonly int Tiling = Shader.PropertyToID("_Tiling");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int HeightTex = Shader.PropertyToID("_HeightTex");
        private static readonly int ObjectScale = Shader.PropertyToID("_ObjectScale");
        private static readonly int FlipY = Shader.PropertyToID("_FlipY");
        private static readonly int SplatScale = Shader.PropertyToID("_SplatScale");
        private static readonly int AspectRatio = Shader.PropertyToID("_AspectRatio");
        private static readonly int TargetAspectRatio = Shader.PropertyToID("_TargetAspectRatio");
        private static readonly int SplatRotation = Shader.PropertyToID("_SplatRotation");
        private static readonly int SplatRotationRandom = Shader.PropertyToID("_SplatRotationRandom");
        private static readonly int SplatKernel = Shader.PropertyToID("_SplatKernel");
        private static readonly int Wobble = Shader.PropertyToID("_Wobble");
        private static readonly int SplatRandomize = Shader.PropertyToID("_SplatRandomize");
        private static readonly int TargetTex = Shader.PropertyToID("_TargetTex");
        private static readonly int Falloff = Shader.PropertyToID("_Falloff");
        private static readonly int OverlapX = Shader.PropertyToID("_OverlapX");
        private static readonly int OverlapY = Shader.PropertyToID("_OverlapY");
        private static readonly int IsHeight = Shader.PropertyToID("_IsHeight");
        private static readonly int DiffuseMap = Shader.PropertyToID("_BaseColorMap");
        private static readonly int MetallicMap = Shader.PropertyToID("_MetallicMap");
        private static readonly int SmoothnessMap = Shader.PropertyToID("_SmoothnessMap");
        private static readonly int IsNormal = Shader.PropertyToID("_IsNormal");
        private static readonly int NormalMap = Shader.PropertyToID("_NormalMap");
        private static readonly int AoMap = Shader.PropertyToID("_AOMap");
        private RenderTexture _aoMapTemp;

        private Material _blitMaterial;
        private RenderTexture _diffuseMapOriginalTemp;
        private RenderTexture _diffuseMapTemp;

        private bool _doStuff;

        private float _falloff = 0.1f;
        private RenderTexture _hdHeightMapTemp;
        private RenderTexture _heightMapTemp;

        private float _lastFalloff = 0.1f;

        private int _lastNewTexSelectionX = 2;
        private int _lastNewTexSelectionY = 2;
        private float _lastOverlapX = 0.2f;
        private float _lastOverlapY = 0.2f;
        private TileTechnique _lastTileTech = TileTechnique.Overlap;
        private RenderTexture _metallicMapTemp;

        private int _newTexSelectionX = 2;
        private int _newTexSelectionY = 2;

        private int _newTexSizeX = 1024;
        private int _newTexSizeY = 1024;
        private RenderTexture _normalMapTemp;

        private Vector3 _objectScale = Vector3.one;

        private Vector2[] _offsetKernel;
        private float _overlapX = 0.2f;
        private float _overlapY = 0.2f;
        private RenderTexture _smoothnessMapTemp;

        private Vector4[] _splatKernel;

        private float _splatRandomize;
        private float _splatRotation;
        private float _splatRotationRandom = 0.25f;

        private float _splatScale = 1.0f;
        private RenderTexture _splatTemp;
        private RenderTexture _splatTempAlt;

        private float _splatWobble = 0.2f;
        private Vector2 _targetAr;

        private bool _techniqueOverlap = true;
        private bool _techniqueSplat;
        private float _texOffsetX;
        private float _texOffsetY;

        private GUIContent[] _texSizes;

        private float _texTiling = 1.0f;

        private Material _thisMaterial;

        private TileTechnique _tileTech = TileTechnique.Overlap;

        private RenderTexture _tileTemp;
        private int _windowId;

        private Rect _windowRect;

        public GameObject TestObject;

        public bool Hide { get; set; }

        private void Awake()
        {
            _windowRect = new Rect(10.0f, 265.0f, 300f, 540f);
        }

        private void Start()
        {
            _blitMaterial = new Material(Shader.Find("Hidden/Blit_Seamless_Texture_Maker"));

            _texSizes = new GUIContent[4];
            _texSizes[0] = new GUIContent("512");
            _texSizes[1] = new GUIContent("1024");
            _texSizes[2] = new GUIContent("2048");
            _texSizes[3] = new GUIContent("4096");

            _offsetKernel = new Vector2[9];
            _offsetKernel[0] = new Vector2(-1, -1);
            _offsetKernel[1] = new Vector2(-1, 0);
            _offsetKernel[2] = new Vector2(-1, 1);
            _offsetKernel[3] = new Vector2(0, -1);
            _offsetKernel[4] = new Vector2(0, 0);
            _offsetKernel[5] = new Vector2(0, 1);
            _offsetKernel[6] = new Vector2(1, -1);
            _offsetKernel[7] = new Vector2(1, 0);
            _offsetKernel[8] = new Vector2(1, 1);

            _windowId = ProgramManager.Instance.GetWindowId;
        }

        private void OnDisable()
        {
            CleanupTextures();
        }

        public void Initialize()
        {
            _thisMaterial = TextureManager.Instance.FullMaterialInstance;

            TestObject.GetComponent<Renderer>().material = _thisMaterial;
            _doStuff = true;
        }

        private void Update()
        {
            _thisMaterial.SetVector(Tiling, new Vector4(_texTiling, _texTiling, _texOffsetX, _texOffsetY));

            if (Math.Abs(_lastOverlapX - _overlapX) > 0.0001f)
            {
                _lastOverlapX = _overlapX;
                _doStuff = true;
            }

            if (Math.Abs(_lastOverlapY - _overlapY) > 0.0001f)
            {
                _lastOverlapY = _overlapY;
                _doStuff = true;
            }

            if (Math.Abs(_lastFalloff - _falloff) > 0.0001f)
            {
                _lastFalloff = _falloff;
                _doStuff = true;
            }

            if (_newTexSelectionX != _lastNewTexSelectionX)
            {
                _lastNewTexSelectionX = _newTexSelectionX;
                _doStuff = true;
            }

            if (_newTexSelectionY != _lastNewTexSelectionY)
            {
                _lastNewTexSelectionY = _newTexSelectionY;
                _doStuff = true;
            }

            if (_tileTech != _lastTileTech)
            {
                _lastTileTech = _tileTech;
                _doStuff = true;
            }

            if (!_doStuff) return;
            _doStuff = false;

            switch (_newTexSelectionX)
            {
                case 0:
                    _newTexSizeX = 512;
                    break;
                case 1:
                    _newTexSizeX = 1024;
                    break;
                case 2:
                    _newTexSizeX = 2048;
                    break;
                case 3:
                    _newTexSizeX = 4096;
                    break;
                default:
                    _newTexSizeX = 1024;
                    break;
            }

            switch (_newTexSelectionY)
            {
                case 0:
                    _newTexSizeY = 512;
                    break;
                case 1:
                    _newTexSizeY = 1024;
                    break;
                case 2:
                    _newTexSizeY = 2048;
                    break;
                case 3:
                    _newTexSizeY = 4096;
                    break;
                default:
                    _newTexSizeY = 1024;
                    break;
            }


            var aspect = _newTexSizeX / (float) _newTexSizeY;

            if (Mathf.Approximately(aspect, 8.0f))
                SkRectWide3();
            else if (Mathf.Approximately(aspect, 4.0f))
                SkRectWide2();
            else if (Mathf.Approximately(aspect, 2.0f))
                SkRectWide();
            else if (Mathf.Approximately(aspect, 1.0f))
                SkSquare();
            else if (Mathf.Approximately(aspect, 0.5f))
                SkRectTall();
            else if (Mathf.Approximately(aspect, 0.25f))
                SkRectTall2();
            else if (Mathf.Approximately(aspect, 0.125f)) SkRectTall3();


            const float area = 1.0f;
            _objectScale = Vector3.one;
            _objectScale.x = aspect;
            var newArea = _objectScale.x * _objectScale.y;
            var areaScale = Mathf.Sqrt(area / newArea);

            _objectScale.x *= areaScale;
            _objectScale.y *= areaScale;

            TestObject.transform.localScale = _objectScale;

            StartCoroutine(TileTextures());
        }

        private void SkSquare()
        {
            _splatKernel = new Vector4[4];
            _splatKernel[0] = new Vector4(0.0f, 0.25f, 0.8f, Random.value);
            _splatKernel[1] = new Vector4(0.5f, 0.25f, 0.8f, Random.value);
            _splatKernel[2] = new Vector4(0.25f, 0.75f, 0.8f, Random.value);
            _splatKernel[3] = new Vector4(0.75f, 0.75f, 0.8f, Random.value);
        }

        private void SkRectWide()
        {
            _splatKernel = new Vector4[6];
            _splatKernel[0] = new Vector4(0.0f, 0.25f, 0.5f, Random.value);
            _splatKernel[1] = new Vector4(0.333f, 0.25f, 0.5f, Random.value);
            _splatKernel[2] = new Vector4(0.666f, 0.25f, 0.5f, Random.value);

            _splatKernel[3] = new Vector4(0.166f, 0.75f, 0.5f, Random.value);
            _splatKernel[4] = new Vector4(0.5f, 0.75f, 0.5f, Random.value);
            _splatKernel[5] = new Vector4(0.833f, 0.75f, 0.5f, Random.value);
        }

        private void SkRectWide2()
        {
            _splatKernel = new Vector4[4];
            _splatKernel[0] = new Vector4(0.0f, 0.375f, 0.4f, Random.value);
            _splatKernel[1] = new Vector4(0.25f, 0.625f, 0.4f, Random.value);
            _splatKernel[2] = new Vector4(0.5f, 0.375f, 0.4f, Random.value);
            _splatKernel[3] = new Vector4(0.75f, 0.625f, 0.4f, Random.value);
        }

        private void SkRectWide3()
        {
            _splatKernel = new Vector4[8];
            _splatKernel[0] = new Vector4(0.0f, 0.375f, 0.25f, Random.value);
            _splatKernel[1] = new Vector4(0.125f, 0.625f, 0.25f, Random.value);
            _splatKernel[2] = new Vector4(0.25f, 0.375f, 0.25f, Random.value);
            _splatKernel[3] = new Vector4(0.375f, 0.625f, 0.25f, Random.value);
            _splatKernel[4] = new Vector4(0.5f, 0.375f, 0.25f, Random.value);
            _splatKernel[5] = new Vector4(0.625f, 0.625f, 0.25f, Random.value);
            _splatKernel[6] = new Vector4(0.75f, 0.375f, 0.25f, Random.value);
            _splatKernel[7] = new Vector4(0.875f, 0.625f, 0.25f, Random.value);
        }

        private void SkRectTall()
        {
            _splatKernel = new Vector4[6];
            _splatKernel[0] = new Vector4(0.25f, 0.0f, 0.5f, Random.value);
            _splatKernel[1] = new Vector4(0.25f, 0.333f, 0.5f, Random.value);
            _splatKernel[2] = new Vector4(0.25f, 0.666f, 0.5f, Random.value);

            _splatKernel[3] = new Vector4(0.75f, 0.166f, 0.5f, Random.value);
            _splatKernel[4] = new Vector4(0.75f, 0.5f, 0.5f, Random.value);
            _splatKernel[5] = new Vector4(0.75f, 0.833f, 0.5f, Random.value);
        }

        private void SkRectTall2()
        {
            _splatKernel = new Vector4[4];
            _splatKernel[0] = new Vector4(0.375f, 0.0f, 0.4f, Random.value);
            _splatKernel[1] = new Vector4(0.625f, 0.25f, 0.4f, Random.value);
            _splatKernel[2] = new Vector4(0.375f, 0.5f, 0.4f, Random.value);
            _splatKernel[3] = new Vector4(0.625f, 0.75f, 0.4f, Random.value);
        }

        private void SkRectTall3()
        {
            _splatKernel = new Vector4[8];
            _splatKernel[0] = new Vector4(0.375f, 0.0f, 0.25f, Random.value);
            _splatKernel[1] = new Vector4(0.625f, 0.125f, 0.25f, Random.value);
            _splatKernel[2] = new Vector4(0.375f, 0.25f, 0.25f, Random.value);
            _splatKernel[3] = new Vector4(0.625f, 0.375f, 0.25f, Random.value);
            _splatKernel[4] = new Vector4(0.375f, 0.5f, 0.25f, Random.value);
            _splatKernel[5] = new Vector4(0.625f, 0.625f, 0.25f, Random.value);
            _splatKernel[6] = new Vector4(0.375f, 0.75f, 0.25f, Random.value);
            _splatKernel[7] = new Vector4(0.625f, 0.875f, 0.25f, Random.value);
        }

        private void DoMyWindow(int windowId)
        {
            const int offsetX = 10;
            var offsetY = 30;

            _techniqueOverlap = GUI.Toggle(new Rect(offsetX, offsetY, 130, 30), _techniqueOverlap, "Technique Overlap");
            if (_techniqueOverlap)
            {
                _techniqueSplat = false;
                _tileTech = TileTechnique.Overlap;
            }
            else if (!_techniqueSplat)
            {
                _techniqueOverlap = true;
                _tileTech = TileTechnique.Overlap;
            }

            _techniqueSplat = GUI.Toggle(new Rect(offsetX + 150, offsetY, 130, 30), _techniqueSplat, "Technique Splat");
            if (_techniqueSplat)
            {
                _techniqueOverlap = false;
                _tileTech = TileTechnique.Splat;
            }
            else if (!_techniqueOverlap)
            {
                _techniqueSplat = true;
                _tileTech = TileTechnique.Splat;
            }

            offsetY += 40;

            GUI.Label(new Rect(offsetX, offsetY, 150, 20), "New Texture Size X");
            _newTexSelectionX =
                GUI.SelectionGrid(new Rect(offsetX, offsetY + 30, 120, 50), _newTexSelectionX, _texSizes, 2);

            GUI.Label(new Rect(offsetX + 150, offsetY, 150, 20), "New Texture Size Y");
            _newTexSelectionY =
                GUI.SelectionGrid(new Rect(offsetX + 150, offsetY + 30, 120, 50), _newTexSelectionY, _texSizes, 2);

            offsetY += 100;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Edge Falloff", _falloff, out _falloff,
                0.01f, 1.0f)) _doStuff = true;
            offsetY += 40;

            if (_techniqueOverlap)
            {
                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Overlap X", _overlapX,
                    out _overlapX, 0.00f, 1.0f)) _doStuff = true;
                offsetY += 40;

                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Overlap Y", _overlapY,
                    out _overlapY, 0.00f, 1.0f)) _doStuff = true;
                offsetY += 50;
            }

            if (_techniqueSplat)
            {
                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Splat Rotation", _splatRotation,
                    out _splatRotation, 0.0f, 1.0f)) _doStuff = true;
                offsetY += 40;

                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Splat Random Rotation", _splatRotationRandom,
                    out _splatRotationRandom, 0.0f, 1.0f))
                    _doStuff = true;
                offsetY += 40;

                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Splat Scale", _splatScale,
                    out _splatScale, 0.5f, 2.0f)) _doStuff = true;
                offsetY += 40;

                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Splat Wooble Amount", _splatWobble,
                    out _splatWobble, 0.0f, 1.0f)) _doStuff = true;
                offsetY += 40;

                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Splat Randomize", _splatRandomize,
                    out _splatRandomize, 0.0f, 1.0f)) _doStuff = true;
                offsetY += 50;
            }

            GUI.Label(new Rect(offsetX, offsetY, 150, 30), "Tiling Test Variables");
            offsetY += 30;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Tiling", _texTiling,
                out _texTiling, 0.1f, 5.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Offset X", _texOffsetX,
                out _texOffsetX, -1.0f, 1.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Offset Y", _texOffsetY,
                out _texOffsetY, -1.0f, 1.0f);
            offsetY += 40;

            if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Set Maps")) StartCoroutine(SetMaps());


            GUI.DragWindow();
        }

        private void OnGUI()
        {
            if (Hide) return;
            _windowRect.width = 300;
            _windowRect.height = _techniqueSplat ? 610 : 490;
            MainGui.MakeScaledWindow(_windowRect, _windowId, DoMyWindow, "Tiling Texture Maker");
        }

        // ReSharper disable once RedundantAssignment
        private Texture2D SetMap(Texture2D textureTarget, RenderTexture textureToSet)
        {
            RenderTexture.active = textureToSet;
            textureTarget = TextureManager.Instance.GetStandardTexture(_newTexSizeX, _newTexSizeY);
            textureTarget.ReadPixels(new Rect(0, 0, _newTexSizeX, _newTexSizeY), 0, 0);
            textureTarget.Apply(false);
            return textureTarget;
        }

        // ReSharper disable once RedundantAssignment
        private RenderTexture SetMapRt(RenderTexture textureTarget, Texture textureToSet)
        {
            textureTarget = RenderTexture.GetTemporary(_newTexSizeX, _newTexSizeY, 0, RenderTextureFormat.RHalf,
                RenderTextureReadWrite.Linear);
            Graphics.Blit(textureToSet, textureTarget);
            return textureTarget;
        }

        private IEnumerator SetMaps()
        {
            if (!TextureManager.Instance.HeightMap) yield break;

            if (TextureManager.Instance.DiffuseMap)
            {
                Logger.Log("Setting Diffuse");
                Destroy(TextureManager.Instance.DiffuseMap);
                TextureManager.Instance.DiffuseMap = null;
                TextureManager.Instance.DiffuseMap = SetMap(TextureManager.Instance.DiffuseMap, _diffuseMapTemp);
            }

            yield return new WaitForSeconds(0.1f);

            if (TextureManager.Instance.DiffuseMapOriginal)
            {
                Logger.Log("Setting Original Diffuse");
                Destroy(TextureManager.Instance.DiffuseMapOriginal);
                TextureManager.Instance.DiffuseMapOriginal = null;
                TextureManager.Instance.DiffuseMapOriginal =
                    SetMap(TextureManager.Instance.DiffuseMapOriginal, _diffuseMapOriginalTemp);
            }

            yield return new WaitForSeconds(0.1f);

            if (TextureManager.Instance.MetallicMap != null)
            {
                Logger.Log("Setting Specular");
                Destroy(TextureManager.Instance.MetallicMap);
                TextureManager.Instance.MetallicMap = null;
                TextureManager.Instance.MetallicMap = SetMap(TextureManager.Instance.MetallicMap, _metallicMapTemp);
            }

            yield return new WaitForSeconds(0.1f);

            if (TextureManager.Instance.SmoothnessMap != null)
            {
                Logger.Log("Setting Roughness");
                Destroy(TextureManager.Instance.SmoothnessMap);
                TextureManager.Instance.SmoothnessMap = null;
                TextureManager.Instance.SmoothnessMap =
                    SetMap(TextureManager.Instance.SmoothnessMap, _smoothnessMapTemp);
            }

            yield return new WaitForSeconds(0.1f);

            if (TextureManager.Instance.NormalMap)
            {
                Logger.Log("Setting Normal");
                Destroy(TextureManager.Instance.NormalMap);
                TextureManager.Instance.NormalMap = null;
                TextureManager.Instance.NormalMap = SetMap(TextureManager.Instance.NormalMap, _normalMapTemp);
            }

            yield return new WaitForSeconds(0.1f);

            if (TextureManager.Instance.AoMap)
            {
                Logger.Log("Setting AO");
                Destroy(TextureManager.Instance.AoMap);
                TextureManager.Instance.AoMap = null;
                TextureManager.Instance.AoMap = SetMap(TextureManager.Instance.AoMap, _aoMapTemp);
            }

            yield return new WaitForSeconds(0.1f);

            if (TextureManager.Instance.HeightMap)
            {
                Logger.Log("Setting Height");
                Destroy(TextureManager.Instance.HeightMap);
                TextureManager.Instance.HeightMap = null;
                TextureManager.Instance.HeightMap = SetMap(TextureManager.Instance.HeightMap, _heightMapTemp);
            }

            yield return new WaitForSeconds(0.1f);


            if (TextureManager.Instance.HdHeightMap)
            {
                Logger.Log("Setting Height");
                TextureManager.Instance.HdHeightMap.Release();
                TextureManager.Instance.HdHeightMap = null;
                TextureManager.Instance.HdHeightMap = SetMapRt(TextureManager.Instance.HdHeightMap, _hdHeightMapTemp);
            }

            yield return new WaitForSeconds(0.1f);
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
            CleanupTexture(_hdHeightMapTemp);
            CleanupTexture(_heightMapTemp);
            CleanupTexture(_diffuseMapTemp);
            CleanupTexture(_diffuseMapOriginalTemp);
            CleanupTexture(_metallicMapTemp);
            CleanupTexture(_smoothnessMapTemp);
            CleanupTexture(_normalMapTemp);
            CleanupTexture(_aoMapTemp);

            CleanupTexture(_tileTemp);
            CleanupTexture(_splatTemp);
            CleanupTexture(_splatTempAlt);
        }

        private RenderTexture TileTexture(Texture textureToTile, RenderTexture textureTarget, string texName)
        {
            CleanupTexture(_tileTemp);
            _tileTemp = TextureManager.Instance.GetTempRenderTexture(textureToTile.width, textureToTile.height);
            _blitMaterial.SetTexture(MainTex, textureToTile);
            Graphics.Blit(textureToTile, _tileTemp);

            return TileTexture(_tileTemp, textureTarget, texName);
        }

        private RenderTexture TileTexture(RenderTexture textureToTile, RenderTexture textureTarget, string texName)
        {
            switch (_tileTech)
            {
                case TileTechnique.Overlap:
                    return TileTextureOverlap(textureToTile, textureTarget, texName);
                case TileTechnique.Splat:
                    return TileTextureSplat(textureToTile, textureTarget, texName);
                default:
                    return TileTextureOverlap(textureToTile, textureTarget, texName);
            }
        }

        private RenderTexture TileTextureSplat(Texture textureToTile, RenderTexture textureTarget, string texName)
        {
            if (textureTarget)
            {
                textureTarget.Release();
                // ReSharper disable once RedundantAssignment
                textureTarget = null;
            }

            //Transform transHelper = new GameObject ().transform;

            CleanupTexture(_splatTemp);
            CleanupTexture(_splatTempAlt);

            if (texName == "_HDDisplacementMap")
            {
                _splatTemp = RenderTexture.GetTemporary(_newTexSizeX, _newTexSizeY, 0, RenderTextureFormat.ARGBHalf,
                    RenderTextureReadWrite.Linear);
                _splatTempAlt = RenderTexture.GetTemporary(_newTexSizeX, _newTexSizeY, 0, RenderTextureFormat.ARGBHalf,
                    RenderTextureReadWrite.Linear);
                textureTarget = RenderTexture.GetTemporary(_newTexSizeX, _newTexSizeY, 0, RenderTextureFormat.ARGBHalf,
                    RenderTextureReadWrite.Linear);
            }
            else
            {
                _splatTemp = RenderTexture.GetTemporary(_newTexSizeX, _newTexSizeY, 0, RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.Linear);
                _splatTempAlt = RenderTexture.GetTemporary(_newTexSizeX, _newTexSizeY, 0, RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.Linear);
                textureTarget = RenderTexture.GetTemporary(_newTexSizeX, _newTexSizeY, 0, RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.Linear);
            }

            textureTarget.wrapMode = TextureWrapMode.Repeat;

            _blitMaterial.SetTexture(MainTex, textureToTile);
            _blitMaterial.SetTexture(HeightTex, TextureManager.Instance.HeightMap);
            _blitMaterial.SetVector(ObjectScale, _objectScale);

            _blitMaterial.SetFloat(FlipY, SettingsGui.Instance.ProgramSettings.NormalMapMayaStyle ? 1.0f : 0.0f);

            // Clear the ping pong buffers
            Graphics.Blit(Texture2D.blackTexture, _splatTemp, _blitMaterial, 2);
            Graphics.Blit(Texture2D.blackTexture, _splatTempAlt, _blitMaterial, 2);

            var texArWidth = TextureManager.Instance.HeightMap.width / (float) TextureManager.Instance.HeightMap.height;
            var texArHeight = TextureManager.Instance.HeightMap.height /
                              (float) TextureManager.Instance.HeightMap.width;
            var texAr = Vector2.one;
            if (texArWidth < texArHeight)
                texAr.x = texArWidth;
            else
                texAr.y = texArHeight;

            var targetArWidth = _newTexSizeX / (float) _newTexSizeY;
            var targetArHeight = _newTexSizeY / (float) _newTexSizeX;
            _targetAr = Vector2.one;
            if (targetArWidth < targetArHeight)
                _targetAr.x = targetArWidth;
            else
                _targetAr.y = targetArHeight;

            _blitMaterial.SetFloat(SplatScale, _splatScale);
            _blitMaterial.SetVector(AspectRatio, texAr);
            _blitMaterial.SetVector(TargetAspectRatio, _targetAr);

            _blitMaterial.SetFloat(SplatRotation, _splatRotation);
            _blitMaterial.SetFloat(SplatRotationRandom, _splatRotationRandom);

            var isEven = true;
            for (var i = 0; i < _splatKernel.Length; i++)
            {
                _blitMaterial.SetVector(SplatKernel, _splatKernel[i]);

                var offsetX = Mathf.Sin((_splatRandomize + 1.0f + i) * 128.352f);
                var offsetY = Mathf.Cos((_splatRandomize + 1.0f + i) * 243.767f);
                _blitMaterial.SetVector(Wobble, new Vector3(offsetX, offsetY, _splatWobble));

                _blitMaterial.SetFloat(SplatRandomize, Mathf.Sin((_splatRandomize + 1.0f + i) * 472.361f));

                if (isEven)
                {
                    _blitMaterial.SetTexture(TargetTex, _splatTempAlt);
                    Graphics.Blit(textureToTile, _splatTemp, _blitMaterial, 1);
                    isEven = false;
                }
                else
                {
                    _blitMaterial.SetTexture(TargetTex, _splatTemp);
                    Graphics.Blit(textureToTile, _splatTempAlt, _blitMaterial, 1);
                    isEven = true;
                }
            }

            //GameObject.Destroy(transHelper.gameObject);

            Graphics.Blit(isEven ? _splatTempAlt : _splatTemp, textureTarget, _blitMaterial, 3);

            _thisMaterial.SetTexture(texName, textureTarget);

            CleanupTexture(_splatTemp);
            CleanupTexture(_splatTempAlt);

            return textureTarget;
        }


        private RenderTexture TileTextureOverlap(Texture textureToTile, RenderTexture textureTarget, string texName)
        {
            if (textureTarget)
            {
                textureTarget.Release();
                // ReSharper disable once RedundantAssignment
                textureTarget = null;
            }

            textureTarget = RenderTexture.GetTemporary(_newTexSizeX, _newTexSizeY, 0,
                texName == "_HDDisplacementMap" ? RenderTextureFormat.RHalf : RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear);

            textureTarget.wrapMode = TextureWrapMode.Repeat;

            _blitMaterial.SetTexture(MainTex, textureToTile);

            Graphics.Blit(textureToTile, textureTarget, _blitMaterial, 0);

            _thisMaterial.SetTexture(texName, textureTarget);

            return textureTarget;
        }

        private IEnumerator TileTextures()
        {
            Logger.Log("Processing Tile");

            _blitMaterial.SetFloat(Falloff, 1.0f);
            _blitMaterial.SetFloat(Falloff, _falloff);
            _blitMaterial.SetFloat(OverlapX, _overlapX);
            _blitMaterial.SetFloat(OverlapY, _overlapY);

            if (TextureManager.Instance.HeightMap == null) yield break;

            _blitMaterial.SetTexture(HeightTex, TextureManager.Instance.HeightMap);
            _blitMaterial.SetFloat(IsHeight, 1.0f);
            _heightMapTemp = TileTexture(TextureManager.Instance.HeightMap, _heightMapTemp, "_DisplacementMap");


            if (TextureManager.Instance.HdHeightMap != null)
            {
                _blitMaterial.SetFloat(IsHeight, 1.0f);
                _hdHeightMapTemp = TileTexture(TextureManager.Instance.HdHeightMap, _hdHeightMapTemp,
                    "_HDDisplacementMap");
            }


            _blitMaterial.SetFloat(IsHeight, 0.0f);

            if (TextureManager.Instance.DiffuseMapOriginal != null)
            {
                _diffuseMapOriginalTemp =
                    TileTexture(TextureManager.Instance.DiffuseMapOriginal, _diffuseMapOriginalTemp,
                        "_DiffuseMapOriginal");
                _thisMaterial.SetTexture(DiffuseMap, _diffuseMapOriginalTemp);
            }

            if (TextureManager.Instance.DiffuseMap != null)
            {
                _diffuseMapTemp = TileTexture(TextureManager.Instance.DiffuseMap, _diffuseMapTemp, "_DiffuseMap");
                _thisMaterial.SetTexture(DiffuseMap, _diffuseMapTemp);
            }

            if (TextureManager.Instance.MetallicMap != null)
            {
                _metallicMapTemp = TileTexture(TextureManager.Instance.MetallicMap, _metallicMapTemp, "_MetallicMap");
                _thisMaterial.SetTexture(MetallicMap, _metallicMapTemp);
            }

            if (TextureManager.Instance.SmoothnessMap != null)
            {
                _smoothnessMapTemp = TileTexture(TextureManager.Instance.SmoothnessMap, _smoothnessMapTemp,
                    "_SmoothnessMap");
                _thisMaterial.SetTexture(SmoothnessMap, _smoothnessMapTemp);
            }

            if (TextureManager.Instance.NormalMap != null)
            {
                _blitMaterial.SetFloat(IsNormal, 1.0f);
                _normalMapTemp = TileTexture(TextureManager.Instance.NormalMap, _normalMapTemp, "_NormalMap");
                _thisMaterial.SetTexture(NormalMap, _normalMapTemp);
            }

            _blitMaterial.SetFloat(IsNormal, 0.0f);

            if (TextureManager.Instance.AoMap != null)
            {
                _aoMapTemp = TileTexture(TextureManager.Instance.AoMap, _aoMapTemp, "_AOMap");
                _thisMaterial.SetTexture(AoMap, _aoMapTemp);
            }

            yield return new WaitForSeconds(0.1f);
        }

        private enum TileTechnique
        {
            Overlap,
            Splat
        }
    }
}