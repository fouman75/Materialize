#region

using System;
using System.Collections.Generic;
using SFB;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;
using Utility;

//using UnityEngine.Experimental.Rendering.HDPipeline;

#endregion

namespace Gui
{
    public class MainGui : MonoBehaviour
    {
        //Nao remover, alguns shaders dependem disso
        private const float GamaCorrection = 1f;

        #region Variables

        public static MainGui Instance;

        public static readonly string[] LoadFormats =
        {
            "png", "jpg", "jpeg", "tga", "bmp", "exr"
        };

        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int DiffuseMapId = Shader.PropertyToID("_BaseColorMap");
        private static readonly int NormalMapId = Shader.PropertyToID("_NormalMap");
        private static readonly int MaskMapId = Shader.PropertyToID("_MaskMap");
        private static readonly int HeightMapId = Shader.PropertyToID("_HeightMap");
        private static readonly int GamaCorrectionId = Shader.PropertyToID("_GamaCorrection");

        private readonly ExtensionFilter[] _imageLoadFilter =
        {
            new ExtensionFilter("Image Files", LoadFormats)
        };

        private readonly ExtensionFilter[] _imageSaveFilter =
        {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg", "tga", "exr")
        };

        #region Map Variables

        [HideInInspector] public RenderTexture HdHeightMap;
        [HideInInspector] public Texture2D HeightMap;
        [HideInInspector] public Texture2D DiffuseMap;
        [HideInInspector] public Texture2D DiffuseMapOriginal;
        [HideInInspector] public Texture2D NormalMap;
        [HideInInspector] public Texture2D MetallicMap;
        [HideInInspector] public Texture2D SmoothnessMap;
        [HideInInspector] public Texture2D AoMap;
        [HideInInspector] public Texture2D MaskMap;
        [HideInInspector] public Texture2D PropertyMap;

        #endregion

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

        public GameObject SettingsGuiObject;
        private SettingsGui _settingsGuiScript;

        public GameObject PostProcessGuiObject;

        private TilingTextureMakerGui _tilingTextureMakerGuiScript;
        private SaveLoadProject _saveLoadProjectScript;
        public AlignmentGui AlignmentGuiScript;

        #endregion

        private MapType _activeMapType;
        private bool _busySaving;
        private bool _clearTextures;
        private bool _exrSelected;
        private bool _jpgSelected;
        private string _lastDirectory = "";
        private List<GameObject> _objectsToUnhide;
        private char _pathChar = '/';
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

        public string QuicksavePathAo = "";
        public string QuicksavePathDiffuse = "";
        public string QuicksavePathMaskMap = "";
        public string QuicksavePathHeight = "";
        public string QuicksavePathMetallic = "";
        public string QuicksavePathNormal = "";
        public string QuicksavePathProperty = "";
        public string QuicksavePathSmoothness = "";

        #endregion

        public ReflectionProbe ReflectionProbe;
        [HideInInspector] public Material SampleMaterial;
        public Material SampleMaterialRef;

        public GameObject SaveLoadProjectObject;
        public FileFormat SelectedFormat;

        public GameObject TestObject;

        public Texture2D TextureBlack;
        public Texture2D TextureGrey;
        public Texture2D TextureNormal;
        public Texture2D TextureWhite;

        public GameObject TilingTextureMakerGuiObject;
        public VolumeProfile VolumeProfile;
        [HideInInspector] public HDRISky HdriSky;

        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _lastDirectory = Application.dataPath;

            HeightMap = null;
            HdHeightMap = null;
            DiffuseMap = null;
            DiffuseMapOriginal = null;
            NormalMap = null;
            MetallicMap = null;
            SmoothnessMap = null;
            MaskMap = null;
            AoMap = null;

            Shader.SetGlobalFloat(GamaCorrectionId, GamaCorrection);

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
            _settingsGuiScript = SettingsGuiObject.GetComponent<SettingsGui>();

            _settingsGuiScript.LoadSettings();

            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer)
                _pathChar = '\\';

            TestObject.GetComponent<Renderer>().material = FullMaterialCopy;
            SetMaterialValues();

            ReflectionProbe.RenderProbe();

            HideGuiLocker.LockEmpty += LoadHideState;
            VolumeProfile.TryGet(out HdriSky);
            HdriSky.hdriSky.value = CubeMaps[0];
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


            const int spacingX = 130;

            var offsetX = 20;
            var offsetY = 20;

            //==============================//
            // 			Height Map			//
            //==============================//

            #region HeightMap

            GUI.Box(new Rect(offsetX, offsetY, 110, 250), "Height Map");

            if (HeightMap != null) GUI.DrawTexture(new Rect(offsetX + 5, offsetY + 25, 100, 100), HeightMap);

            // Paste 
            if (GUI.Button(new Rect(offsetX + 5, offsetY + 130, 20, 20), "P"))
            {
                _activeMapType = MapType.Height;
                PasteFile();
            }

            GUI.enabled = HeightMap != null;

            // Copy
            if (GUI.Button(new Rect(offsetX + 30, offsetY + 130, 20, 20), "C"))
            {
                _textureToSave = HeightMap;
                CopyFile();
            }

            GUI.enabled = true;

            // Open
            if (GUI.Button(new Rect(offsetX + 60, offsetY + 130, 20, 20), "O")) OpenTextureFile(MapType.Height);

