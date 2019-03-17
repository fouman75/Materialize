#region

using System.Collections;
using General;
using JetBrains.Annotations;
using UnityEngine;
using Logger = General.Logger;

#endregion

namespace Gui
{
    public class AlignmentGui : MonoBehaviour, IHideable
    {
        private static readonly int TargetPoint = Shader.PropertyToID("_TargetPoint");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int CorrectTex = Shader.PropertyToID("_CorrectTex");
        private static readonly int PointScale = Shader.PropertyToID("_PointScale");
        private static readonly int PointTl = Shader.PropertyToID("_PointTL");
        private static readonly int PointTr = Shader.PropertyToID("_PointTR");
        private static readonly int PointBl = Shader.PropertyToID("_PointBL");
        private static readonly int PointBr = Shader.PropertyToID("_PointBR");
        private static readonly int Width = Shader.PropertyToID("_Width");
        private static readonly int Height = Shader.PropertyToID("_Height");
        private static readonly int Lens = Shader.PropertyToID("_Lens");
        private static readonly int PerspectiveX = Shader.PropertyToID("_PerspectiveX");
        private static readonly int PerspectiveY = Shader.PropertyToID("_PerspectiveY");
        private static readonly int Slider = Shader.PropertyToID("_Slider");
        private RenderTexture _alignMap;

        private Material _blitMaterial;
        private Camera _camera;

        private bool _doStuff;


        private int _grabbedPoint;

        private float _lensDistort;

        private RenderTexture _lensMap;

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

        private Rect _windowRect;
        [UsedImplicitly] public bool NewTexture;
        public GameObject TestObject;

        public Material ThisMaterial;

        public bool Hide { get; set; }

        private void Awake()
        {
            _camera = Camera.main;
            _windowRect = new Rect(10.0f, 265.0f, 300f, 430f);
        }

        private void OnDisable()
        {
            RenderTexture.ReleaseTemporary(_alignMap);
            RenderTexture.ReleaseTemporary(_lensMap);
            RenderTexture.ReleaseTemporary(_perspectiveMap);
        }

        public void Initialize()
        {
            gameObject.SetActive(true);
            TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;
            _blitMaterial = new Material(Shader.Find("Hidden/Blit_Alignment")) {hideFlags = HideFlags.HideAndDontSave};

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


            _doStuff = true;
        }


        private static void CleanupTexture(RenderTexture texture)
        {
            if (!texture) return;
            texture.Release();
            // ReSharper disable once RedundantAssignment
            texture = null;
        }

        public void Close()
        {
            CleanupTexture(_lensMap);
            CleanupTexture(_alignMap);
            CleanupTexture(_perspectiveMap);
            gameObject.SetActive(false);
        }

