#region

using System;
using System.Collections;
using Materialize.General;
using Materialize.Settings;
using UnityEngine;
using Utility;
using Logger = Utility.Logger;

#endregion

namespace Materialize.Gui
{
    public class AlignmentGui : TexturePanelGui
    {
        private int _alignKernel;
        private RenderTexture _alignMap;

        private Camera _camera;

        private int _grabbedPoint;

        private float _lensDistort;
        private int _lensKernel;

        private RenderTexture _lensMap;
        private int _perspectiveKernel;

        private RenderTexture _perspectiveMap;

        private float _perspectiveX;

        private float _perspectiveY;
        private Vector2 _pointBl = new Vector2(0.0f, 0.0f);
        private Vector2 _pointBr = new Vector2(1.0f, 0.0f);

        private Vector2 _pointTl = new Vector2(0.0f, 1.0f);
        private Vector2 _pointTr = new Vector2(1.0f, 1.0f);

        private float _slider = 0.5f;
        private Vector2 _startOffset = Vector2.zero;

        private Texture2D _textureToAlign;
        public ComputeShader AlignmentCompute;

        private void Awake()
        {
            _camera = Camera.main;
            WindowRect = new Rect(10.0f, 265.0f, 300f, 430f);
            _lensKernel = AlignmentCompute.FindKernel("CSLens");
            _alignKernel = AlignmentCompute.FindKernel("CSAlign");
            _perspectiveKernel = AlignmentCompute.FindKernel("CSPerspective");
        }

        public void Initialize()
        {
            gameObject.SetActive(true);
            TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

            if (TextureManager.Instance.DiffuseMapOriginal != null)
                _textureToAlign = TextureManager.Instance.DiffuseMapOriginal;
            else if (TextureManager.Instance.HeightMap != null)
                _textureToAlign = TextureManager.Instance.HeightMap;
            else if (TextureManager.Instance.MetallicMap != null)
                _textureToAlign = TextureManager.Instance.MetallicMap;
            else if (TextureManager.Instance.SmoothnessMap != null)
                _textureToAlign = TextureManager.Instance.SmoothnessMap;
            else if (TextureManager.Instance.MaskMap != null)
                _textureToAlign = TextureManager.Instance.MaskMap;
            else if (TextureManager.Instance.AoMap != null) _textureToAlign = TextureManager.Instance.AoMap;
            else
                gameObject.SetActive(false);


            StuffToBeDone = true;
        }

        protected override void CleanupTextures()
        {
            RenderTexture.ReleaseTemporary(_alignMap);
            RenderTexture.ReleaseTemporary(_lensMap);
            RenderTexture.ReleaseTemporary(_perspectiveMap);
        }

        protected override IEnumerator Process()
        {
            throw new NotImplementedException();
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

        private void SelectClosestPoint()
        {
            if (Input.GetMouseButton(0)) return;
            if (!_camera) return;
            const int mask = 1 << 11;
            var wasHit = Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit,
                Mathf.Infinity, mask, QueryTriggerInteraction.UseGlobal);

            if (!wasHit) return;

            var rend = hit.transform.GetComponent<Renderer>();
            var hasMeshCollider = hit.collider is MeshCollider;
            if (!rend || !rend.sharedMaterial || !rend.sharedMaterial.mainTexture || !hasMeshCollider) return;

            var hitTc = hit.textureCoord;

            var dist1 = Vector2.Distance(hitTc, _pointTl);
            var dist2 = Vector2.Distance(hitTc, _pointTr);
            var dist3 = Vector2.Distance(hitTc, _pointBl);
            var dist4 = Vector2.Distance(hitTc, _pointBr);

            var closestDist = dist1;
            var closestPoint = _pointTl;
            _grabbedPoint = 0;
            if (dist2 < closestDist)
            {
                closestDist = dist2;
                closestPoint = _pointTr;
                _grabbedPoint = 1;
            }

            if (dist3 < closestDist)
            {
                closestDist = dist3;
                closestPoint = _pointBl;
                _grabbedPoint = 2;
            }

            if (dist4 < closestDist)
            {
                closestDist = dist4;
                closestPoint = _pointBr;
                _grabbedPoint = 3;
            }

            if (closestDist > 0.1f)
            {
                closestPoint = new Vector2(-1, -1);
                _grabbedPoint = -1;
            }

            ThisMaterial.SetVector(TargetPoint, closestPoint);
        }