            GUI.enabled = HeightMap != null;

            // Save
            if (GUI.Button(new Rect(offsetX + 85, offsetY + 130, 20, 20), "S")) SaveTextureFile(MapType.Height);


            if (HeightMap == null || QuicksavePathHeight == "")
                GUI.enabled = false;
            else
                GUI.enabled = true;

            // Quick Save
            if (GUI.Button(new Rect(offsetX + 15, offsetY + 160, 80, 20), "Quick Save"))
            {
                _textureToSave = HeightMap;
                SaveFile(QuicksavePathProperty);
            }

            GUI.enabled = HeightMap != null;

            if (GUI.Button(new Rect(offsetX + 15, offsetY + 190, 80, 20), "Preview")) SetPreviewMaterial(HeightMap);

            GUI.enabled = true;

            if (DiffuseMapOriginal == null && DiffuseMap == null && NormalMap == null)
                GUI.enabled = false;
            else
                GUI.enabled = true;

            if (GUI.Button(new Rect(offsetX + 5, offsetY + 220, 50, 20), "Create"))
            {
                CloseWindows();
                FixSize();
                HeightFromDiffuseGuiObject.SetActive(true);
                HeightFromDiffuseGuiScript.NewTexture();
                HeightFromDiffuseGuiScript.DoStuff();
            }

            GUI.enabled = HeightMap != null;

            if (GUI.Button(new Rect(offsetX + 60, offsetY + 220, 45, 20), "Clear"))
            {
                ClearTexture(MapType.Height);
                CloseWindows();
                SetMaterialValues();
                FixSize();
            }

            GUI.enabled = true;

            #endregion

            //==============================//
            // 			Diffuse Map			//
            //==============================//

            #region Diffuse Map

            GUI.Box(new Rect(offsetX + spacingX, offsetY, 110, 250), "Diffuse Map");

            if (DiffuseMap != null)
                GUI.DrawTexture(new Rect(offsetX + spacingX + 5, offsetY + 25, 100, 100), DiffuseMap);
            else if (DiffuseMapOriginal != null)
                GUI.DrawTexture(new Rect(offsetX + spacingX + 5, offsetY + 25, 100, 100), DiffuseMapOriginal);

            // Paste 
            if (GUI.Button(new Rect(offsetX + spacingX + 5, offsetY + 130, 20, 20), "P"))
            {
                _activeMapType = MapType.DiffuseOriginal;
                PasteFile();
            }

            if (DiffuseMapOriginal == null && DiffuseMap == null)
                GUI.enabled = false;
            else
                GUI.enabled = true;

            // Copy
            if (GUI.Button(new Rect(offsetX + spacingX + 30, offsetY + 130, 20, 20), "C"))
            {
                _textureToSave = DiffuseMap != null ? DiffuseMap : DiffuseMapOriginal;

                CopyFile();
            }

            GUI.enabled = true;

            // Open
            if (GUI.Button(new Rect(offsetX + spacingX + 60, offsetY + 130, 20, 20), "O"))
                OpenTextureFile(MapType.DiffuseOriginal);

            if (DiffuseMapOriginal == null && DiffuseMap == null)
                GUI.enabled = false;
            else
                GUI.enabled = true;

            // Save
            if (GUI.Button(new Rect(offsetX + spacingX + 85, offsetY + 130, 20, 20), "S"))
                SaveTextureFile(MapType.Diffuse);

            if (DiffuseMapOriginal == null && DiffuseMap == null || QuicksavePathDiffuse == "")
                GUI.enabled = false;
            else
                GUI.enabled = true;

            // Quick Save
            if (GUI.Button(new Rect(offsetX + spacingX + 15, offsetY + 160, 80, 20), "Quick Save"))
            {
                _textureToSave = DiffuseMap != null ? DiffuseMap : DiffuseMapOriginal;

                SaveFile(QuicksavePathDiffuse);
            }

            if (DiffuseMapOriginal == null && DiffuseMap == null)
                GUI.enabled = false;
            else
                GUI.enabled = true;

            if (GUI.Button(new Rect(offsetX + spacingX + 15, offsetY + 190, 80, 20), "Preview"))
            {
                SetPreviewMaterial(DiffuseMap != null ? DiffuseMap : DiffuseMapOriginal);
            }

            GUI.enabled = DiffuseMapOriginal != null;

            if (GUI.Button(new Rect(offsetX + spacingX + 5, offsetY + 220, 50, 20), "Edit"))
            {
                CloseWindows();
                FixSize();
                EditDiffuseGuiObject.SetActive(true);
                EditDiffuseGuiScript.NewTexture();
                EditDiffuseGuiScript.DoStuff();
            }

            if (DiffuseMapOriginal == null && DiffuseMap == null)
                GUI.enabled = false;
            else
                GUI.enabled = true;

            if (GUI.Button(new Rect(offsetX + spacingX + 60, offsetY + 220, 45, 20), "Clear"))
            {
                ClearTexture(MapType.Diffuse);
                CloseWindows();
                SetMaterialValues();
                FixSize();
            }

            GUI.enabled = true;

            #endregion

            //==============================//
            // 			Normal Map			//
            //==============================//

            #region Normal Map

            GUI.Box(new Rect(offsetX + spacingX * 2, offsetY, 110, 250), "Normal Map");

