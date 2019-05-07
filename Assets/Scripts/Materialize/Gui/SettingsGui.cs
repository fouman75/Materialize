#region

using JetBrains.Annotations;
using Materialize.General;
using UnityEngine;

#endregion


namespace Materialize.Gui
{
    public class SettingsGui : MonoBehaviour, IHideable
    {
        private readonly Rect _windowRect = new Rect(ProgramManager.GuiReferenceSize.x - 300,
            ProgramManager.GuiReferenceSize.y - 420, 280, 370);

        private int _windowId;

        public bool Hide { get; set; }

        private void Start()
        {
            _windowId = ProgramManager.Instance.GetWindowId;
            CachedGraphicsQuality = PrefsManager.GraphicsQuality;
        }

        private ProgramEnums.GraphicsQuality CachedGraphicsQuality { get; set; }

        private void DoMyWindow(int windowId)
        {
            var programSettings = ProgramManager.Instance.ProgramSettings;
            const int offsetX = 10;
            var offsetY = 20;

            programSettings.HideUiOnRotate = GUI.Toggle(new Rect(offsetX, offsetY, 150, 30),
                programSettings.HideUiOnRotate,
                "Hide UI On Rotate");

            offsetY += 30;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Normal Map Style");

            offsetY += 20;

            programSettings.NormalMapMaxStyle =
                GUI.Toggle(new Rect(offsetX, offsetY, 100, 30), programSettings.NormalMapMaxStyle, " Max Style");
            programSettings.NormalMapMayaStyle = !programSettings.NormalMapMaxStyle;


            programSettings.NormalMapMayaStyle = GUI.Toggle(new Rect(offsetX + 100, offsetY, 100, 30),
                programSettings.NormalMapMayaStyle,
                " Maya Style");
            programSettings.NormalMapMaxStyle = !programSettings.NormalMapMayaStyle;

            TextureManager.Instance.FlipNormalY = programSettings.NormalMapMayaStyle;

            offsetY += 30;

            programSettings.PostProcessEnabled = GUI.Toggle(new Rect(offsetX, offsetY, 280, 30),
                programSettings.PostProcessEnabled,
                " Enable Post Process By Default");

            offsetY += 30;
            GUI.Label(new Rect(offsetX + 70, offsetY, 250, 30), "Limit Frame Rate");

            offsetY += 30;


            if (GUI.Toggle(new Rect(offsetX + 30, offsetY, 40, 30), programSettings.FrameRate == 30, "30", "Button"))
                programSettings.FrameRate = 30;

            if (GUI.Toggle(new Rect(offsetX + 80, offsetY, 40, 30), programSettings.FrameRate == 60, "60", "Button"))
                programSettings.FrameRate = 60;

            if (GUI.Toggle(new Rect(offsetX + 130, offsetY, 40, 30), programSettings.FrameRate == 120, "120", "Button"))
                programSettings.FrameRate = 120;

            if (GUI.Toggle(new Rect(offsetX + 180, offsetY, 40, 30), programSettings.FrameRate == -1, "None", "Button"))
                programSettings.FrameRate = -1;

            offsetY += 40;

            GUI.Label(new Rect(offsetX + 70, offsetY, 250, 30), "Graphics Quality");

            offsetY += 30;

            var newQuality = CachedGraphicsQuality;

            var isMinimal = CachedGraphicsQuality == ProgramEnums.GraphicsQuality.Minimal;
            if (GUI.Toggle(new Rect(offsetX, offsetY, 70, 30), isMinimal, "Minimal")
                && !isMinimal)
            {
                newQuality = ProgramEnums.GraphicsQuality.Minimal;
            }

            var isLow = CachedGraphicsQuality == ProgramEnums.GraphicsQuality.Low;
            if (GUI.Toggle(new Rect(offsetX + 75, offsetY, 60, 30), isLow, "Low")
                && !isLow)
            {
                newQuality = ProgramEnums.GraphicsQuality.Low;
            }

            var isMedium = CachedGraphicsQuality == ProgramEnums.GraphicsQuality.Medium;
            if (GUI.Toggle(new Rect(offsetX + 130, offsetY, 70, 30), isMedium, "Medium")
                && !isMedium)
                newQuality = ProgramEnums.GraphicsQuality.Medium;

            var isHigh = CachedGraphicsQuality == ProgramEnums.GraphicsQuality.High;
            if (GUI.Toggle(new Rect(offsetX + 210, offsetY, 70, 30), isHigh, "High")
                && !isHigh)
                newQuality = ProgramEnums.GraphicsQuality.High;

            if (newQuality != CachedGraphicsQuality)
            {
                PersistentSettings.Instance.ChangeGraphicsQuality(newQuality.ToString());
                CachedGraphicsQuality = newQuality;
            }

            offsetY += 40;


            if (GUI.Button(new Rect(offsetX, offsetY, 260, 25), "Set Default Property Map Channels"))
            {
                programSettings.PropRed = MainGui.Instance.PropRed;
                programSettings.PropGreen = MainGui.Instance.PropGreen;
                programSettings.PropBlue = MainGui.Instance.PropBlue;
            }

            offsetY += 30;

            if (GUI.Button(new Rect(offsetX, offsetY, 260, 25), "Set Default File Format"))
                programSettings.FileFormat = ProgramEnums.FileFormat.Png;

            offsetY += 30;

            if (GUI.Button(new Rect(offsetX + 140, offsetY, 120, 30), "Save and Close"))
            {
                ProgramManager.Instance.SaveSettings();
                gameObject.SetActive(false);
            }
        }

        private void OnGUI()
        {
            if (Hide) return;
            MainGui.MakeScaledWindow(_windowRect, _windowId, DoMyWindow, "Setting and Preferences");
        }

        [UsedImplicitly]
        public void SettingsButtonCallBack()
        {
            if (gameObject.activeSelf)
            {
                ProgramManager.Instance.SaveSettings();
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }
    }
}