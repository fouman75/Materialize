#region

using System;
using System.Collections;
using System.Collections.Generic;
using General;
using Plugins.Extension;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;

//using UnityEngine.Experimental.Rendering.HDPipeline;

#endregion

namespace Gui
{
    public class MainGui : MonoBehaviour
    {
        #region Variables

        public static MainGui Instance;

        #region Gui Objects and scripts

        public GameObject HeightFromDiffuseGuiObject;
        [HideInInspector] public HeightFromDiffuseGui HeightFromDiffuseGuiScript;

        public GameObject MaterialGuiObject;
        [HideInInspector] public MaterialGui MaterialGuiScript;

        public GameObject MetallicGuiObject;
        [HideInInspector] public MetallicGui MetallicGuiScript;

        public GameObject NormalFromHeightGuiObject;
        [HideInInspector] public NormalFromHeightGui NormalFromHeightGuiScript;

        public GameObject EditDiffuseGuiObject;
        [HideInInspector] public EditDiffuseGui EditDiffuseGuiScript;

        public GameObject SmoothnessGuiObject;
        [HideInInspector] public SmoothnessGui SmoothnessGuiScript;

        public GameObject AoFromNormalGuiObject;
        [HideInInspector] public AoFromNormalGui AoFromNormalGuiScript;

        public GameObject PostProcessGuiObject;

        private TilingTextureMakerGui _tilingTextureMakerGuiScript;
        private SaveLoadProject _saveLoadProjectScript;
        public AlignmentGui AlignmentGuiScript;

        #endregion

        private ProgramEnums.MapType _activeMapType;
        private bool _busySaving;
        private bool _clearTextures;
        private bool _exrSelected;
        private bool _jpgSelected;
        private string _lastDirectory = "";
        private List<GameObject> _objectsToUnhide;

        private bool _pngSelected = true;
        private bool _propBlueChoose;
        private Material _propertyCompMaterial;
        private Shader _propertyCompShader;
        private bool _propGreenChoose;
        private bool _propRedChoose;
        private int _selectedCubemap;
        private Texture2D _textureToLoad;
        private Texture2D _textureToSave;
        private bool _tgaSelected;

        private Material _thisMaterial;

        public Cubemap[] CubeMaps;

        [HideInInspector] public Material FullMaterialCopy;

        // ReSharper disable once RedundantDefaultMemberInitializer
        [SerializeField] private Material FullMaterial = null;

        public PropChannelMap PropBlue = PropChannelMap.None;

        public PropChannelMap PropGreen = PropChannelMap.None;

        public PropChannelMap PropRed = PropChannelMap.None;

        #region QuickSave

        public string QuicksavePathProperty = "";

        #endregion

        public ReflectionProbe ReflectionProbe;
        [HideInInspector] public Material SampleMaterial;
        public Material SampleMaterialRef;

        public GameObject SaveLoadProjectObject;
        public ProgramEnums.FileFormat SelectedFormat;

        public Texture2D TextureBlack;
        public Texture2D TextureGrey;
        public GameObject TilingTextureMakerGuiObject;
        public VolumeProfile VolumeProfile;
        [HideInInspector] public HDRISky HdriSky;

        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            Instance = this;
        }