            if (NormalMap != null)
                GUI.DrawTexture(new Rect(offsetX + spacingX * 2 + 5, offsetY + 25, 100, 100), NormalMap);

            // Paste 
            if (GUI.Button(new Rect(offsetX + spacingX * 2 + 5, offsetY + 130, 20, 20), "P"))
            {
                _activeMapType = MapType.Normal;
                PasteFile();
            }

            GUI.enabled = NormalMap != null;

            // Copy
            if (GUI.Button(new Rect(offsetX + spacingX * 2 + 30, offsetY + 130, 20, 20), "C"))
            {
                _textureToSave = NormalMap;
                CopyFile();
            }

            GUI.enabled = true;

            //Open
            if (GUI.Button(new Rect(offsetX + spacingX * 2 + 60, offsetY + 130, 20, 20), "O"))
                OpenTextureFile(MapType.Normal);

            GUI.enabled = NormalMap != null;

            // Save
            if (GUI.Button(new Rect(offsetX + spacingX * 2 + 85, offsetY + 130, 20, 20), "S"))
                SaveTextureFile(MapType.Normal);

            if (NormalMap == null || QuicksavePathNormal == "")
                GUI.enabled = false;
            else
                GUI.enabled = true;

            // Quick Save
            if (GUI.Button(new Rect(offsetX + spacingX * 2 + 15, offsetY + 160, 80, 20), "Quick Save"))
            {
                _textureToSave = NormalMap;
                SaveFile(QuicksavePathNormal);
            }

            GUI.enabled = NormalMap != null;

            if (GUI.Button(new Rect(offsetX + spacingX * 2 + 15, offsetY + 190, 80, 20), "Preview"))
                SetPreviewMaterial(NormalMap);

            GUI.enabled = HeightMap != null;

            if (GUI.Button(new Rect(offsetX + spacingX * 2 + 5, offsetY + 220, 50, 20), "Create"))
            {
                CloseWindows();
                FixSize();
                NormalFromHeightGuiObject.SetActive(true);
                NormalFromHeightGuiScript.NewTexture();
                NormalFromHeightGuiScript.DoStuff();
            }

            GUI.enabled = NormalMap != null;

            if (GUI.Button(new Rect(offsetX + spacingX * 2 + 60, offsetY + 220, 45, 20), "Clear"))
            {
                ClearTexture(MapType.Normal);
                CloseWindows();
                SetMaterialValues();
                FixSize();
            }

            GUI.enabled = true;

            #endregion

            //==============================//
            // 			Metallic Map		//
            //==============================//

            #region Metallic Map

            GUI.Box(new Rect(offsetX + spacingX * 3, offsetY, 110, 250), "Metallic Map");

            if (MetallicMap != null)
                GUI.DrawTexture(new Rect(offsetX + spacingX * 3 + 5, offsetY + 25, 100, 100), MetallicMap);

            // Paste 
            if (GUI.Button(new Rect(offsetX + spacingX * 3 + 5, offsetY + 130, 20, 20), "P"))
            {
                _activeMapType = MapType.Metallic;
                PasteFile();
            }

            GUI.enabled = MetallicMap != null;

            // Copy
            if (GUI.Button(new Rect(offsetX + spacingX * 3 + 30, offsetY + 130, 20, 20), "C"))
            {
                _textureToSave = MetallicMap;
                CopyFile();
            }

            GUI.enabled = true;

            //Open
            if (GUI.Button(new Rect(offsetX + spacingX * 3 + 60, offsetY + 130, 20, 20), "O"))
                OpenTextureFile(MapType.Metallic);

            GUI.enabled = MetallicMap != null;

            // Save
            if (GUI.Button(new Rect(offsetX + spacingX * 3 + 85, offsetY + 130, 20, 20), "S"))
                SaveTextureFile(MapType.Metallic);

            if (MetallicMap == null || QuicksavePathMetallic == "")
                GUI.enabled = false;
            else
                GUI.enabled = true;

            // Quick Save
            if (GUI.Button(new Rect(offsetX + spacingX * 3 + 15, offsetY + 160, 80, 20), "Quick Save"))
            {
                _textureToSave = MetallicMap;
                SaveFile(QuicksavePathMetallic);
            }

            GUI.enabled = MetallicMap != null;

            if (GUI.Button(new Rect(offsetX + spacingX * 3 + 15, offsetY + 190, 80, 20), "Preview"))
                SetPreviewMaterial(MetallicMap);

            if (DiffuseMapOriginal == null && DiffuseMap == null)
                GUI.enabled = false;
            else
                GUI.enabled = true;

            if (GUI.Button(new Rect(offsetX + spacingX * 3 + 5, offsetY + 220, 50, 20), "Create"))
            {
                CloseWindows();
                FixSize();

                MetallicGuiObject.SetActive(true);
                MetallicGuiScript.NewTexture();
                MetallicGuiScript.DoStuff();
            }

            GUI.enabled = MetallicMap != null;

            if (GUI.Button(new Rect(offsetX + spacingX * 3 + 60, offsetY + 220, 45, 20), "Clear"))
            {
                ClearTexture(MapType.Metallic);
                CloseWindows();
                SetMaterialValues();
                FixSize();
            }

            GUI.enabled = true;

            #endregion

            //==============================//
            // 		Smoothness Map			//
            //==============================//

            #region Smoothness Map

