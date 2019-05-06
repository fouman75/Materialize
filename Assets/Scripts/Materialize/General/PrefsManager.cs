using System.Collections.Generic;
using Materialize.Settings;
using UnityEngine;
using Logger = Utility.Logger;
using Newtonsoft.Json;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace Materialize.General
{
    public static class PrefsManager
    {
        private static List<string> _serializationErrors;
        private const string LastPathKey = nameof(LastPathKey);
        private const string GraphicsQualityKey = nameof(GraphicsQualityKey);
        private const string DontAskAgainForGraphicsQualityKey = nameof(DontAskAgainForGraphicsQualityKey);
        private const string SettingsKey = nameof(SettingsKey);
        private const string IsJsonKey = nameof(IsJsonKey);


        public static string LastPath
        {
            get => PlayerPrefs.HasKey(LastPathKey) ? PlayerPrefs.GetString(LastPathKey) : null;
            set => PlayerPrefs.SetString(LastPathKey, value);
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
            set => PlayerPrefs.SetInt(GraphicsQualityKey, (int) value);
        }

        public static ProgramSettings ProgramSettings
        {
            get
            {
                if (!PlayerPrefs.HasKey(SettingsKey)) return null;
                if (!PlayerPrefs.HasKey(IsJsonKey))
                {
                    PlayerPrefs.DeleteKey(SettingsKey);
                }

                PlayerPrefs.SetInt(IsJsonKey, 1);

                var settings = PlayerPrefs.GetString(SettingsKey);

                return Deserialize<ProgramSettings>(settings);
            }
            set
            {
                var jss = new JsonSerializerSettings
                {
                    Error = JsonErrorHandler
                };
                var settings = JsonConvert.SerializeObject(value, jss);

                PlayerPrefs.SetString(SettingsKey, settings);
            }
        }

        private static T Deserialize<T>(string settings)
        {
            _serializationErrors = new List<string>();
            var jss = new JsonSerializerSettings
            {
                Error = JsonErrorHandler
            };
            var obj = JsonConvert.DeserializeObject<T>(settings, jss);
            foreach (var error in _serializationErrors)
            {
                Logger.Log($"Error with {nameof(T)}: {error}");
            }

            return obj;
        }

        private static void JsonErrorHandler(object sender, ErrorEventArgs e)
        {
            var currentError = e.ErrorContext.Error.Message;
            _serializationErrors.Add(currentError);
        }


        public static void Save()
        {
            Logger.Log("Saving Settings");
            PlayerPrefs.Save();
        }
    }
}