        private IEnumerator Start()
        {
            _propertyCompShader = Shader.Find("Hidden/Blit_Property_Comp");
            _propertyCompMaterial = new Material(_propertyCompShader);

            FullMaterialCopy = new Material(FullMaterial.shader);
            FullMaterialCopy.CopyPropertiesFromMaterial(FullMaterial);

            SampleMaterial = new Material(SampleMaterialRef.shader);
            SampleMaterial.CopyPropertiesFromMaterial(SampleMaterialRef);

            HeightFromDiffuseGuiScript = HeightFromDiffuseGuiObject.GetComponent<HeightFromDiffuseGui>();
            NormalFromHeightGuiScript = NormalFromHeightGuiObject.GetComponent<NormalFromHeightGui>();
            AoFromNormalGuiScript = AoFromNormalGuiObject.GetComponent<AoFromNormalGui>();
            EditDiffuseGuiScript = EditDiffuseGuiObject.GetComponent<EditDiffuseGui>();
            MetallicGuiScript = MetallicGuiObject.GetComponent<MetallicGui>();
            SmoothnessGuiScript = SmoothnessGuiObject.GetComponent<SmoothnessGui>();
            MaterialGuiScript = MaterialGuiObject.GetComponent<MaterialGui>();
            _tilingTextureMakerGuiScript = TilingTextureMakerGuiObject.GetComponent<TilingTextureMakerGui>();
            _saveLoadProjectScript = SaveLoadProjectObject.GetComponent<SaveLoadProject>();

            HideGuiLocker.LockEmpty += LoadHideState;
            VolumeProfile.TryGet(out HdriSky);
            HdriSky.hdriSky.value = CubeMaps[0];

            HDRenderPipeline hdrp = null;
            if (RenderPipelineManager.currentPipeline != null)
            {
                hdrp = RenderPipelineManager.currentPipeline as HDRenderPipeline;
            }

            while (hdrp == null)
            {
                if (RenderPipelineManager.currentPipeline != null)
                {
                    hdrp = RenderPipelineManager.currentPipeline as HDRenderPipeline;
                }

                yield return new WaitForSeconds(0.1f);
            }

            hdrp.RequestSkyEnvironmentUpdate();
            Debug.Log("HDRI Sky Atualizado");
        }

        #endregion

        #region Gui

