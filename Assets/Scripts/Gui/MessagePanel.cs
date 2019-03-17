#region

using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

#endregion

namespace Gui
{
    public class MessagePanel : Singleton<MessagePanel>
    {
        // ReSharper disable once RedundantDefaultMemberInitializer
        [SerializeField] private TextMeshProUGUI Message = null;

        private void Start()
        {
            _instance = this;
            gameObject.SetActive(false);
        }

        public static void ShowMessage(string message)
        {
            Instance.Message.text = message;
            Instance.gameObject.SetActive(true);
            MainGui.Instance.SaveHideStateAndHideAndLock(Instance);
        }

        public static void HideMessage()
        {
            Instance.gameObject.SetActive(false);
            MainGui.Instance.HideGuiLocker.Unlock(Instance);
        }
    }
}