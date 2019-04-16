#region

using System.IO;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Materialize.General;
using Materialize.Settings;
using UnityEngine;
using Utility;
using Logger = Utility.Logger;

#endregion


namespace Materialize.Gui
{
    public class SettingsGui : MonoBehaviour, IHideable
    {
        private const string SettingsKey = "Settings";
        private static readonly int FlipNormalY = Shader.PropertyToID("_FlipNormalY");

        public static SettingsGui Instance;

        private readonly Rect _windowRect = new Rect(ProgramManager.GuiReferenceSize.x - 300,
            ProgramManager.GuiReferenceSize.y - 420, 280, 370);

        private bool _invalidSettings;
        private int _windowId;

        private bool _windowOpen;
        public ObjectZoomPanRotate ObjectHandler;
        public PostProcessGui PostProcessGui;
        [HideInInspector] public ProgramSettings ProgramSettings = new ProgramSettings();

        public bool Hide { get; set; }

        private void Awake()
        {
            ProgramManager.Instance.SceneObjects.Add(gameObject);
        }

        private void Start()
        {
            Instance = this;

            LoadSettings();
            _windowId = ProgramManager.Instance.GetWindowId;
        }

        private void Update()
        {
            ObjectHandler.AllowHide = ProgramSettings.HideUiOnRotate;
            ProgramManager.Instance.DesiredFrameRate = ProgramSettings.FrameRate;
            PersistentSettings.Instance.ChangeGraphicsQuality(ProgramSettings.GraphicsQuality.ToString());
            TextureManager.Instance.Hdr = ProgramSettings.HDR;
        }

        private void LoadSettings()
        {
            if (PlayerPrefs.HasKey(SettingsKey))
            {
                var set = PlayerPrefs.GetString(SettingsKey);
                var serializer = new XmlSerializer(typeof(ProgramSettings));
                using (TextReader sr = new StringReader(set))
                {
                    serializer.UnknownNode += Serializer_UnknownNode;
                    serializer.UnknownAttribute += Serializer_UnknownAttribute;
                    serializer.UnknownElement += Serializer_UnknownElement;
                    ProgramSettings = serializer.Deserialize(sr) as ProgramSettings;
                }
            }
            else
            {
                _invalidSettings = true;
            }

            if (_invalidSettings) InitializeSettings();

            SetSettings();
        }

        private void InitializeSettings()
        {
            Logger.Log("Initializing Program Settings");
            ProgramSettings.HideUiOnRotate = ObjectHandler.AllowHide;
            ProgramSettings.FrameRate = ProgramManager.Instance.DefaultFrameRate;
            ProgramSettings.GraphicsQuality = ProgramEnums.GraphicsQuality.Medium;
            ProgramSettings.HDR = TextureManager.Instance.Hdr;
            ProgramSettings.NormalMapMaxStyle = true;
            ProgramSettings.NormalMapMayaStyle = false;
            ProgramSettings.PostProcessEnabled = true;
            ProgramSettings.PropRed = ProgramEnums.PropChannelMap.None;
            ProgramSettings.PropGreen = ProgramEnums.PropChannelMap.None;
            ProgramSettings.PropBlue = ProgramEnums.PropChannelMap.None;
            ProgramSettings.FileFormat = ProgramEnums.FileFormat.Png;
            SaveSettings();
        }


        private void Serializer_UnknownNode
            (object sender, XmlNodeEventArgs e)
        {
            Logger.LogError($"Unknown Node: {e.Name}\te.Text ");
            _invalidSettings = true;
        }

        private void Serializer_UnknownAttribute
            (object sender, XmlAttributeEventArgs e)
        {
            var attr = e.Attr;
            Logger.LogError($"Unknown attribute {attr.Name} + ='{attr.Value}'");
            _invalidSettings = true;
        }

        private void Serializer_UnknownElement
            (object sender, XmlElementEventArgs xmlElementEventArgs)
        {
            var element = xmlElementEventArgs.Element;
            Logger.LogError($"Unknown element {element.Name} + ='{element.Value}'");
            _invalidSettings = true;
        }

        private void SaveSettings()
        {
            var serializer = new XmlSerializer(typeof(ProgramSettings));
            using (TextWriter sw = new StringWriter())
            {
                serializer.Serialize(sw, ProgramSettings);
                PlayerPrefs.SetString(SettingsKey, sw.ToString());
            }
        }

        private void SetNormalMode()
        {
            var flipNormalY = 0;
            if (ProgramSettings.NormalMapMayaStyle) flipNormalY = 1;

            Shader.SetGlobalInt(FlipNormalY, flipNormalY);
        }

