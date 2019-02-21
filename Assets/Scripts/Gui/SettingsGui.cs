#region

using System.IO;
using System.Xml.Serialization;
using General;
using Settings;
using UnityEngine;

#endregion


namespace Gui
{
    public class SettingsGui : MonoBehaviour
    {
        private const string SettingsKey = "Settings";
        public static SettingsGui Instance;
        private static readonly int FlipNormalY = Shader.PropertyToID("_FlipNormalY");
        private bool _windowOpen;

        private Rect _windowRect = new Rect(Screen.width - 300, Screen.height - 320, 280, 600);
        public PostProcessGui PostProcessGui;
        [HideInInspector] public ProgramSettings ProgramSettings = new ProgramSettings();
        private bool _invalidSettings;

        private void Awake()
        {
            ProgramManager.Instance.SceneObjects.Add(gameObject);
        }

        private void Start()
        {
            Instance = this;

            LoadSettings();
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
            Debug.Log("Initializing Program Settings");
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
            Debug.LogError($"Unknown Node: {e.Name}\te.Text ");
            _invalidSettings = true;
        }

        private void Serializer_UnknownAttribute
            (object sender, XmlAttributeEventArgs e)
        {
            var attr = e.Attr;
            Debug.LogError($"Unknown attribute {attr.Name} + ='{attr.Value}'");
            _invalidSettings = true;
        }

        private void Serializer_UnknownElement
            (object sender, XmlElementEventArgs xmlElementEventArgs)
        {
            var element = xmlElementEventArgs.Element;
            Debug.LogError($"Unknown element {element.Name} + ='{element.Value}'");
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
            var offsetY = 30;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Normal Map Style");

            offsetY += 30;

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

            if (GUI.Button(new Rect(offsetX, offsetY, 260, 25), "Set Default Property Map Channels"))
            {
                ProgramSettings.PropRed = MainGui.Instance.PropRed;
                ProgramSettings.PropGreen = MainGui.Instance.PropGreen;
                ProgramSettings.PropBlue = MainGui.Instance.PropBlue;
            }

            offsetY += 30;

            if (GUI.Button(new Rect(offsetX, offsetY, 260, 25), "Set Default File Format"))
                ProgramSettings.FileFormat = ProgramEnums.FileFormat.Png;

            offsetY += 40;

            if (GUI.Button(new Rect(offsetX + 140, offsetY, 120, 30), "Save and Close"))
            {
                SaveSettings();
                SetNormalMode();
                _windowOpen = false;
            }

            GUI.DragWindow();
        }

        private void OnGUI()
        {
            _windowRect = new Rect(Screen.width - 300, Screen.height - 320, 280, 230);
            var pivotPoint = new Vector2(_windowRect.x, _windowRect.y);
            GUIUtility.ScaleAroundPivot(ProgramManager.Instance.GuiScale, pivotPoint);

            if (_windowOpen) _windowRect = GUI.Window(20, _windowRect, DoMyWindow, "Setting and Preferences");
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
    }
}