        private void SelectClosestPoint()
        {
            if (Input.GetMouseButton(0)) return;
            if (!_camera) return;

            if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit))
                return;

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
            if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit))
                return;

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

                if (point != null) ThisMaterial.SetVector(TargetPoint, point);

                _startOffset = hitTc;
            }

            _doStuff = true;
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

            _blitMaterial.SetVector(PointTl, _pointTl);
            _blitMaterial.SetVector(PointTr, _pointTr);
            _blitMaterial.SetVector(PointBl, _pointBl);
            _blitMaterial.SetVector(PointBr, _pointBr);

            _blitMaterial.SetFloat(Width, _textureToAlign.width);
            _blitMaterial.SetFloat(Height, _textureToAlign.height);

            _blitMaterial.SetFloat(Lens, _lensDistort);
            _blitMaterial.SetFloat(PerspectiveX, _perspectiveX);
            _blitMaterial.SetFloat(PerspectiveY, _perspectiveY);

            if (_doStuff) _doStuff = false;

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
                _doStuff = true;
            }

            GUI.enabled = TextureManager.Instance.DiffuseMap != null;
            if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Diffuse Map"))
            {
                _textureToAlign = TextureManager.Instance.DiffuseMap;
                _doStuff = true;
            }

            offsetY += 40;


            GUI.enabled = TextureManager.Instance.HeightMap != null;
            if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Height Map"))
            {
                _textureToAlign = TextureManager.Instance.HeightMap;
                _doStuff = true;
            }

            offsetY += 40;

            GUI.enabled = TextureManager.Instance.MetallicMap != null;
            if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Metallic Map"))
            {
                _textureToAlign = TextureManager.Instance.MetallicMap;
                _doStuff = true;
            }

            GUI.enabled = TextureManager.Instance.SmoothnessMap != null;
            if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Smoothness Map"))
            {
                _textureToAlign = TextureManager.Instance.SmoothnessMap;
                _doStuff = true;
            }

            offsetY += 40;

            GUI.enabled = TextureManager.Instance.MaskMap != null;
            if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Mask Map"))
            {
                _textureToAlign = TextureManager.Instance.MaskMap;
                _doStuff = true;
            }

            GUI.enabled = TextureManager.Instance.AoMap != null;
            if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "AO Map"))
            {
                _textureToAlign = TextureManager.Instance.AoMap;
                _doStuff = true;
            }

            offsetY += 40;

            GUI.enabled = true;


            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Lens Distort Correction", _lensDistort,
                out _lensDistort, -1.0f, 1.0f)) _doStuff = true;
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Perspective Correction X", _perspectiveX,
                out _perspectiveX, -5.0f, 5.0f)) _doStuff = true;
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Perspective Correction Y", _perspectiveY,
                out _perspectiveY, -5.0f, 5.0f)) _doStuff = true;
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
            var pivotPoint = new Vector2(_windowRect.x, _windowRect.y);
            GUIUtility.ScaleAroundPivot(ProgramManager.Instance.GuiScale, pivotPoint);

            _windowRect = GUI.Window(21, _windowRect, DoMyWindow, "Texture Alignment Adjuster");
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

            Graphics.Blit(textureTarget, _lensMap, _blitMaterial, 0);
            Graphics.Blit(_lensMap, _alignMap, _blitMaterial, 1);
            Graphics.Blit(_alignMap, _perspectiveMap, _blitMaterial, 2);
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

            Graphics.Blit(textureTarget, _lensMap, _blitMaterial, 0);
            Graphics.Blit(_lensMap, _alignMap, _blitMaterial, 1);
            Graphics.Blit(_alignMap, _perspectiveMap, _blitMaterial, 2);

            var replaceTexture = _textureToAlign == textureTarget;

            Destroy(textureTarget);
            // ReSharper disable once RedundantAssignment
            textureTarget = null;

            RenderTexture.active = _perspectiveMap;
            textureTarget = TextureManager.Instance.GetStandardTexture(width, height);
            textureTarget.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            textureTarget.Apply(false);

            RenderTexture.active = null;

            CleanupTexture(_lensMap);
            CleanupTexture(_alignMap);
            CleanupTexture(_perspectiveMap);

            if (replaceTexture) _textureToAlign = textureTarget;

            _doStuff = true;

            return textureTarget;
        }

        private RenderTexture SetMap(RenderTexture textureTarget)
        {
            var width = textureTarget.width;
            var height = textureTarget.height;

            CleanupTexture(_lensMap);
            CleanupTexture(_alignMap);
            CleanupTexture(_perspectiveMap);

            _lensMap = new RenderTexture(width, height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            _alignMap = new RenderTexture(width, height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            _perspectiveMap =
                new RenderTexture(width, height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);

            Graphics.Blit(textureTarget, _lensMap, _blitMaterial, 0);
            Graphics.Blit(_lensMap, _alignMap, _blitMaterial, 1);
            Graphics.Blit(_alignMap, _perspectiveMap, _blitMaterial, 2);

            if (textureTarget != null)
            {
                textureTarget.Release();
                textureTarget = null;
            }

            Graphics.Blit(_perspectiveMap, textureTarget);

            CleanupTexture(_lensMap);
            CleanupTexture(_alignMap);
            CleanupTexture(_perspectiveMap);

            _doStuff = true;

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