        private void DragPoint()
        {
            if (!_camera) return;
            const int mask = 1 << 11;
            var wasHit = Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit,
                Mathf.Infinity, mask, QueryTriggerInteraction.UseGlobal);

            if (!wasHit) return;

            var rend = hit.transform.GetComponent<Renderer>();
            var hasMeshCollider = hit.collider is MeshCollider;
            if (!rend || !rend.sharedMaterial || !rend.sharedMaterial.mainTexture || !hasMeshCollider) return;

            var hitTc = hit.textureCoord;

            if (Input.GetMouseButtonDown(0))
            {
                _startOffset = hitTc;
            }
            else if (Input.GetMouseButton(0))
            {
                Vector2 point;
                switch (_grabbedPoint)
                {
                    case 0:
                        _pointTl += hitTc - _startOffset;
                        point = _pointTl;
                        break;
                    case 1:
                        _pointTr += hitTc - _startOffset;
                        point = _pointTr;
                        break;
                    case 2:
                        _pointBl += hitTc - _startOffset;
                        point = _pointBl;
                        break;
                    case 3:
                        _pointBr += hitTc - _startOffset;
                        point = _pointBr;

                        break;
                    default: return;
                }

                ClampPoints();

                if (point != null) ThisMaterial.SetVector(TargetPoint, point);

                _startOffset = hitTc;
            }

            StuffToBeDone = true;
        }

        private void ClampPoints()
        {
            _pointTl.x = Mathf.Clamp01(_pointTl.x);
            _pointTl.y = Mathf.Clamp01(_pointTl.y);

            _pointTr.x = Mathf.Clamp01(_pointTr.x);
            _pointTr.y = Mathf.Clamp01(_pointTr.y);

            _pointBl.x = Mathf.Clamp01(_pointBl.x);
            _pointBl.y = Mathf.Clamp01(_pointBl.y);

            _pointBr.x = Mathf.Clamp01(_pointBr.x);
            _pointBr.y = Mathf.Clamp01(_pointBr.y);
        }

        // Update is called once per frame
        private void Update()
        {
            SelectClosestPoint();
            DragPoint();

            ProcessMap(_textureToAlign);

            var aspect = _textureToAlign.width / (float) _textureToAlign.height;
            const float area = 1.0f;
            var pointScale = Vector2.one;
            pointScale.x = aspect;
            var newArea = pointScale.x * pointScale.y;
            var areaScale = Mathf.Sqrt(area / newArea);

            pointScale.x *= areaScale;
            pointScale.y *= areaScale;

            ThisMaterial.SetTexture(MainTex, _lensMap);
            ThisMaterial.SetTexture(CorrectTex, _perspectiveMap);

            ThisMaterial.SetVector(PointScale, pointScale);

            ThisMaterial.SetVector(PointTl, _pointTl);
            ThisMaterial.SetVector(PointTr, _pointTr);
            ThisMaterial.SetVector(PointBl, _pointBl);
            ThisMaterial.SetVector(PointBr, _pointBr);

            var imageSize = new Vector2(_textureToAlign.width, _textureToAlign.height);

            AlignmentCompute.SetVector(ImageSizeId, imageSize);

            AlignmentCompute.SetVector(PointTl, _pointTl);
            AlignmentCompute.SetVector(PointTr, _pointTr);
            AlignmentCompute.SetVector(PointBl, _pointBl);
            AlignmentCompute.SetVector(PointBr, _pointBr);

            AlignmentCompute.SetFloat(Lens, _lensDistort);
            AlignmentCompute.SetFloat(PerspectiveX, _perspectiveX);
            AlignmentCompute.SetFloat(PerspectiveY, _perspectiveY);

            if (StuffToBeDone) StuffToBeDone = false;

            ThisMaterial.SetFloat(Slider, _slider);
        }