        private void OnGUI()
        {
            #region Unhideable Buttons

            //==================================================//
            // 					Unhidable Buttons				//
            //==================================================//

            if (GUI.Button(new Rect(Screen.width - 80, Screen.height - 40, 70, 30), "Quit")) Application.Quit();

            GUI.enabled = false;
            if (Screen.fullScreen)
            {
                if (GUI.Button(new Rect(Screen.width - 190, Screen.height - 40, 100, 30), "Windowed")) Fullscreen();
            }
            else
            {
                if (GUI.Button(new Rect(Screen.width - 190, Screen.height - 40, 100, 30), "Full Screen")) Fullscreen();
            }

            GUI.enabled = true;

//        if (GUI.Button(new Rect(Screen.width - 260, 10, 140, 30), "Make Suggestion"))
//            SuggestionGuiObject.SetActive(true);

            if (IsGuiHidden)
            {
                if (GUI.Button(new Rect(Screen.width - 110, 10, 100, 30), "Show Gui"))
                {
                    IsGuiHidden = false;
                }
                else return;
            }
            else
            {
                if (GUI.Button(new Rect(Screen.width - 110, 10, 100, 30), "Hide Gui"))
                {
                    IsGuiHidden = true;
                }
            }

            #endregion

            #region Main Gui

            //==================================================//
            // 						Main Gui					//
            //==================================================//

            //==============================//
            // 		Map Saving Options		//
            //==============================//

            #region Map Saving Options

            var offsetXm = Screen.width - 230;
            const int offsetY = 50;

            GUI.Box(new Rect(offsetXm, offsetY, 220, 250), "Saving Options");
            offsetXm -= 5;

            GUI.Label(new Rect(offsetXm + 20, offsetY + 20, 100, 25), "File Format");

            _pngSelected = GUI.Toggle(new Rect(offsetXm + 30, offsetY + 60, 80, 20), _pngSelected, "PNG");
            if (_pngSelected) SetFormat(ProgramEnums.FileFormat.Png);

            _jpgSelected = GUI.Toggle(new Rect(offsetXm + 30, offsetY + 80, 80, 20), _jpgSelected, "JPG");
            if (_jpgSelected) SetFormat(ProgramEnums.FileFormat.Jpg);

            _tgaSelected = GUI.Toggle(new Rect(offsetXm + 30, offsetY + 100, 80, 20), _tgaSelected, "TGA");
            if (_tgaSelected) SetFormat(ProgramEnums.FileFormat.Tga);

            _exrSelected = GUI.Toggle(new Rect(offsetXm + 30, offsetY + 120, 80, 20), _exrSelected, "EXR");
            if (_exrSelected) SetFormat(ProgramEnums.FileFormat.Exr);

            // Flip Normal Map Y
            GUI.enabled = TextureManager.Instance.NotNull(ProgramEnums.MapType.Normal);

            if (GUI.Button(new Rect(offsetXm + 10, offsetY + 145, 100, 25), "Flip Normal Y"))
            {
                TextureManager.Instance.FlipNormalY();
            }

            GUI.enabled = true;

            //Save Project
            if (GUI.Button(new Rect(offsetXm + 10, offsetY + 180, 100, 25), "Save Project"))
            {
                const string defaultName = "baseName.mtz";
                StandaloneFileBrowser.StandaloneFileBrowser.SaveFilePanelAsync("Save Project", _lastDirectory,
                    defaultName, "mtz",
                    SaveProjectCallback);
            }

            //Load Project
            if (GUI.Button(new Rect(offsetXm + 10, offsetY + 215, 100, 25), "Load Project"))
            {
                StandaloneFileBrowser.StandaloneFileBrowser.OpenFilePanelAsync("Load Project", _lastDirectory, "mtz",
                    false,
                    LoadProjectCallback);
            }

            #endregion

            //======================================//
            //			Property Map Settings		//
            //======================================//

            #region Property Map Settings

            GUI.Label(new Rect(offsetXm + 130, offsetY + 20, 100, 25), "Property Map");

            GUI.enabled = !_propRedChoose;

            GUI.Label(new Rect(offsetXm + 100, offsetY + 45, 20, 20), "R:");
            if (GUI.Button(new Rect(offsetXm + 120, offsetY + 45, 100, 25), PCM2String(PropRed, "Red None")))
            {
                _propRedChoose = true;
                _propGreenChoose = false;
                _propBlueChoose = false;
            }

            GUI.enabled = !_propGreenChoose;

            GUI.Label(new Rect(offsetXm + 100, offsetY + 80, 20, 20), "G:");
            if (GUI.Button(new Rect(offsetXm + 120, offsetY + 80, 100, 25), PCM2String(PropGreen, "Green None")))
            {
                _propRedChoose = false;
                _propGreenChoose = true;
                _propBlueChoose = false;
            }

            GUI.enabled = !_propBlueChoose;

            GUI.Label(new Rect(offsetXm + 100, offsetY + 115, 20, 20), "B:");
            if (GUI.Button(new Rect(offsetXm + 120, offsetY + 115, 100, 25), PCM2String(PropBlue, "Blue None")))
            {
                _propRedChoose = false;
                _propGreenChoose = false;
                _propBlueChoose = true;
            }

            GUI.enabled = true;

            var propBoxOffsetX = offsetXm + 250;
            const int propBoxOffsetY = 20;
            if (_propRedChoose || _propGreenChoose || _propBlueChoose)
            {
                GUI.Box(new Rect(propBoxOffsetX, propBoxOffsetY, 150, 245), "Map for Channel");
                var chosen = false;
                var chosenPcm = PropChannelMap.None;

                if (GUI.Button(new Rect(propBoxOffsetX + 10, propBoxOffsetY + 30, 130, 25), "None"))
                {
                    chosen = true;
                    chosenPcm = PropChannelMap.None;
                }

                if (GUI.Button(new Rect(propBoxOffsetX + 10, propBoxOffsetY + 60, 130, 25), "Height"))
                {
                    chosen = true;
                    chosenPcm = PropChannelMap.Height;
                }

                if (GUI.Button(new Rect(propBoxOffsetX + 10, propBoxOffsetY + 90, 130, 25), "Metallic"))
                {
                    chosen = true;
                    chosenPcm = PropChannelMap.Metallic;
                }

                if (GUI.Button(new Rect(propBoxOffsetX + 10, propBoxOffsetY + 120, 130, 25), "Smoothness"))
                {
                    chosen = true;
                    chosenPcm = PropChannelMap.Smoothness;
                }

                if (GUI.Button(new Rect(propBoxOffsetX + 10, propBoxOffsetY + 150, 130, 25), "MaskMap"))
                {
                    chosen = true;
                    chosenPcm = PropChannelMap.MaskMap;
                }

                if (GUI.Button(new Rect(propBoxOffsetX + 10, propBoxOffsetY + 180, 130, 25), "Ambient Occlusion"))
                {
                    chosen = true;
                    chosenPcm = PropChannelMap.Ao;
                }

                if (chosen)
                {
                    if (_propRedChoose) PropRed = chosenPcm;

                    if (_propGreenChoose) PropGreen = chosenPcm;

                    if (_propBlueChoose) PropBlue = chosenPcm;

                    _propRedChoose = false;
                    _propGreenChoose = false;
                    _propBlueChoose = false;
                }
            }

            if (GUI.Button(new Rect(offsetXm + 120, offsetY + 150, 100, 40), "Save\r\nProperty Map"))
            {
                ProcessPropertyMap();
                TextureManager.Instance.SaveMap(ProgramEnums.MapType.Property);
            }

            if (QuicksavePathProperty == "") GUI.enabled = false;

            if (GUI.Button(new Rect(offsetXm + 120, offsetY + 200, 100, 40), "Quick Save\r\nProperty Map"))
            {
            }

            GUI.enabled = true;

            #endregion

            #endregion
        }

