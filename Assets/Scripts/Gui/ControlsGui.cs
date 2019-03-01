#region

using General;
using UnityEngine;

#endregion

namespace Gui
{
    public class ControlsGui : MonoBehaviour, IHideable
    {
        private bool _windowOpen;

        private Rect _windowRect = new Rect(Screen.width - 520, Screen.height - 320, 300, 600);
        private int _windowId;

        private void Awake()
        {
            ProgramManager.Instance.SceneObjects.Add(gameObject);
        }

        private void Start()
        {
            _windowId = ProgramManager.Instance.GetWindowId;
        }

        private void DoMyWindow(int windowId)
        {
            const int offsetX = 10;
            var offsetY = 30;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Rotate Model");
            offsetY += 20;
            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Right Mouse Button");
            offsetY += 30;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Move Model");
            offsetY += 20;
            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Middle Mouse Button");
            offsetY += 30;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Zoom In/Out");
            offsetY += 20;
            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Mouse Scroll Wheel");
            offsetY += 30;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Rotate Light");
            offsetY += 20;
            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Middle Mouse Button + L");
            offsetY += 30;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Rotate Background");
            offsetY += 20;
            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Middle Mouse Button + B");
            offsetY += 30;

            if (GUI.Button(new Rect(offsetX + 160, offsetY, 120, 30), "Close")) _windowOpen = false;
        }

        private void OnGUI()
        {
            if (Hide) return;
            _windowRect = new Rect(1185, 485, 170, 280);
            if (_windowOpen) MainGui.MakeScaledWindow(_windowRect, _windowId, DoMyWindow, "Controls");
        }

        public void ButtonCallback()
        {
            _windowOpen = !_windowOpen;
        }

        public bool Hide { get; set; }
    }
}