            GUI.Box(new Rect(offsetX + spacingX * 4, offsetY, 110, 250), "Smoothness Map");

            if (SmoothnessMap != null)
                GUI.DrawTexture(new Rect(offsetX + spacingX * 4 + 5, offsetY + 25, 100, 100), SmoothnessMap);

            // Paste 
            if (GUI.Button(new Rect(offsetX + spacingX * 4 + 5, offsetY + 130, 20, 20), "P"))
            {
                _activeMapType = MapType.Smoothness;
                PasteFile();
            }

            GUI.enabled = SmoothnessMap != null;

            // Copy
            if (GUI.Button(new Rect(offsetX + spacingX * 4 + 30, offsetY + 130, 20, 20), "C"))
            {
                _textureToSave = SmoothnessMap;
                CopyFile();
            }

            GUI.enabled = true;

            //Open
            if (GUI.Button(new Rect(offsetX + spacingX * 4 + 60, offsetY + 130, 20, 20), "O"))
                OpenTextureFile(MapType.Smoothness);

            GUI.enabled = SmoothnessMap != null;

            // Save
            if (GUI.Button(new Rect(offsetX + spacingX * 4 + 85, offsetY + 130, 20, 20), "S"))
                SaveTextureFile(MapType.Smoothness);

            if (SmoothnessMap == null || QuicksavePathSmoothness == "")
                GUI.enabled = false;
            else
                GUI.enabled = true;

            // Quick Save
            if (GUI.Button(new Rect(offsetX + spacingX * 4 + 15, offsetY + 160, 80, 20), "Quick Save"))
            {
                _textureToSave = SmoothnessMap;
                SaveFile(QuicksavePathSmoothness);
            }

            GUI.enabled = SmoothnessMap != null;

            if (GUI.Button(new Rect(offsetX + spacingX * 4 + 15, offsetY + 190, 80, 20), "Preview"))
                SetPreviewMaterial(SmoothnessMap);

            if (DiffuseMapOriginal == null && DiffuseMap == null)
                GUI.enabled = false;
            else
                GUI.enabled = true;

            if (GUI.Button(new Rect(offsetX + spacingX * 4 + 5, offsetY + 220, 50, 20), "Create"))
            {
                CloseWindows();
                FixSize();
                SmoothnessGuiObject.SetActive(true);
                SmoothnessGuiScript.NewTexture();
                SmoothnessGuiScript.DoStuff();
            }

            GUI.enabled = SmoothnessMap != null;

            if (GUI.Button(new Rect(offsetX + spacingX * 4 + 60, offsetY + 220, 45, 20), "Clear"))
            {
                ClearTexture(MapType.Smoothness);
                CloseWindows();
                SetMaterialValues();
                FixSize();
            }

            GUI.enabled = true;

            #endregion

            //==============================//
            // 			AO Map				//
            //==============================//

            #region AO Map

            GUI.Box(new Rect(offsetX + spacingX * 5, offsetY, 110, 250), "AO Map");

            if (AoMap != null) GUI.DrawTexture(new Rect(offsetX + spacingX * 5 + 5, offsetY + 25, 100, 100), AoMap);


            // Paste 
            if (GUI.Button(new Rect(offsetX + spacingX * 5 + 5, offsetY + 130, 20, 20), "P"))
            {
                _activeMapType = MapType.Ao;
                PasteFile();
            }

            GUI.enabled = AoMap != null;

            // Copy
            if (GUI.Button(new Rect(offsetX + spacingX * 5 + 30, offsetY + 130, 20, 20), "C"))
            {
                _textureToSave = AoMap;
                CopyFile();
            }

            GUI.enabled = true;

            //Open
            if (GUI.Button(new Rect(offsetX + spacingX * 5 + 60, offsetY + 130, 20, 20), "O"))
                OpenTextureFile(MapType.Ao);

            GUI.enabled = AoMap != null;

            // Save
            if (GUI.Button(new Rect(offsetX + spacingX * 5 + 85, offsetY + 130, 20, 20), "S"))
                SaveTextureFile(MapType.Ao);

            if (AoMap == null || QuicksavePathAo == "")
                GUI.enabled = false;
            else
                GUI.enabled = true;

            // Quick Save
            if (GUI.Button(new Rect(offsetX + spacingX * 5 + 15, offsetY + 160, 80, 20), "Quick Save"))
            {
                _textureToSave = AoMap;
                SaveFile(QuicksavePathAo);
            }

            GUI.enabled = AoMap != null;

            if (GUI.Button(new Rect(offsetX + spacingX * 5 + 15, offsetY + 190, 80, 20), "Preview"))
                SetPreviewMaterial(AoMap);

            if (NormalMap == null || HeightMap == null)
                GUI.enabled = false;
            else
                GUI.enabled = true;

            if (GUI.Button(new Rect(offsetX + spacingX * 5 + 5, offsetY + 220, 50, 20), "Create"))
            {
                CloseWindows();
                FixSize();
                AoFromNormalGuiObject.SetActive(true);
                AoFromNormalGuiScript.NewTexture();
                AoFromNormalGuiScript.DoStuff();
            }

            GUI.enabled = AoMap != null;

            if (GUI.Button(new Rect(offsetX + spacingX * 5 + 60, offsetY + 220, 45, 20), "Clear"))
            {
                ClearTexture(MapType.Ao);
                CloseWindows();
                SetMaterialValues();
                FixSize();
            }

