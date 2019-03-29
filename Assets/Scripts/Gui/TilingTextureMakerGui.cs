#region

using System;
using System.Collections;
using General;
using Settings;
using UnityEngine;
using Logger = General.Logger;
using Random = UnityEngine.Random;

#endregion

namespace Gui
{
    public class TilingTextureMakerGui : TexturePanelGui
    {
        private RenderTexture _aoMapTemp;

        private RenderTexture _diffuseMapOriginalTemp;
        private RenderTexture _diffuseMapTemp;

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
        public ComputeShader TilingCompute;
        private int _tileKernel;
        private int _splatComputeKernel;
        private bool _busy;

        private void Awake()
        {
            WindowRect = new Rect(10.0f, 265.0f, 300f, 540f);
            _tileKernel = TilingCompute.FindKernel("CSTile");
            _splatComputeKernel = TilingCompute.FindKernel("CSSplat");
        }

        private void Start()
        {
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

            WindowId = ProgramManager.Instance.GetWindowId;
        }


        public void Initialize()
        {
            _thisMaterial = TextureManager.Instance.GetMaterialInstance();

            TestObject.GetComponent<Renderer>().material = _thisMaterial;
            StuffToBeDone = true;
        }

        private void Update()
        {
            _thisMaterial.SetTextureScale(DiffuseMap, new Vector2(_texTiling, _texTiling));
            _thisMaterial.SetTextureOffset(DiffuseMap, new Vector2(_texOffsetX, _texOffsetY));

            if (Math.Abs(_lastOverlapX - _overlapX) > 0.0001f)
            {
                _lastOverlapX = _overlapX;
                StuffToBeDone = true;
            }

            if (Math.Abs(_lastOverlapY - _overlapY) > 0.0001f)
            {
                _lastOverlapY = _overlapY;
                StuffToBeDone = true;
            }

            if (Math.Abs(_lastFalloff - _falloff) > 0.0001f)
            {
                _lastFalloff = _falloff;
                StuffToBeDone = true;
            }

            if (_newTexSelectionX != _lastNewTexSelectionX)
            {
                _lastNewTexSelectionX = _newTexSelectionX;
                StuffToBeDone = true;
            }

            if (_newTexSelectionY != _lastNewTexSelectionY)
            {
                _lastNewTexSelectionY = _newTexSelectionY;
                StuffToBeDone = true;
            }

            if (_tileTech != _lastTileTech)
            {
                _lastTileTech = _tileTech;
                StuffToBeDone = true;
            }

            if (!StuffToBeDone) return;
            StuffToBeDone = false;

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

            StartCoroutine(StartProcessing());
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
                0.01f, 1.0f)) StuffToBeDone = true;
            offsetY += 40;