        public void AdjustAlignment()
        {
            CloseWindows();
            TextureManager.Instance.FixSize();
            AlignmentGuiScript.Initialize();
        }

        public void OpenTileMaps()
        {
            CloseWindows();
            TextureManager.Instance.FixSize();
            TilingTextureMakerGuiObject.SetActive(true);
            _tilingTextureMakerGuiScript.Initialize();
        }

        public void NextCubeMap()
        {
            _selectedCubemap += 1;
            if (_selectedCubemap >= CubeMaps.Length) _selectedCubemap = 0;

            HdriSky.hdriSky.value = CubeMaps[_selectedCubemap];

            var hdrp = UnityEngine.Rendering.RenderPipelineManager.currentPipeline as HDRenderPipeline;
            hdrp.RequestSkyEnvironmentUpdate();
        }


        private void SaveProjectCallback(string path)
        {
            if (path.IsNullOrEmpty()) return;

            var lastBar = path.LastIndexOf(ProgramManager.Instance.PathChar);
            _lastDirectory = path.Substring(0, lastBar + 1);

            _saveLoadProjectScript.SaveProject(path);
        }

        private void LoadProjectCallback(string[] path)
        {
            if (path[0].IsNullOrEmpty()) return;

            var lastBar = path[0].LastIndexOf(ProgramManager.Instance.PathChar);
            _lastDirectory = path[0].Substring(0, lastBar + 1);

            _saveLoadProjectScript.LoadProject(path[0]);
        }

        private void ShowGui()
        {
            foreach (var objToHide in _objectsToUnhide)
                objToHide.SetActive(true);
        }

        private void HideGui()
        {
            HideWindows();
        }

        #endregion

        #region Hide State

        public void SaveHideState()
        {
            if (HideGuiLocker.IsLocked) return;
            _lastGuiIsHiddenState = IsGuiHidden;
        }

        public void SaveHideStateAndHideAndLock(object sender)
        {
            SaveHideState();
            IsGuiHidden = true;
            HideGuiLocker.Lock(sender);
        }

        private void LoadHideState(object sender, EventArgs eventArgs)
        {
            IsGuiHidden = _lastGuiIsHiddenState;
        }

        #endregion

        #region Windows

        public void CloseWindows()
        {
            HeightFromDiffuseGuiScript.Close();
            NormalFromHeightGuiScript.Close();
            AoFromNormalGuiScript.Close();
            EditDiffuseGuiScript.Close();
            MetallicGuiScript.Close();
            SmoothnessGuiScript.Close();
            _tilingTextureMakerGuiScript.Close();
            AlignmentGuiScript.Close();
            MaterialGuiObject.SetActive(false);
            PostProcessGuiObject.SetActive(false);
        }