            GUI.enabled = true;

            #endregion


            //==============================//
            // 			Mask Map			//
            //==============================//

            #region Mask Map

            GUI.Box(new Rect(offsetX + spacingX * 6, offsetY, 110, 250), "Mask Map");

            if (MaskMap != null) GUI.DrawTexture(new Rect(offsetX + spacingX * 6 + 5, offsetY + 25, 100, 100), MaskMap);

            // Paste 
            if (GUI.Button(new Rect(offsetX + spacingX * 6 + 5, offsetY + 130, 20, 20), "P"))
            {
                _activeMapType = MapType.MaskMap;
                PasteFile();
            }

            GUI.enabled = MaskMap != null;

            // Copy
            if (GUI.Button(new Rect(offsetX + spacingX * 6 + 30, offsetY + 130, 20, 20), "C"))
            {
                _textureToSave = MaskMap;
                CopyFile();
            }

            GUI.enabled = true;

            //Open
            if (GUI.Button(new Rect(offsetX + spacingX * 6 + 60, offsetY + 130, 20, 20), "O"))
                OpenTextureFile(MapType.MaskMap);

            GUI.enabled = MaskMap != null;

            // Save
            if (GUI.Button(new Rect(offsetX + spacingX * 6 + 85, offsetY + 130, 20, 20), "S"))
                SaveTextureFile(MapType.MaskMap);

            if (MaskMap == null || QuicksavePathMaskMap == "")
                GUI.enabled = false;
            else
                GUI.enabled = true;

            // Quick Save
            if (GUI.Button(new Rect(offsetX + spacingX * 6 + 15, offsetY + 160, 80, 20), "Quick Save"))
            {
                _textureToSave = MaskMap;
                SaveFile(QuicksavePathMaskMap);
            }

            GUI.enabled = MaskMap != null;

            if (GUI.Button(new Rect(offsetX + spacingX * 6 + 15, offsetY + 190, 80, 20), "Preview"))
                SetPreviewMaterial(MaskMap);

            GUI.enabled = NormalMap != null;

            if (GUI.Button(new Rect(offsetX + spacingX * 6 + 5, offsetY + 220, 50, 20), "Create"))
            {
                CloseWindows();
                FixSize();
                MakeMaskMap();
                SetPreviewMaterial(MaskMap);
            }

            GUI.enabled = MaskMap != null;

            if (GUI.Button(new Rect(offsetX + spacingX * 6 + 60, offsetY + 220, 45, 20), "Clear"))
            {
                ClearTexture(MapType.MaskMap);
                CloseWindows();
                SetMaterialValues();
                FixSize();
            }

            GUI.enabled = true;

            #endregion

            //==============================//
            // 		Map Saving Options		//
            //==============================//

            #region Map Saving Options

            offsetX = offsetX + spacingX * 7;

            GUI.Box(new Rect(offsetX, offsetY, 230, 250), "Saving Options");

            GUI.Label(new Rect(offsetX + 20, offsetY + 20, 100, 25), "File Format");

            _pngSelected = GUI.Toggle(new Rect(offsetX + 30, offsetY + 60, 80, 20), _pngSelected, "PNG");
            if (_pngSelected) SetFormat(FileFormat.Png);

            _jpgSelected = GUI.Toggle(new Rect(offsetX + 30, offsetY + 80, 80, 20), _jpgSelected, "JPG");
            if (_jpgSelected) SetFormat(FileFormat.Jpg);

            _tgaSelected = GUI.Toggle(new Rect(offsetX + 30, offsetY + 100, 80, 20), _tgaSelected, "TGA");
            if (_tgaSelected) SetFormat(FileFormat.Tga);

            _exrSelected = GUI.Toggle(new Rect(offsetX + 30, offsetY + 120, 80, 20), _exrSelected, "EXR");
            if (_exrSelected) SetFormat(FileFormat.Exr);

            // Flip Normal Map Y
            GUI.enabled = NormalMap != null;

            if (GUI.Button(new Rect(offsetX + 10, offsetY + 145, 100, 25), "Flip Normal Y"))
            {
                NormalMap = TextureProcessing.FlipNormalMapY(NormalMap);
            }


            GUI.enabled = true;

            //Save Project
            if (GUI.Button(new Rect(offsetX + 10, offsetY + 180, 100, 25), "Save Project"))
            {
                const string defaultName = "baseName.mtz";
                StandaloneFileBrowser.SaveFilePanelAsync("Save Project", _lastDirectory, defaultName, "mtz",
                    SaveProjectCallback);
            }

            //Load Project
            if (GUI.Button(new Rect(offsetX + 10, offsetY + 215, 100, 25), "Load Project"))
            {
                StandaloneFileBrowser.OpenFilePanelAsync("Load Project", _lastDirectory, "mtz", false,
                    LoadProjectCallback);
            }

            #endregion

            //======================================//
            //			Property Map Settings		//
            //======================================//

            #region Property Map Settings

            GUI.Label(new Rect(offsetX + 130, offsetY + 20, 100, 25), "Property Map");

            GUI.enabled = !_propRedChoose;

            GUI.Label(new Rect(offsetX + 100, offsetY + 45, 20, 20), "R:");
            if (GUI.Button(new Rect(offsetX + 120, offsetY + 45, 100, 25), PCM2String(PropRed, "Red None")))
            {
                _propRedChoose = true;
                _propGreenChoose = false;
                _propBlueChoose = false;
            }

