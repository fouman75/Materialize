using UnityEngine;
using Logger = Utility.Logger;

namespace Materialize.General
{
    public static class PrefsManager
    {
        private const string LastPathKey = nameof(LastPathKey);
        private const string GraphicsQualityKey = nameof(GraphicsQualityKey);
        private const string DontAskAgainForGraphicsQualityKey = nameof(DontAskAgainForGraphicsQualityKey);

        public static string LastPath
        {
            get => PlayerPrefs.HasKey(LastPathKey) ? PlayerPrefs.GetString(LastPathKey) : null;
            set => PlayerPrefs.SetString(LastPathKey, LastPath);
        }

        public static bool DontAskAgainForGraphicsQuality
        {
            get
            {
                if (!PlayerPrefs.HasKey(DontAskAgainForGraphicsQualityKey)) return false;

                var val = PlayerPrefs.GetInt(DontAskAgainForGraphicsQualityKey);
                return val != 0;
            }
            set => PlayerPrefs.SetInt(DontAskAgainForGraphicsQualityKey, value ? 1 : 0);
        }

        public static ProgramEnums.GraphicsQuality GraphicsQuality
        {
            get
            {
                if (!PlayerPrefs.HasKey(GraphicsQualityKey)) return ProgramEnums.GraphicsQuality.High;

                return (ProgramEnums.GraphicsQuality) PlayerPrefs.GetInt(GraphicsQualityKey);
            }
            set => PlayerPrefs.SetInt(DontAskAgainForGraphicsQualityKey, (int) value);
        }

        public static void Save()
        {
            Logger.Log("Saving Settings");
            PlayerPrefs.Save();
        }
    }
}