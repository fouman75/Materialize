#region

using System.IO;
using System.Xml.Serialization;
using General;
using Settings;
using UnityEngine;

#endregion


namespace Gui
{
    public class SettingsGui : MonoBehaviour, IHideable
    {
        private const string SettingsKey = "Settings";
        private static readonly int FlipNormalY = Shader.PropertyToID("_FlipNormalY");

        public static SettingsGui Instance;
        public PostProcessGui PostProcessGui;
        public ObjectZoomPanRotate ObjectHandler;
        [HideInInspector] public ProgramSettings ProgramSettings = new ProgramSettings();

        private bool _windowOpen;

        private readonly Rect _windowRect = new Rect(ProgramManager.GuiReferenceSize.x - 300,
            ProgramManager.GuiReferenceSize.y - 400, 280, 300);

        private bool _invalidSettings;
        private int _windowId;

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
            TextureManager.Instance.Hdr = ProgramSettings.HDR;
        }

        public void LoadSettings()
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
            else _invalidSettings = true;

            if (_invalidSettings) InitializeSettings();

            SetSettings();
        }

        private void InitializeSettings()
        {
            General.Logger.Log("Initializing Program Settings");
            ProgramSettings.HideUiOnRotate = ObjectHandler.AllowHide;
            ProgramSettings.FrameRate = ProgramManager.DefaultFrameRate;
            ProgramSettings.HDR = TextureManager.Instance.Hdr;
            ProgramSettings.NormalMapMaxStyle = true;
            ProgramSettings.NormalMapMayaStyle = false;
            ProgramSettings.PostProcessEnabled = true;
            ProgramSettings.PropRed = PropChannelMap.None;
            ProgramSettings.PropGreen = PropChannelMap.None;
            ProgramSettings.PropBlue = PropChannelMap.None;
            ProgramSettings.FileFormat = ProgramEnums.FileFormat.Png;
            SaveSettings();
        }


        private void Serializer_UnknownNode
            (object sender, XmlNodeEventArgs e)
        {
            General.Logger.LogError($"Unknown Node: {e.Name}\te.Text ");
            _invalidSettings = true;
        }

        private void Serializer_UnknownAttribute
            (object sender, XmlAttributeEventArgs e)
        {
            var attr = e.Attr;
            General.Logger.LogError($"Unknown attribute {attr.Name} + ='{attr.Value}'");
            _invalidSettings = true;
        }

        private void Serializer_UnknownElement
            (object sender, XmlElementEventArgs xmlElementEventArgs)
        {
            var element = xmlElementEventArgs.Element;
            General.Logger.LogError($"Unknown element {element.Name} + ='{element.Value}'");
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

            if (GUI.Button(new Rect(offsetX + 40, offsetY, 30, 30), "30"))
            {
                ProgramSettings.FrameRate = 30;
            }

            if (GUI.Button(new Rect(offsetX + 80, offsetY, 30, 30), "60"))
            {
                ProgramSettings.FrameRate = 60;
            }

            if (GUI.Button(new Rect(offsetX + 120, offsetY, 30, 30), "120"))
            {
                ProgramSettings.FrameRate = 120;
            }

            if (GUI.Button(new Rect(offsetX + 160, offsetY, 40, 30), "None"))
            {
                ProgramSettings.FrameRate = -1;
            }

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

        public bool Hide { get; set; }
    }
}