        private void HideWindows()
        {
            _objectsToUnhide = new List<GameObject>();

            if (HeightFromDiffuseGuiObject.activeSelf) _objectsToUnhide.Add(HeightFromDiffuseGuiObject);

            if (NormalFromHeightGuiObject.activeSelf) _objectsToUnhide.Add(NormalFromHeightGuiObject);

            if (AoFromNormalGuiObject.activeSelf) _objectsToUnhide.Add(AoFromNormalGuiObject);

            if (EditDiffuseGuiObject.activeSelf) _objectsToUnhide.Add(EditDiffuseGuiObject);

            if (MetallicGuiObject.activeSelf) _objectsToUnhide.Add(MetallicGuiObject);

            if (SmoothnessGuiObject.activeSelf) _objectsToUnhide.Add(SmoothnessGuiObject);

            if (MaterialGuiObject.activeSelf) _objectsToUnhide.Add(MaterialGuiObject);

            if (PostProcessGuiObject.activeSelf) _objectsToUnhide.Add(PostProcessGuiObject);

            if (TilingTextureMakerGuiObject.activeSelf) _objectsToUnhide.Add(TilingTextureMakerGuiObject);

            HeightFromDiffuseGuiObject.SetActive(false);
            NormalFromHeightGuiObject.SetActive(false);
            AoFromNormalGuiObject.SetActive(false);
            EditDiffuseGuiObject.SetActive(false);
            MetallicGuiObject.SetActive(false);
            SmoothnessGuiObject.SetActive(false);
            MaterialGuiObject.SetActive(false);
            PostProcessGuiObject.SetActive(false);
            TilingTextureMakerGuiObject.SetActive(false);
        }