            GUI.enabled = !_propGreenChoose;

            GUI.Label(new Rect(offsetX + 100, offsetY + 80, 20, 20), "G:");
            if (GUI.Button(new Rect(offsetX + 120, offsetY + 80, 100, 25), PCM2String(PropGreen, "Green None")))
            {
                _propRedChoose = false;
                _propGreenChoose = true;
                _propBlueChoose = false;
            }

            GUI.enabled = !_propBlueChoose;

            GUI.Label(new Rect(offsetX + 100, offsetY + 115, 20, 20), "B:");
            if (GUI.Button(new Rect(offsetX + 120, offsetY + 115, 100, 25), PCM2String(PropBlue, "Blue None")))
            {
                _propRedChoose = false;
                _propGreenChoose = false;
                _propBlueChoose = true;
            }

            GUI.enabled = true;

            var propBoxOffsetX = offsetX + 250;
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

            if (GUI.Button(new Rect(offsetX + 120, offsetY + 150, 100, 40), "Save\r\nProperty Map"))
            {
                ProcessPropertyMap();
                SaveTextureFile(MapType.Property);
            }

            if (QuicksavePathProperty == "") GUI.enabled = false;

            if (GUI.Button(new Rect(offsetX + 120, offsetY + 200, 100, 40), "Quick Save\r\nProperty Map"))
            {
                ProcessPropertyMap();
                _textureToSave = PropertyMap;
                SaveFile(QuicksavePathProperty);
            }

            GUI.enabled = true;

            #endregion

            //==========================//
            // 		View Buttons		//
            //==========================//

            #region View Buttons

            offsetX = 430;
            offsetY = 280;

            if (GUI.Button(new Rect(offsetX, offsetY, 100, 40), "Post Process"))
            {
                PostProcessGuiObject.SetActive(!PostProcessGuiObject.activeSelf);
            }

            offsetX += 110;

            if (GUI.Button(new Rect(offsetX, offsetY, 80, 40), "Show Full\r\nMaterial"))
            {
                CloseWindows();
                FixSize();
                MaterialGuiObject.SetActive(true);
                MaterialGuiScript.Initialize();
            }

            offsetX += 90;

            if (GUI.Button(new Rect(offsetX, offsetY, 80, 40), "Next\r\nCube Map"))
            {
                _selectedCubemap += 1;
                if (_selectedCubemap >= CubeMaps.Length) _selectedCubemap = 0;

                HdriSky.hdriSky.value = CubeMaps[_selectedCubemap];
                ReflectionProbe.RenderProbe();
            }

            offsetX += 90;

            GUI.enabled = HeightMap != null;

            if (GUI.Button(new Rect(offsetX, offsetY, 60, 40), "Tile\r\nMaps"))
            {
                CloseWindows();
                FixSize();
                TilingTextureMakerGuiObject.SetActive(true);
                _tilingTextureMakerGuiScript.Initialize();
            }

            GUI.enabled = true;

            offsetX += 70;

            if (HeightMap == null && DiffuseMapOriginal == null && MetallicMap == null && SmoothnessMap == null &&
                MaskMap == null && AoMap == null)
                GUI.enabled = false;
            else
                GUI.enabled = true;

            if (GUI.Button(new Rect(offsetX, offsetY, 90, 40), "Adjust\r\nAlignment"))
            {
                CloseWindows();
                FixSize();
                AlignmentGuiScript.Initialize();
            }

            GUI.enabled = true;

            offsetX += 100;

            if (GUI.Button(new Rect(offsetX, offsetY, 120, 40), "Clear All\r\nTexture Maps")) _clearTextures = true;

            if (_clearTextures)
            {
                offsetY += 60;

                GUI.Box(new Rect(offsetX, offsetY, 120, 60), "Are You Sure?");

                if (GUI.Button(new Rect(offsetX + 10, offsetY + 30, 45, 20), "Yes"))
                {
                    _clearTextures = false;
                    ClearAllTextures();
                    CloseWindows();
                    SetMaterialValues();
                    FixSizeSize(1024.0f, 1024.0f);
                }

                if (GUI.Button(new Rect(offsetX + 65, offsetY + 30, 45, 20), "No")) _clearTextures = false;
            }

            GUI.enabled = true;

            #endregion

