#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Gui;
using StandaloneFileBrowser;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;

#endregion

namespace General
{
    public class ProgramManager : MonoBehaviour
    {
        private const string LastPathKey = nameof(LastPathKey);
        public const int DefaultFrameRate = 30;
        public static ProgramManager Instance;
        public static Vector2 GuiReferenceSize = new Vector2(1440, 810);
        private int _windowId;
        public bool ApplicationIsQuitting;
        public int DesiredFrameRate;
        public Vector2 GuiScale = new Vector2(1, 1);
        [HideInInspector] public string LastPath;
        public Light MainLight;
        public MessagePanel MessagePanelObject;
        public HDRenderPipeline RenderPipeline;
        public HDRenderPipelineAsset HighQualityAsset;
        public HDRenderPipelineAsset MediumQualityAsset;
        public HDRenderPipelineAsset LowQualityAsset;
        public ProgramEnums.GraphicsQuality GraphicsQuality;

        #region Settings

        public Cubemap StartCubeMap;

        #endregion


        private ProgramManager()
        {
        }

        public char PathChar { get; private set; }

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
            LastPath = PlayerPrefs.HasKey(LastPathKey) ? PlayerPrefs.GetString(LastPathKey) : null;

            PathChar = Path.DirectorySeparatorChar;

            GuiScale = new Vector2(Screen.width / GuiReferenceSize.x, Screen.height / GuiReferenceSize.y);
            MessagePanelObject.gameObject.SetActive(true);

            StartCoroutine(SetScreen(ProgramEnums.ScreenMode.Windowed));
        }

        private IEnumerator Start()
        {
            Logger.Log("Starting " + name);
            ControlsGuiObject = FindMonoBehaviour<ControlsGui>().gameObject;
            MainGuiObject = FindMonoBehaviour<MainGui>().gameObject;
            SettingsGuiObject = FindMonoBehaviour<SettingsGui>().gameObject;
            CommandListExecutorObject = FindMonoBehaviour<CommandListExecutor>().gameObject;
            MaterialGuiObject = FindMonoBehaviour<MaterialGui>().gameObject;

            ActivateObjects();
            StartCoroutine(SlowUpdate());
            yield return StartCoroutine(GetHdrpCoroutine());
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
                if (Application.targetFrameRate != DesiredFrameRate && DesiredFrameRate != 0)
                {
                    Logger.Log("Setting FrameRate to " + DesiredFrameRate);
                    Application.targetFrameRate = DesiredFrameRate;
                }

                if (LastPath != null) PlayerPrefs.SetString(LastPathKey, LastPath);

                yield return StartCoroutine(UpdateQuality());

                yield return new WaitForSeconds(0.2f);
            }
        }

        private IEnumerator UpdateQuality()
        {
            HDRenderPipelineAsset hdRenderPipelineAsset;
            switch (GraphicsQuality)
            {
                case ProgramEnums.GraphicsQuality.High:
                    QualitySettings.SetQualityLevel(2);
                    hdRenderPipelineAsset = HighQualityAsset;
                    break;
                case ProgramEnums.GraphicsQuality.Medium:
                    QualitySettings.SetQualityLevel(1);
                    hdRenderPipelineAsset = MediumQualityAsset;
                    break;
                case ProgramEnums.GraphicsQuality.Low:
                    QualitySettings.SetQualityLevel(0);
                    hdRenderPipelineAsset = LowQualityAsset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (GraphicsSettings.renderPipelineAsset == hdRenderPipelineAsset) yield break;

            switch (GraphicsQuality)
            {
                case ProgramEnums.GraphicsQuality.High:
                    QualitySettings.SetQualityLevel(2);
                    break;
                case ProgramEnums.GraphicsQuality.Medium:
                    QualitySettings.SetQualityLevel(1);
                    break;
                case ProgramEnums.GraphicsQuality.Low:
                    QualitySettings.SetQualityLevel(0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            yield return new WaitForSeconds(0.3f);

            GraphicsSettings.renderPipelineAsset = hdRenderPipelineAsset;

            yield return new WaitForSeconds(0.8f);
        }

        private void ActivateObjects()
        {
            TestObject.SetActive(true);
            MainGuiObject.SetActive(true);
            SettingsGuiObject.SetActive(true);
            ControlsGuiObject.SetActive(true);
            MaterialGuiObject.SetActive(false);
            CommandListExecutorObject.SetActive(true);
        }

        private IEnumerator GetHdrpCoroutine()
        {
            RenderPipeline = RenderPipelineManager.currentPipeline as HDRenderPipeline;
            var maxTries = 10;
            while (RenderPipeline == null)
            {
                RenderPipeline = RenderPipelineManager.currentPipeline as HDRenderPipeline;
                yield return new WaitForSeconds(0.1f);
                maxTries--;

                if (maxTries != 0) continue;
                yield break;
            }
        }

        public static void RenderProbe()
        {
            var probes = FindObjectsOfType<ReflectionProbe>();

            foreach (var probe in probes)
            {
                var isRealTime = probe.mode == ReflectionProbeMode.Realtime;
                var hdProbe = probe.GetComponent<HDAdditionalReflectionData>();
                var needsRefresh = hdProbe.realtimeMode == ProbeSettings.RealtimeMode.OnDemand;

                if (!isRealTime || !needsRefresh) continue;

                Logger.Log("Refreshing probe " + probe.name);
                probe.RequestRenderNextUpdate();
            }
        }

        private void OnApplicationQuit()
        {
            ApplicationIsQuitting = true;
        }

        public static IEnumerator SetScreen(ProgramEnums.ScreenMode screenMode)
        {
            if (Application.isEditor) yield break;

            switch (screenMode)
            {
                case ProgramEnums.ScreenMode.FullScreen:
                    var fsRes = Screen.resolutions;
                    var highRes = fsRes[fsRes.Length - 1];
                    Screen.SetResolution(highRes.width, highRes.height, FullScreenMode.ExclusiveFullScreen);
                    break;
                case ProgramEnums.ScreenMode.Windowed:
                    var res = GetSecondHighestResolution();

                    Screen.SetResolution(res.width, res.height, FullScreenMode.Windowed);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(screenMode), screenMode, null);
            }

//            yield return new WaitForSeconds(0.3f);

            GraphicsSettings.renderPipelineAsset = GraphicsSettings.renderPipelineAsset;
        }

        private static Resolution GetSecondHighestResolution()
        {
            var winRes = Screen.resolutions;
            var nativeRes = winRes[winRes.Length - 1];
            var res = nativeRes;
            var nativeAspect = 1.0f * nativeRes.width / nativeRes.height;
            for (var i = winRes.Length - 1; i >= 0; i--)
            {
                var aspectRatio = 1.0f * winRes[i].width / winRes[i].height;

                if (!(Mathf.Abs(aspectRatio - nativeAspect) < 0.001f)) continue;

                res = winRes[i];
                break;
            }

            return res;
        }

        #region Gui Objects

        public List<GameObject> SceneObjects = new List<GameObject>();

        [HideInInspector] [Header("Gui Objects")]
        public GameObject CommandListExecutorObject;

        [HideInInspector] public GameObject ControlsGuiObject;
        [HideInInspector] public GameObject MainGuiObject;
        [HideInInspector] public GameObject SettingsGuiObject;
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

        #endregion
    }
}