        public void SetSettings()
        {
            SetNormalMode();

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


        private void DoMyWindow(int windowId)
        {
            const int offsetX = 10;
            var offsetY = 20;

            ProgramSettings.HideUiOnRotate = GUI.Toggle(new Rect(offsetX, offsetY, 150, 30),
                ProgramSettings.HideUiOnRotate,
                "Hide UI On Rotate");

            offsetY += 30;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Normal Map Style");

            offsetY += 20;

            ProgramSettings.NormalMapMaxStyle =
                GUI.Toggle(new Rect(offsetX, offsetY, 100, 30), ProgramSettings.NormalMapMaxStyle, " Max Style");
            ProgramSettings.NormalMapMayaStyle = !ProgramSettings.NormalMapMaxStyle;


            ProgramSettings.NormalMapMayaStyle = GUI.Toggle(new Rect(offsetX + 100, offsetY, 100, 30),
                ProgramSettings.NormalMapMayaStyle,
                " Maya Style");
            ProgramSettings.NormalMapMaxStyle = !ProgramSettings.NormalMapMayaStyle;

            offsetY += 30;

            ProgramSettings.PostProcessEnabled = GUI.Toggle(new Rect(offsetX, offsetY, 280, 30),
                ProgramSettings.PostProcessEnabled,
                " Enable Post Process By Default");

            offsetY += 30;
            GUI.Label(new Rect(offsetX + 70, offsetY, 250, 30), "Limit Frame Rate");

            offsetY += 30;

            if (GUI.Button(new Rect(offsetX + 40, offsetY, 30, 30), "30")) ProgramSettings.FrameRate = 30;

            if (GUI.Button(new Rect(offsetX + 80, offsetY, 30, 30), "60")) ProgramSettings.FrameRate = 60;

            if (GUI.Button(new Rect(offsetX + 120, offsetY, 30, 30), "120")) ProgramSettings.FrameRate = 120;

            if (GUI.Button(new Rect(offsetX + 160, offsetY, 40, 30), "None")) ProgramSettings.FrameRate = -1;

            offsetY += 40;

            GUI.Label(new Rect(offsetX + 70, offsetY, 250, 30), "Graphics Quality");

            offsetY += 30;

            var isLow = ProgramSettings.GraphicsQuality == ProgramEnums.GraphicsQuality.Low;
            if (GUI.Toggle(new Rect(offsetX + 20, offsetY, 50, 30), isLow, "Low"))
                ProgramSettings.GraphicsQuality = ProgramEnums.GraphicsQuality.Low;

            var isMedium = ProgramSettings.GraphicsQuality == ProgramEnums.GraphicsQuality.Medium;
            if (GUI.Toggle(new Rect(offsetX + 100, offsetY, 50, 30), isMedium, "Medium"))
                ProgramSettings.GraphicsQuality = ProgramEnums.GraphicsQuality.Medium;

            var isHigh = ProgramSettings.GraphicsQuality == ProgramEnums.GraphicsQuality.High;
            if (GUI.Toggle(new Rect(offsetX + 180, offsetY, 50, 30), isHigh, "High"))
                ProgramSettings.GraphicsQuality = ProgramEnums.GraphicsQuality.High;

            offsetY += 40;


            if (GUI.Button(new Rect(offsetX, offsetY, 260, 25), "Set Default Property Map Channels"))
            {
                ProgramSettings.PropRed = MainGui.Instance.PropRed;
                ProgramSettings.PropGreen = MainGui.Instance.PropGreen;
                ProgramSettings.PropBlue = MainGui.Instance.PropBlue;
            }

            offsetY += 30;

            if (GUI.Button(new Rect(offsetX, offsetY, 260, 25), "Set Default File Format"))
                ProgramSettings.FileFormat = ProgramEnums.FileFormat.Png;

            offsetY += 30;

            if (GUI.Button(new Rect(offsetX + 140, offsetY, 120, 30), "Save and Close"))
            {
                SaveSettings();
                SetNormalMode();
                _windowOpen = false;
            }
        }

        private void OnGUI()
        {
            if (Hide) return;
            if (_windowOpen) MainGui.MakeScaledWindow(_windowRect, _windowId, DoMyWindow, "Setting and Preferences");
        }

        [UsedImplicitly]
        public void SettingsButtonCallBack()
        {
            if (_windowOpen)
            {
                SaveSettings();
                _windowOpen = false;
            }
            else
            {
                _windowOpen = true;
            }
        }
    }
}