using Gui;
using StandaloneFileBrowser;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;

namespace General
{
    public class ProgramManager : MonoBehaviour
    {
        public static ProgramManager Instance;
        public char PathChar { get; private set; }
        private const string LastPathKey = nameof(LastPathKey);
        [HideInInspector] public string LastPath;
        private static readonly int GamaCorrectionId = Shader.PropertyToID("_GamaCorrection");
        public HDRenderPipeline RenderPipeline;

        //Nao remover, alguns shaders dependem disso
        private const float GamaCorrection = 1f;

        #region Settings

        [Header("Settings")] public int TargetFps = 30;
        public Cubemap StartCubeMap;

        #endregion

        #region Gui Objects

        [Header("Gui Objects")] public GameObject CommandListExecutorObject;
        public GameObject ControlsGuiObject;
        public GameObject MainGuiObject;
        public GameObject SettingsGuiObject;
        public GameObject TestObject;
        public GameObject MaterialGuiObject;

        #endregion

        #region Suported Formats

        public static readonly string[] LoadFormats =
        {
            "png", "jpg", "jpeg", "tga", "bmp", "exr"
        };

        public static readonly ExtensionFilter[] ImageLoadFilter =
        {
            new ExtensionFilter("Image Files", LoadFormats)
        };

        public static readonly string[] SaveFormats =
        {
            "png", "jpg", "jpeg", "tga", "exr"
        };

        public static readonly ExtensionFilter[] ImageSaveFilter =
        {
            new ExtensionFilter("Image Files", SaveFormats)
        };

        #endregion


        private ProgramManager()
        {
        }

        private void Awake()
        {
            RenderPipeline = UnityEngine.Rendering.RenderPipelineManager.currentPipeline as HDRenderPipeline;
            Instance = this;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFps;
            Shader.SetGlobalFloat(GamaCorrectionId, GamaCorrection);
            LastPath = PlayerPrefs.HasKey(LastPathKey) ? PlayerPrefs.GetString(LastPathKey) : null;

            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer)
                PathChar = '\\';
            else PathChar = '/';
        }

        private void Start()
        {
            ActivateObjects();
            RenderPipeline.RequestSkyEnvironmentUpdate();
            
        }

        void Update()
        {
            if (Application.targetFrameRate != TargetFps)
            {
                Application.targetFrameRate = TargetFps;
            }
        }

        private void ActivateObjects()
        {
            TestObject.SetActive(true);
            MainGuiObject.SetActive(true);
            SettingsGuiObject.SetActive(true);
            ControlsGuiObject.SetActive(true);
            CommandListExecutorObject.SetActive(true);
        }

        public void OpenFullMaterial()
        {
            MainGui.Instance.CloseWindows();
            TextureManager.Instance.FixSize();
            TextureManager.Instance.SetFullMaterial();
            MaterialGuiObject.SetActive(true);
            MaterialGuiObject.GetComponent<MaterialGui>().Initialize();
        }
    }
}