            #endregion
        }

        private void SaveProjectCallback(string path)
        {
            if (path.IsNullOrEmpty()) ;

            var lastBar = path.LastIndexOf(_pathChar);
            _lastDirectory = path.Substring(0, lastBar + 1);

            _saveLoadProjectScript.SaveProject(path);
        }

        private void LoadProjectCallback(string[] path)
        {
            if (path[0].IsNullOrEmpty()) return;

            var lastBar = path[0].LastIndexOf(_pathChar);
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

        #region Material

        public void SetPreviewMaterial(Texture2D textureToPreview)
        {
            CloseWindows();
            if (textureToPreview == null) return;
            FixSizeMap(textureToPreview);
            SampleMaterial.SetTexture(MainTexId, textureToPreview);
            TestObject.GetComponent<Renderer>().material = SampleMaterial;
        }

        public void SetPreviewMaterial(RenderTexture textureToPreview)
        {
            CloseWindows();
            if (textureToPreview == null) return;
            FixSizeMap(textureToPreview);
            SampleMaterial.SetTexture(MainTexId, textureToPreview);
            TestObject.GetComponent<Renderer>().material = SampleMaterial;
        }

        public void SetMaterialValues()
        {
            FullMaterialCopy.SetTexture(HeightMapId, HeightMap ? HeightMap : TextureGrey);

            if (DiffuseMap != null)
                FullMaterialCopy.SetTexture(DiffuseMapId, DiffuseMap);
            else if (DiffuseMapOriginal != null)
                FullMaterialCopy.SetTexture(DiffuseMapId, DiffuseMapOriginal);
            else
                FullMaterialCopy.SetTexture(DiffuseMapId, TextureGrey);

            FullMaterialCopy.SetTexture(NormalMapId, NormalMap ? NormalMap : TextureNormal);
            if (MaskMap)
            {
                FullMaterialCopy.SetTexture(MaskMapId, MaskMap);
            }

            TestObject.GetComponent<Renderer>().material = FullMaterialCopy;
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

        private void SaveTextureFile(MapType mapType)
        {
            _textureToSave = GetTextureToSave(mapType);
            var defaultName = "_" + mapType + ".png";
            StandaloneFileBrowser.SaveFilePanelAsync("Save Height Map", _lastDirectory, defaultName,
                _imageSaveFilter, SaveTextureFileCallback);
        }

        private void SaveTextureFileCallback(string path)
        {
            if (path.IsNullOrEmpty()) return;

            var lastBar = path.LastIndexOf(_pathChar);
            _lastDirectory = path.Substring(0, lastBar + 1);
            SaveFile(path);
        }

        private Texture2D GetTextureToSave(MapType mapType)
        {
            switch (mapType)
            {
                case MapType.Height:
                    return HeightMap;
                case MapType.Diffuse:
                    return DiffuseMap != null ? DiffuseMap : DiffuseMapOriginal;
                case MapType.DiffuseOriginal:
                    return DiffuseMapOriginal;
                case MapType.Metallic:
                    return MetallicMap;
                case MapType.Smoothness:
                    return SmoothnessMap;
                case MapType.Normal:
                    return NormalMap;
                case MapType.MaskMap:
                    return MaskMap;
                case MapType.Ao:
                    return AoMap;
                case MapType.Property:
                    return PropertyMap;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }
        }

        private void OpenTextureFile(MapType mapType)
        {
            _activeMapType = mapType;
            var title = "Open " + mapType + " Map";
            StandaloneFileBrowser.OpenFilePanelAsync(title, _lastDirectory, _imageLoadFilter, false,
                OpenTextureCallback);
        }

        private void OpenTextureCallback(string[] path)
        {
            if (path[0].IsNullOrEmpty()) return;
            var lastBar = path[0].LastIndexOf(_pathChar);
            _lastDirectory = path[0].Substring(0, lastBar + 1);
            OpenFile(path[0]);
        }

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

        private void ClearTexture(MapType mapType)
        {
            switch (mapType)
            {
                case MapType.Height:
                    if (HeightMap)
                    {
                        Destroy(HeightMap);
                        HeightMap = null;
                    }

                    if (HdHeightMap)
                    {
                        RenderTexture.ReleaseTemporary(HdHeightMap);
                    }

                    break;
                case MapType.Diffuse:
                    if (DiffuseMap)
                    {
                        Destroy(DiffuseMap);
                        DiffuseMap = null;
                    }

                    if (DiffuseMapOriginal)
                    {
                        Destroy(DiffuseMapOriginal);
                        DiffuseMapOriginal = null;
                    }

                    break;
                case MapType.Normal:
                    if (NormalMap)
                    {
                        Destroy(NormalMap);
                        NormalMap = null;
                    }

                    break;
                case MapType.Metallic:
                    if (MetallicMap)
                    {
                        Destroy(MetallicMap);
                        MetallicMap = null;
                    }

                    break;
                case MapType.Smoothness:
                    if (SmoothnessMap)
                    {
                        Destroy(SmoothnessMap);
                        SmoothnessMap = null;
                    }

                    break;
                case MapType.MaskMap:
                    if (MaskMap)
                    {
                        Destroy(MaskMap);
                        MaskMap = null;
                    }

                    break;
                case MapType.Ao:
                    if (AoMap)
                    {
                        Destroy(AoMap);
                        AoMap = null;
                    }

                    break;
                case MapType.DiffuseOriginal:
                    if (DiffuseMapOriginal)
                    {
                        Destroy(DiffuseMapOriginal);
                        DiffuseMapOriginal = null;
                    }

                    break;
                case MapType.Property:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }

            Resources.UnloadUnusedAssets();
        }

        public void ClearAllTextures()
        {
            ClearTexture(MapType.Height);
            ClearTexture(MapType.Diffuse);
            ClearTexture(MapType.Normal);
            ClearTexture(MapType.Metallic);
            ClearTexture(MapType.Smoothness);
            ClearTexture(MapType.MaskMap);
            ClearTexture(MapType.Ao);
        }

        public void SetFormat(FileFormat newFormat)
        {
            _jpgSelected = false;
            _pngSelected = false;
            _tgaSelected = false;
            _exrSelected = false;

            switch (newFormat)
            {
                case FileFormat.Jpg:
                    _jpgSelected = true;
                    break;
                case FileFormat.Png:
                    _pngSelected = true;
                    break;
                case FileFormat.Tga:
                    _tgaSelected = true;
                    break;
                case FileFormat.Exr:
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
                    SelectedFormat = FileFormat.Jpg;
                    break;
                case "png":
                    _pngSelected = true;
                    SelectedFormat = FileFormat.Png;
                    break;
                case "tga":
                    _tgaSelected = true;
                    SelectedFormat = FileFormat.Tga;
                    break;
                case "exr":
                    _exrSelected = true;
                    SelectedFormat = FileFormat.Exr;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newFormat), newFormat, null);
            }
        }

        public void SetLoadedTexture(MapType loadedTexture)
        {
            switch (loadedTexture)
            {
                case MapType.Height:
                    SetPreviewMaterial(HeightMap);
                    break;
                case MapType.Diffuse:
                    SetPreviewMaterial(DiffuseMap);
                    break;
                case MapType.DiffuseOriginal:
                    SetPreviewMaterial(DiffuseMapOriginal);
                    break;
                case MapType.Normal:
                    SetPreviewMaterial(NormalMap);
                    break;
                case MapType.Metallic:
                    SetPreviewMaterial(MetallicMap);
                    break;
                case MapType.Smoothness:
                    SetPreviewMaterial(SmoothnessMap);
                    break;
                case MapType.MaskMap:
                    SetPreviewMaterial(MaskMap);
                    break;
                case MapType.Ao:
                    SetPreviewMaterial(AoMap);
                    break;
                case MapType.Property:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(loadedTexture), loadedTexture, null);
            }

            FixSize();
        }

        #endregion

        #region Commands

        public void FlipNormalMapY()
        {
            if (NormalMap == null) return;

            NormalMap = TextureProcessing.FlipNormalMapY(NormalMap);
        }

        public void MakeMaskMap()
        {
            MaskMap = TextureProcessing.BlitMaskMap(MetallicMap, SmoothnessMap, AoMap);
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
                    SetPropertyTexture(texPrefix, HeightMap, TextureGrey);
                    break;
                case PropChannelMap.Metallic:
                    SetPropertyTexture(texPrefix, MetallicMap, TextureGrey);
                    break;
                case PropChannelMap.Smoothness:
                    SetPropertyTexture(texPrefix, SmoothnessMap, TextureGrey);
                    break;
                case PropChannelMap.Ao:
                    SetPropertyTexture(texPrefix, AoMap, TextureGrey);
                    break;
                case PropChannelMap.MaskMap:
                    SetPropertyTexture(texPrefix, MaskMap, TextureGrey);
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

            var size = GetSize();
            var tempMap = RenderTexture.GetTemporary((int) size.x, (int) size.y, 0, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Default);
            Graphics.Blit(MetallicMap, tempMap, _propertyCompMaterial, 0);
            RenderTexture.active = tempMap;

            if (PropertyMap != null)
            {
                Destroy(PropertyMap);
                PropertyMap = null;
            }

            PropertyMap = new Texture2D(tempMap.width, tempMap.height, TextureFormat.RGB24, false);
            PropertyMap.ReadPixels(new Rect(0, 0, tempMap.width, tempMap.height), 0, 0);
            PropertyMap.Apply();

            RenderTexture.ReleaseTemporary(tempMap);
            // ReSharper disable once RedundantAssignment
            tempMap = null;
        }

        #endregion

        //==================================================//
        //					Project Saving					//
        //==================================================//

        #region Project Saving

        private void SaveFile(string pathToFile)
        {
            _saveLoadProjectScript.SaveFile(pathToFile, _textureToSave);
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void CopyFile()
        {
            _saveLoadProjectScript.CopyFile(_textureToSave);
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void PasteFile()
        {
            ClearTexture(_activeMapType);
            _saveLoadProjectScript.PasteFile(_activeMapType);
        }

        private void OpenFile(string pathToFile)
        {
            if (pathToFile == null) return;

            // clear the existing texture we are loading
            ClearTexture(_activeMapType);

            StartCoroutine(_saveLoadProjectScript.LoadTexture(_activeMapType, pathToFile));
        }

        #endregion

        //==================================================//
        //			Fix the size of the test model			//
        //==================================================//

        #region Fix Size

        private Vector2 GetSize()
        {
            Texture2D mapToUse = null;

            var size = new Vector2(1024, 1024);

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

        private void FixSizeMap(Texture mapToUse)
        {
            FixSizeSize(mapToUse.width, mapToUse.height);
        }

        private void FixSizeMap(RenderTexture mapToUse)
        {
            FixSizeSize(mapToUse.width, mapToUse.height);
        }

        private void FixSizeSize(float width, float height)
        {
            var testObjectScale = new Vector3(1, 1, 1);
            const float area = 1.0f;

            testObjectScale.x = width / height;

            var newArea = testObjectScale.x * testObjectScale.y;
            var areaScale = Mathf.Sqrt(area / newArea);

            testObjectScale.x *= areaScale;
            testObjectScale.y *= areaScale;

            if (TestObject.transform.parent && TestObject.GetComponentInParent<MeshFilter>() != null)
            {
                TestObject.transform.parent.localScale.Scale(testObjectScale);
            }
            else
            {
                TestObject.transform.localScale.Scale(testObjectScale);
            }
        }

        #endregion


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
    }
}