        private static void Fullscreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
        }

        #endregion

        #region Texture Handle

        // ReSharper disable once InconsistentNaming
        private static string PCM2String(PropChannelMap pcm, string defaultName)
        {
            var returnString = defaultName;

            switch (pcm)
            {
                case PropChannelMap.Height:
                    returnString = "Height";
                    break;
                case PropChannelMap.Metallic:
                    returnString = "Metallic";
                    break;
                case PropChannelMap.Smoothness:
                    returnString = "Smoothness";
                    break;
                case PropChannelMap.MaskMap:
                    returnString = "MaskMap";
                    break;
                case PropChannelMap.Ao:
                    returnString = "Ambient Occ";
                    break;
                case PropChannelMap.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pcm), pcm, null);
            }

            return returnString;
        }


        public void SetFormat(ProgramEnums.FileFormat newFormat)
        {
            _jpgSelected = false;
            _pngSelected = false;
            _tgaSelected = false;
            _exrSelected = false;

            switch (newFormat)
            {
                case ProgramEnums.FileFormat.Jpg:
                    _jpgSelected = true;
                    break;
                case ProgramEnums.FileFormat.Png:
                    _pngSelected = true;
                    break;
                case ProgramEnums.FileFormat.Tga:
                    _tgaSelected = true;
                    break;
                case ProgramEnums.FileFormat.Exr:
                    _exrSelected = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newFormat), newFormat, null);
            }

            SelectedFormat = newFormat;
        }

        public void SetFormat(string newFormat)
        {
            _jpgSelected = false;
            _pngSelected = false;
            _tgaSelected = false;
            _exrSelected = false;

            switch (newFormat)
            {
                case "jpg":
                    _jpgSelected = true;
                    SelectedFormat = ProgramEnums.FileFormat.Jpg;
                    break;
                case "png":
                    _pngSelected = true;
                    SelectedFormat = ProgramEnums.FileFormat.Png;
                    break;
                case "tga":
                    _tgaSelected = true;
                    SelectedFormat = ProgramEnums.FileFormat.Tga;
                    break;
                case "exr":
                    _exrSelected = true;
                    SelectedFormat = ProgramEnums.FileFormat.Exr;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newFormat), newFormat, null);
            }
        }

        #endregion

        //==================================================//
        //					Property Map					//
        //==================================================//

        #region Property Map

        private void SetPropertyTexture(string texPrefix, Texture2D texture, Texture overlayTexture)
        {
            _propertyCompMaterial.SetTexture(texPrefix + "Tex", texture != null ? texture : TextureBlack);

            _propertyCompMaterial.SetTexture(texPrefix + "OverlayTex", overlayTexture);
        }

        private void SetPropertyMapChannel(string texPrefix, PropChannelMap pcm)
        {
            switch (pcm)
            {
                case PropChannelMap.Height:
                    SetPropertyTexture(texPrefix, TextureManager.Instance.HeightMap, TextureGrey);
                    break;
                case PropChannelMap.Metallic:
                    SetPropertyTexture(texPrefix, TextureManager.Instance.MetallicMap, TextureGrey);
                    break;
                case PropChannelMap.Smoothness:
                    SetPropertyTexture(texPrefix, TextureManager.Instance.SmoothnessMap, TextureGrey);
                    break;
                case PropChannelMap.Ao:
                    SetPropertyTexture(texPrefix, TextureManager.Instance.AoMap, TextureGrey);
                    break;
                case PropChannelMap.MaskMap:
                    SetPropertyTexture(texPrefix, TextureManager.Instance.MaskMap, TextureGrey);
                    break;
                case PropChannelMap.None:
                    SetPropertyTexture(texPrefix, TextureBlack, TextureGrey);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pcm), pcm, null);
            }
        }

        public void ProcessPropertyMap()
        {
            SetPropertyMapChannel("_Red", PropRed);
            SetPropertyMapChannel("_Green", PropGreen);
            SetPropertyMapChannel("_Blue", PropBlue);

            var size = TextureManager.Instance.GetSize();
            var tempMap = TextureManager.Instance.GetTempRenderTexture(size.x, size.y);
            Graphics.Blit(TextureManager.Instance.MetallicMap, tempMap, _propertyCompMaterial, 0);
            RenderTexture.active = tempMap;

            if (TextureManager.Instance.PropertyMap != null)
            {
                Destroy(TextureManager.Instance.PropertyMap);
                TextureManager.Instance.PropertyMap = null;
            }

            TextureManager.Instance.PropertyMap =
                new Texture2D(tempMap.width, tempMap.height, TextureFormat.RGB24, false);
            TextureManager.Instance.PropertyMap.ReadPixels(new Rect(0, 0, tempMap.width, tempMap.height), 0, 0);
            TextureManager.Instance.PropertyMap.Apply(false);

            RenderTexture.ReleaseTemporary(tempMap);
            // ReSharper disable once RedundantAssignment
            tempMap = null;
        }

        #endregion

        //==================================================//
        //					Project Saving					//
        //==================================================//


        //==================================================//
        //			Fix the size of the test model			//
        //==================================================//


        #region Gui Hide Variables

        [HideInInspector] public CountLocker HideGuiLocker = new CountLocker();
        private bool _lastGuiIsHiddenState;
        private bool _isGuiHidden;

        public bool IsGuiHidden
        {
            get => _isGuiHidden;
            set
            {
                if (HideGuiLocker.IsLocked)
                {
                    Debug.Log("Tentando modificar IsGuiHidden quando travado");
                    return;
                }

                if (value && !_isGuiHidden)
                {
                    HideGui();
                    _isGuiHidden = true;
                }
                else if (!value && _isGuiHidden)
                {
                    ShowGui();
                    _isGuiHidden = false;
                }
            }
        }

        #endregion

        public void SaveImage(ProgramEnums.MapType mapType)
        {
            switch (mapType)
            {
                case ProgramEnums.MapType.Height:
                    break;
                case ProgramEnums.MapType.Diffuse:
                    break;
                case ProgramEnums.MapType.DiffuseOriginal:
                    break;
                case ProgramEnums.MapType.Metallic:
                    break;
                case ProgramEnums.MapType.Smoothness:
                    break;
                case ProgramEnums.MapType.Normal:
                    break;
                case ProgramEnums.MapType.Ao:
                    break;
                case ProgramEnums.MapType.Property:
                    break;
                case ProgramEnums.MapType.MaskMap:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }
        }

        public void LoadImage(ProgramEnums.MapType mapType)
        {
            switch (mapType)
            {
                case ProgramEnums.MapType.Height:
                    break;
                case ProgramEnums.MapType.Diffuse:
                    break;
                case ProgramEnums.MapType.DiffuseOriginal:
                    break;
                case ProgramEnums.MapType.Metallic:
                    break;
                case ProgramEnums.MapType.Smoothness:
                    break;
                case ProgramEnums.MapType.Normal:
                    break;
                case ProgramEnums.MapType.Ao:
                    break;
                case ProgramEnums.MapType.Property:
                    break;
                case ProgramEnums.MapType.MaskMap:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }
        }

        public void PasteImage(ProgramEnums.MapType mapType)
        {
            switch (mapType)
            {
                case ProgramEnums.MapType.Height:
                    break;
                case ProgramEnums.MapType.Diffuse:
                    break;
                case ProgramEnums.MapType.DiffuseOriginal:
                    break;
                case ProgramEnums.MapType.Metallic:
                    break;
                case ProgramEnums.MapType.Smoothness:
                    break;
                case ProgramEnums.MapType.Normal:
                    break;
                case ProgramEnums.MapType.Ao:
                    break;
                case ProgramEnums.MapType.Property:
                    break;
                case ProgramEnums.MapType.MaskMap:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }
        }

        public void CopyImage(ProgramEnums.MapType mapType)
        {
            switch (mapType)
            {
                case ProgramEnums.MapType.Height:
                    break;
                case ProgramEnums.MapType.Diffuse:
                    break;
                case ProgramEnums.MapType.DiffuseOriginal:
                    break;
                case ProgramEnums.MapType.Metallic:
                    break;
                case ProgramEnums.MapType.Smoothness:
                    break;
                case ProgramEnums.MapType.Normal:
                    break;
                case ProgramEnums.MapType.Ao:
                    break;
                case ProgramEnums.MapType.Property:
                    break;
                case ProgramEnums.MapType.MaskMap:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }
        }

        public void CreateImage(ProgramEnums.MapType mapType)
        {
            switch (mapType)
            {
                case ProgramEnums.MapType.Height:
                    CreateHeight();
                    break;
                case ProgramEnums.MapType.Diffuse:
                    break;
                case ProgramEnums.MapType.DiffuseOriginal:
                    break;
                case ProgramEnums.MapType.Metallic:
                    CreateMetallic();
                    break;
                case ProgramEnums.MapType.Smoothness:
                    CreateSmoothness();
                    break;
                case ProgramEnums.MapType.Normal:
                    CreateNormal();
                    break;
                case ProgramEnums.MapType.Ao:
                    CreateAo();
                    break;
                case ProgramEnums.MapType.Property:
                    break;
                case ProgramEnums.MapType.MaskMap:
                    CreateMaskMap();
                    break;
                case ProgramEnums.MapType.None:
                    break;
                case ProgramEnums.MapType.Any:
                    break;
                case ProgramEnums.MapType.AnyDiffuse:
                    EditDiffuse();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }
        }

        private void CreateHeight()
        {
            CloseWindows();
            TextureManager.Instance.FixSize();
            HeightFromDiffuseGuiObject.SetActive(true);
            HeightFromDiffuseGuiScript.NewTexture();
            HeightFromDiffuseGuiScript.DoStuff();
        }

        private void EditDiffuse()
        {
            CloseWindows();
            TextureManager.Instance.FixSize();
            EditDiffuseGuiObject.SetActive(true);
            EditDiffuseGuiScript.NewTexture();
            EditDiffuseGuiScript.DoStuff();
        }

        private void CreateNormal()
        {
            CloseWindows();
            TextureManager.Instance.FixSize();
            NormalFromHeightGuiObject.SetActive(true);
            NormalFromHeightGuiScript.NewTexture();
            NormalFromHeightGuiScript.DoStuff();
        }

        private void CreateMetallic()
        {
            CloseWindows();
            TextureManager.Instance.FixSize();
            MetallicGuiObject.SetActive(true);
            MetallicGuiScript.NewTexture();
            MetallicGuiScript.DoStuff();
        }

        private void CreateSmoothness()
        {
            CloseWindows();
            TextureManager.Instance.FixSize();
            SmoothnessGuiObject.SetActive(true);
            SmoothnessGuiScript.NewTexture();
            SmoothnessGuiScript.DoStuff();
        }

        private void CreateAo()
        {
            CloseWindows();
            TextureManager.Instance.FixSize();
            AoFromNormalGuiObject.SetActive(true);
            AoFromNormalGuiScript.NewTexture();
            AoFromNormalGuiScript.DoStuff();
        }

        private void CreateMaskMap()
        {
            CloseWindows();
            TextureManager.Instance.FixSize();
            TextureManager.Instance.MakeMaskMap();
        }
    }
}