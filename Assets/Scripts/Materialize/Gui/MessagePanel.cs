#region

using TMPro;
using UnityEngine;
using Utility;

#endregion

namespace Materialize.Gui
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