            if (_techniqueOverlap)
            {
                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Overlap X", _overlapX,
                    out _overlapX, 0.00f, 1.0f)) StuffToBeDone = true;
                offsetY += 40;

                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Overlap Y", _overlapY,
                    out _overlapY, 0.00f, 1.0f)) StuffToBeDone = true;
                offsetY += 50;
            }

            if (_techniqueSplat)
            {
                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Splat Rotation", _splatRotation,
                    out _splatRotation, 0.0f, 1.0f)) StuffToBeDone = true;
                offsetY += 40;

                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Splat Random Rotation", _splatRotationRandom,
                    out _splatRotationRandom, 0.0f, 1.0f))
                    StuffToBeDone = true;
                offsetY += 40;

                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Splat Scale", _splatScale,
                    out _splatScale, 0.5f, 2.0f)) StuffToBeDone = true;
                offsetY += 40;

                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Splat Wooble Amount", _splatWobble,
                    out _splatWobble, 0.0f, 1.0f)) StuffToBeDone = true;
                offsetY += 40;

                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Splat Randomize", _splatRandomize,
                    out _splatRandomize, 0.0f, 1.0f)) StuffToBeDone = true;
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
        }

        private void OnGUI()
        {
            if (Hide) return;
            var rect = WindowRect;
            rect.height = _techniqueSplat ? 610 : 490;
            MainGui.MakeScaledWindow(rect, WindowId, DoMyWindow, "Tiling Texture Maker");
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

        private static void CleanupTexture(RenderTexture texture)
        {
            if (!texture) return;
            texture.Release();
            // ReSharper disable once RedundantAssignment
            texture = null;
        }

        protected override void CleanupTextures()
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

        private RenderTexture TileTexture(Texture textureToTile, RenderTexture textureTarget)
        {
            CleanupTexture(_tileTemp);
            _tileTemp = TextureManager.Instance.GetTempRenderTexture(textureToTile.width, textureToTile.height);
            ImageSize = new Vector2Int(textureToTile.width, textureToTile.height);
            Graphics.Blit(textureToTile, _tileTemp);

            return TileTexture(_tileTemp, textureTarget);
        }

        private RenderTexture TileTexture(RenderTexture textureToTile, RenderTexture textureTarget)
        {
            switch (_tileTech)
            {
                case TileTechnique.Overlap:
                    return TileTextureOverlap(textureToTile, textureTarget);
                case TileTechnique.Splat:
                    return TileTextureSplat(textureToTile, textureTarget);
                default:
                    return TileTextureOverlap(textureToTile, textureTarget);
            }
        }

        private RenderTexture TileTextureSplat(Texture textureToTile, RenderTexture textureTarget)
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

            _splatTemp = TextureManager.Instance.GetTempRenderTexture(_newTexSizeX, _newTexSizeY);
            _splatTempAlt = TextureManager.Instance.GetTempRenderTexture(_newTexSizeX, _newTexSizeY);
            textureTarget = TextureManager.Instance.GetTempRenderTexture(_newTexSizeX, _newTexSizeY);

            TilingCompute.SetTexture(_tileKernel, HeightTex, TextureManager.Instance.HeightMap);
            TilingCompute.SetTexture(_splatComputeKernel, HeightTex, TextureManager.Instance.HeightMap);

            TilingCompute.SetVector(ObjectScale, _objectScale);

            TilingCompute.SetFloat(FlipY, SettingsGui.Instance.ProgramSettings.NormalMapMayaStyle ? 1.0f : 0.0f);

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

            TilingCompute.SetFloat(SplatScale, _splatScale);
            TilingCompute.SetVector(AspectRatio, texAr);
            TilingCompute.SetVector(TargetAspectRatio, _targetAr);

            TilingCompute.SetFloat(SplatRotation, _splatRotation);
            TilingCompute.SetFloat(SplatRotationRandom, _splatRotationRandom);

            var isEven = true;
            for (var i = 0; i < _splatKernel.Length; i++)
            {
                TilingCompute.SetVector(SplatKernel, _splatKernel[i]);

                var offsetX = Mathf.Sin((_splatRandomize + 1.0f + i) * 128.352f);
                var offsetY = Mathf.Cos((_splatRandomize + 1.0f + i) * 243.767f);
                TilingCompute.SetVector(Wobble, new Vector3(offsetX, offsetY, _splatWobble));

                TilingCompute.SetFloat(SplatRandomize, Mathf.Sin((_splatRandomize + 1.0f + i) * 472.361f));

                ImageSize = new Vector2Int(textureToTile.width, textureToTile.height);
                TilingCompute.SetVector(ImageSizeId, (Vector2) ImageSize);
                if (isEven)
                {
                    TilingCompute.SetTexture(_splatComputeKernel, TargetTex, _splatTempAlt);
                    RunKernel(TilingCompute, _splatComputeKernel, textureToTile, _splatTemp);
                    isEven = false;
                }
                else
                {
                    TilingCompute.SetTexture(_splatComputeKernel, TargetTex, _splatTemp);
                    RunKernel(TilingCompute, _splatComputeKernel, textureToTile, _splatTempAlt);
                    isEven = true;
                }
            }

            //GameObject.Destroy(transHelper.gameObject);

            Graphics.CopyTexture(isEven ? _splatTempAlt : _splatTemp, 0, 0, textureTarget, 0, 0);

            CleanupTexture(_splatTemp);
            CleanupTexture(_splatTempAlt);

            return textureTarget;
        }


        private RenderTexture TileTextureOverlap(Texture textureToTile, RenderTexture textureTarget)
        {
            if (textureTarget)
            {
                textureTarget.Release();
                // ReSharper disable once RedundantAssignment
                textureTarget = null;
            }

            textureTarget = TextureManager.Instance.GetTempRenderTexture(_newTexSizeX, _newTexSizeY);

            ImageSize = new Vector2Int(textureToTile.width, textureToTile.height);
            TilingCompute.SetVector(ImageSizeId, (Vector2) ImageSize);

            RunKernel(TilingCompute, _tileKernel, textureToTile, textureTarget);

            return textureTarget;
        }

        protected override IEnumerator Process()
        {
            if (_busy) yield break;

            _busy = true;
            MessagePanel.ShowMessage("Processing Tile");

            TilingCompute.SetFloat(Falloff, 1.0f);
            TilingCompute.SetFloat(Falloff, _falloff);
            TilingCompute.SetFloat(OverlapX, _overlapX);
            TilingCompute.SetFloat(OverlapY, _overlapY);

            if (TextureManager.Instance.HeightMap == null) yield break;

            TilingCompute.SetTexture(_tileKernel, HeightTex, TextureManager.Instance.HeightMap);
            TilingCompute.SetTexture(_splatComputeKernel, HeightTex, TextureManager.Instance.HeightMap);
            TilingCompute.SetFloat(IsHeight, 1.0f);

            _heightMapTemp = TileTexture(TextureManager.Instance.HeightMap, _heightMapTemp);
            _thisMaterial.SetTexture(HeightMapId, _heightMapTemp);

            if (TextureManager.Instance.HdHeightMap != null)
            {
                TilingCompute.SetFloat(IsHeight, 1.0f);
                _hdHeightMapTemp = TileTexture(TextureManager.Instance.HdHeightMap, _hdHeightMapTemp);
            }


            TilingCompute.SetFloat(IsHeight, 0.0f);

            if (TextureManager.Instance.DiffuseMapOriginal != null)
            {
                _diffuseMapOriginalTemp =
                    TileTexture(TextureManager.Instance.DiffuseMapOriginal, _diffuseMapOriginalTemp);
                _thisMaterial.SetTexture(DiffuseMap, _diffuseMapOriginalTemp);
            }

            if (TextureManager.Instance.DiffuseMap != null)
            {
                _diffuseMapTemp = TileTexture(TextureManager.Instance.DiffuseMap, _diffuseMapTemp);
                _thisMaterial.SetTexture(DiffuseMap, _diffuseMapTemp);
            }

            if (TextureManager.Instance.MetallicMap != null)
            {
                _metallicMapTemp = TileTexture(TextureManager.Instance.MetallicMap, _metallicMapTemp);
                _thisMaterial.SetTexture(MetallicMap, _metallicMapTemp);
            }

            if (TextureManager.Instance.SmoothnessMap != null)
            {
                _smoothnessMapTemp = TileTexture(TextureManager.Instance.SmoothnessMap, _smoothnessMapTemp);
                _thisMaterial.SetTexture(SmoothnessMap, _smoothnessMapTemp);
            }

            if (TextureManager.Instance.NormalMap != null)
            {
                TilingCompute.SetFloat(IsNormal, 1.0f);
                _normalMapTemp = TileTexture(TextureManager.Instance.NormalMap, _normalMapTemp);
                _thisMaterial.SetTexture(NormalMap, _normalMapTemp);
            }

            TilingCompute.SetFloat(IsNormal, 0.0f);

            if (TextureManager.Instance.AoMap != null)
            {
                _aoMapTemp = TileTexture(TextureManager.Instance.AoMap, _aoMapTemp);
                _thisMaterial.SetTexture(AoMap, _aoMapTemp);
            }

            yield return new WaitForSeconds(0.1f);

            _busy = false;
        }

        protected override void ResetSettings()
        {
            throw new NotImplementedException();
        }

        protected override TexturePanelSettings GetSettings()
        {
            throw new NotImplementedException();
        }

        protected override void SetSettings(TexturePanelSettings settings)
        {
            throw new NotImplementedException();
        }

        private enum TileTechnique
        {
            Overlap,
            Splat
        }
    }
}