        private void DoMyWindow(int windowId)
        {
            const int offsetX = 10;
            var offsetY = 30;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Alignment Reveal Slider");
            _slider = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 20, 280, 10), _slider, 0.0f, 1.0f);
            offsetY += 40;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Preview Map");
            offsetY += 30;

            GUI.enabled = TextureManager.Instance.DiffuseMapOriginal != null;
            if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Original Diffuse Map"))
            {
                _textureToAlign = TextureManager.Instance.DiffuseMapOriginal;
                StuffToBeDone = true;
            }

            GUI.enabled = TextureManager.Instance.DiffuseMap != null;
            if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Diffuse Map"))
            {
                _textureToAlign = TextureManager.Instance.DiffuseMap;
                StuffToBeDone = true;
            }

            offsetY += 40;


            GUI.enabled = TextureManager.Instance.HeightMap != null;
            if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Height Map"))
            {
                _textureToAlign = TextureManager.Instance.HeightMap;
                StuffToBeDone = true;
            }

            offsetY += 40;

            GUI.enabled = TextureManager.Instance.MetallicMap != null;
            if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Metallic Map"))
            {
                _textureToAlign = TextureManager.Instance.MetallicMap;
                StuffToBeDone = true;
            }

            GUI.enabled = TextureManager.Instance.SmoothnessMap != null;
            if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Smoothness Map"))
            {
                _textureToAlign = TextureManager.Instance.SmoothnessMap;
                StuffToBeDone = true;
            }

            offsetY += 40;

            GUI.enabled = TextureManager.Instance.MaskMap != null;
            if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Mask Map"))
            {
                _textureToAlign = TextureManager.Instance.MaskMap;
                StuffToBeDone = true;
            }

            GUI.enabled = TextureManager.Instance.AoMap != null;
            if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "AO Map"))
            {
                _textureToAlign = TextureManager.Instance.AoMap;
                StuffToBeDone = true;
            }

            offsetY += 40;

            GUI.enabled = true;


            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Lens Distort Correction", _lensDistort,
                out _lensDistort, -1.0f, 1.0f)) StuffToBeDone = true;
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Perspective Correction X", _perspectiveX,
                out _perspectiveX, -5.0f, 5.0f)) StuffToBeDone = true;
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Perspective Correction Y", _perspectiveY,
                out _perspectiveY, -5.0f, 5.0f)) StuffToBeDone = true;
            offsetY += 50;

            if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Reset Points"))
            {
                _pointTl = new Vector2(0.0f, 1.0f);
                _pointTr = new Vector2(1.0f, 1.0f);
                _pointBl = new Vector2(0.0f, 0.0f);
                _pointBr = new Vector2(1.0f, 0.0f);
            }


            if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Set All Maps")) StartCoroutine(SetMaps());


            GUI.DragWindow();
        }

        private void OnGUI()
        {
            if (Hide) return;

            MainGui.MakeScaledWindow(WindowRect, WindowId, DoMyWindow, "Texture Alignment Adjuster", GuiScale);
        }

        private void ProcessMap(Texture2D textureTarget)
        {
            var width = textureTarget.width;
            var height = textureTarget.height;

            if (_lensMap == null)
                _lensMap = TextureManager.Instance.GetTempRenderTexture(width, height);
            if (_alignMap == null)
                _alignMap = TextureManager.Instance.GetTempRenderTexture(width, height);
            if (_perspectiveMap == null)
                _perspectiveMap = TextureManager.Instance.GetTempRenderTexture(width, height);

            ImageSize = new Vector2Int(_textureToAlign.width, _textureToAlign.height);

            AlignmentCompute.SetVector(ImageSizeId, (Vector2) ImageSize);
            RunKernel(AlignmentCompute, _lensKernel, textureTarget, _lensMap);
            RunKernel(AlignmentCompute, _alignKernel, _lensMap, _alignMap);
            RunKernel(AlignmentCompute, _perspectiveKernel, _alignMap, _perspectiveMap);
        }

        private Texture2D SetMap(Texture2D textureTarget)
        {
            var width = textureTarget.width;
            var height = textureTarget.height;

            RenderTexture.ReleaseTemporary(_lensMap);
            RenderTexture.ReleaseTemporary(_alignMap);
            RenderTexture.ReleaseTemporary(_perspectiveMap);

            _lensMap = TextureManager.Instance.GetTempRenderTexture(width, height);
            _alignMap = TextureManager.Instance.GetTempRenderTexture(width, height);
            _perspectiveMap = TextureManager.Instance.GetTempRenderTexture(width, height);

            ImageSize = new Vector2Int(_textureToAlign.width, _textureToAlign.height);

            AlignmentCompute.SetVector(ImageSizeId, (Vector2) ImageSize);

            RunKernel(AlignmentCompute, _lensKernel, textureTarget, _lensMap);
            RunKernel(AlignmentCompute, _alignKernel, _lensMap, _alignMap);
            RunKernel(AlignmentCompute, _perspectiveKernel, _alignMap, _perspectiveMap);

            var replaceTexture = _textureToAlign == textureTarget;

            Destroy(textureTarget);
            // ReSharper disable once RedundantAssignment
            textureTarget = null;

            RenderTexture.active = _perspectiveMap;
            textureTarget = TextureManager.Instance.GetStandardTexture(width, height);
            textureTarget.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            textureTarget.Apply(false);

            RenderTexture.active = null;

            RenderTexture.ReleaseTemporary(_lensMap);
            RenderTexture.ReleaseTemporary(_alignMap);
            RenderTexture.ReleaseTemporary(_perspectiveMap);

            if (replaceTexture) _textureToAlign = textureTarget;

            StuffToBeDone = true;

            return textureTarget;
        }

        private RenderTexture SetMap(RenderTexture textureTarget)
        {
            var width = textureTarget.width;
            var height = textureTarget.height;

            RenderTexture.ReleaseTemporary(_lensMap);
            RenderTexture.ReleaseTemporary(_alignMap);
            RenderTexture.ReleaseTemporary(_perspectiveMap);

            _lensMap = TextureManager.Instance.GetTempRenderTexture(width, height);
            _alignMap = TextureManager.Instance.GetTempRenderTexture(width, height);
            _perspectiveMap = TextureManager.Instance.GetTempRenderTexture(width, height);

            ImageSize = new Vector2Int(_textureToAlign.width, _textureToAlign.height);

            AlignmentCompute.SetVector(ImageSizeId, (Vector2) ImageSize);

            RunKernel(AlignmentCompute, _lensKernel, textureTarget, _lensMap);
            RunKernel(AlignmentCompute, _alignKernel, _lensMap, _alignMap);
            RunKernel(AlignmentCompute, _perspectiveKernel, _alignMap, _perspectiveMap);

            if (textureTarget)
            {
                textureTarget.Release();
                textureTarget = null;
            }

            Graphics.Blit(_perspectiveMap, textureTarget);

            RenderTexture.ReleaseTemporary(_lensMap);
            RenderTexture.ReleaseTemporary(_alignMap);
            RenderTexture.ReleaseTemporary(_perspectiveMap);

            StuffToBeDone = true;

            return textureTarget;
        }

        private IEnumerator SetMaps()
        {
            if (TextureManager.Instance.HeightMap != null)
            {
                Logger.Log("Setting Height");
                TextureManager.Instance.HeightMap = SetMap(TextureManager.Instance.HeightMap);
            }

            if (TextureManager.Instance.HdHeightMap != null)
            {
                Logger.Log("Setting HD Height");
                TextureManager.Instance.HdHeightMap = SetMap(TextureManager.Instance.HdHeightMap);
            }

            yield return new WaitForSeconds(0.1f);

            if (TextureManager.Instance.DiffuseMap != null)
            {
                Logger.Log("Setting Diffuse");
                TextureManager.Instance.DiffuseMap = SetMap(TextureManager.Instance.DiffuseMap);
            }

            yield return new WaitForSeconds(0.1f);

            if (TextureManager.Instance.DiffuseMapOriginal != null)
            {
                Logger.Log("Setting Diffuse Original");
                TextureManager.Instance.DiffuseMapOriginal = SetMap(TextureManager.Instance.DiffuseMapOriginal);
            }

            yield return new WaitForSeconds(0.1f);

            if (TextureManager.Instance.NormalMap != null)
            {
                Logger.Log("Setting Normal");
                TextureManager.Instance.NormalMap = SetMap(TextureManager.Instance.NormalMap);
            }

            yield return new WaitForSeconds(0.1f);

            if (TextureManager.Instance.MetallicMap != null)
            {
                Logger.Log("Setting Metallic");
                TextureManager.Instance.MetallicMap = SetMap(TextureManager.Instance.MetallicMap);
            }

            yield return new WaitForSeconds(0.1f);

            if (TextureManager.Instance.SmoothnessMap != null)
            {
                Logger.Log("Setting Smoothness");
                TextureManager.Instance.SmoothnessMap = SetMap(TextureManager.Instance.SmoothnessMap);
            }

            yield return new WaitForSeconds(0.1f);

            if (TextureManager.Instance.MaskMap != null)
            {
                Logger.Log("Setting MaskMap");
                TextureManager.Instance.MaskMap = SetMap(TextureManager.Instance.MaskMap);
            }

            yield return new WaitForSeconds(0.1f);

            if (TextureManager.Instance.AoMap != null)
            {
                Logger.Log("Setting AO");
                TextureManager.Instance.AoMap = SetMap(TextureManager.Instance.AoMap);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}