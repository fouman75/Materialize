#region

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Materialize.Gui;
using Materialize.Settings;
using StandaloneFileBrowser;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;
using Utility;
using Logger = Utility.Logger;

#endregion

namespace Materialize.General
{
    public class ProgramManager : MonoBehaviour
    {
        public static ProgramManager Instance;
        public static Vector2 GuiReferenceSize = new Vector2(1440, 810);
        private int _windowId;
        public bool ApplicationIsQuitting;

        [HideInInspector] public int DefaultFrameRate = 30;

        public int ForcedFrameRate = 30;
        public bool ForceFrameRate;
        public Vector2 GuiScale = new Vector2(1, 1);

        public MessagePanel MessagePanelObject;
        public ObjectZoomPanRotate ObjectHandler;
        public Volume PostProcessingVolume;
        [HideInInspector] public ProgramSettings ProgramSettings;
        public HDRenderPipeline RenderPipeline;
        public Volume SceneVolume;

        #region Settings

        public Cubemap StartCubeMap;

        #endregion


        private ProgramManager()
        {
        }

        public static char PathChar => Path.DirectorySeparatorChar;

        public int GetWindowId => _windowId++;

        public static bool IsLocked { get; private set; }

        public static bool Lock()
        {
            if (IsLocked) return false;

            IsLocked = true;
            return true;
        }

        public static void Unlock()
        {
            IsLocked = false;
        }

        private void Awake()
        {
            Instance = this;


            GuiScale = new Vector2(Screen.width / GuiReferenceSize.x, Screen.height / GuiReferenceSize.y);
            MessagePanelObject.gameObject.SetActive(true);

            if (!Application.isEditor)
            {
                var monitorFrameRate = Screen.resolutions[Screen.resolutions.Length - 1].refreshRate;
                DefaultFrameRate = ForceFrameRate ? ForcedFrameRate : monitorFrameRate;
            }
        }

        private IEnumerator Start()
        {
            _probes = FindObjectsOfType<ReflectionProbe>();
            Logger.Log("Starting " + name);
            ControlsGuiObject = FindMonoBehaviour<ControlsGui>().gameObject;
            MainGuiObject = FindMonoBehaviour<MainGui>().gameObject;
            CommandListExecutorObject = FindMonoBehaviour<CommandListExecutor>().gameObject;
            MaterialGuiObject = FindMonoBehaviour<MaterialGui>().gameObject;

            ActivateObjects();
            yield return StartCoroutine(GetHdrpCoroutine());

            LoadSettings();

            StartCoroutine(SlowUpdate());
        }

        private T FindMonoBehaviour<T>() where T : MonoBehaviour
        {
            foreach (var sceneObject in SceneObjects)
            {
                var behaviour = sceneObject.GetComponent<T>();
                if (behaviour != null) return behaviour;
            }

            return null;
        }

        private void Update()
        {
            GuiScale = new Vector2(Screen.width / GuiReferenceSize.x, Screen.height / GuiReferenceSize.y);
        }

        private IEnumerator SlowUpdate()
        {
            while (!ApplicationIsQuitting)
            {
                if (Application.targetFrameRate != ProgramSettings.FrameRate && ProgramSettings.FrameRate != 0)
                {
                    Logger.Log("Setting FrameRate to " + ProgramSettings.FrameRate);
                    Application.targetFrameRate = ProgramSettings.FrameRate;
                }

                ObjectHandler.AllowHide = ProgramSettings.HideUiOnRotate;
                TextureManager.Instance.Hdr = ProgramSettings.HDR;

                yield return new WaitForSeconds(0.5f);
            }
        }


        private void ActivateObjects()
        {
            TestObject.SetActive(true);
            MainGuiObject.SetActive(true);
            ControlsGuiObject.SetActive(true);
            MaterialGuiObject.SetActive(false);
            CommandListExecutorObject.SetActive(true);
        }

        private IEnumerator GetHdrpCoroutine()
        {
            RenderPipeline = RenderPipelineManager.currentPipeline as HDRenderPipeline;
            var maxTries = 100;
            RenderPipeline = null;
            while (RenderPipeline == null)
            {
                RenderPipeline = RenderPipelineManager.currentPipeline as HDRenderPipeline;
                yield return new WaitForSeconds(0.1f);
                maxTries--;

                if (maxTries != 0) continue;
                yield break;
            }
        }

        public IEnumerator RenderProbe()
        {
            foreach (var probe in Instance._probes)
            {
                var isRealTime = probe.mode == ReflectionProbeMode.Realtime;
                var hdProbe = probe.GetComponent<HDAdditionalReflectionData>();
                var needsRefresh = hdProbe.realtimeMode == ProbeSettings.RealtimeMode.OnDemand;

                if (!isRealTime || !needsRefresh) continue;

                Logger.Log("Refreshing probe " + probe.name);
                probe.RequestRenderNextUpdate();
            }

            yield return StartCoroutine(GetHdrpCoroutine());

            RenderPipeline.RequestSkyEnvironmentUpdate();
        }

        private void OnApplicationQuit()
        {
            ApplicationIsQuitting = true;
            PrefsManager.Save();
        }

        private void LoadSettings()
        {
            ProgramSettings = PrefsManager.ProgramSettings;

            if (ProgramSettings == null) InitializeSettings();

            SetSettings();
        }

        private void InitializeSettings()
        {
            Logger.Log("Initializing Program Settings");
            ProgramSettings = new ProgramSettings
            {
                HideUiOnRotate = ObjectHandler.AllowHide,
                FrameRate = DefaultFrameRate,
                HDR = TextureManager.Instance.Hdr,
                NormalMapMaxStyle = true,
                NormalMapMayaStyle = false,
                PostProcessEnabled = true,
                PropRed = ProgramEnums.PropChannelMap.None,
                PropGreen = ProgramEnums.PropChannelMap.None,
                PropBlue = ProgramEnums.PropChannelMap.None,
                FileFormat = ProgramEnums.FileFormat.Png
            };
            PrefsManager.ProgramSettings = ProgramSettings;
            PrefsManager.Save();
        }

        public void SetSettings()
        {
            TextureManager.Instance.FlipNormalY = ProgramSettings.NormalMapMayaStyle;

            if (ProgramSettings.PostProcessEnabled)
                PostProcessGui.PostProcessOn();
            else
                PostProcessGui.PostProcessOff();

            var mainGui = MainGui.Instance;
            mainGui.PropRed = ProgramSettings.PropRed;
            mainGui.PropGreen = ProgramSettings.PropGreen;
            mainGui.PropBlue = ProgramSettings.PropBlue;

            mainGui.SetFormat(ProgramSettings.FileFormat);
        }

        public void SaveSettings()
        {
            PrefsManager.ProgramSettings = ProgramSettings;
            PrefsManager.Save();
        }


        #region Gui Objects

        public List<GameObject> SceneObjects = new List<GameObject>();

        [HideInInspector] [Header("Gui Objects")]
        public GameObject CommandListExecutorObject;

        [HideInInspector] public GameObject ControlsGuiObject;
        [HideInInspector] public GameObject MainGuiObject;
        [HideInInspector] public GameObject MaterialGuiObject;
        public GameObject TestObject;

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

        private ReflectionProbe[] _probes;

        #endregion
    }
}