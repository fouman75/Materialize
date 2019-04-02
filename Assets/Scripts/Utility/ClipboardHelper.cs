#region

using UnityEngine;

#endregion

namespace Utility
{
    public static class ClipboardHelper
    {
        public static string ClipBoard
        {
            get => GUIUtility.systemCopyBuffer;
            set => GUIUtility.systemCopyBuffer = value;
        }
    }
}