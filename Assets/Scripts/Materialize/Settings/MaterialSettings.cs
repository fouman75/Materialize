#region

using Materialize.General;
using UnityEngine;

#endregion

namespace Materialize.Settings
{
    public class MaterialSettings : TexturePanelSettings
    {
        private const float LightIntensityDefault = 10f;
        private readonly Color _originalLightColor;
        private readonly float _originalLightIntensity;
        public float AoRemapMax;
        public float AoRemapMin;
        public float DisplacementAmplitude;
        public float DisplacementCenter;
        public float LightB;
        public float LightG;
        public float LightIntensity;
        public float LightR;
        public TrackableProperty Metallic;
        public float NormalStrength;
        public float SmoothnessRemapMax;
        public float SmoothnessRemapMin;
        public float TexOffsetX;
        public float TexOffsetY;
        public float TexTilingX;
        public float TexTilingY;

        public MaterialSettings()
        {
            var light = ProgramManager.Instance.MainLight;

            var color = light ? light.color : new Color(1.0f, 1.0f, 1.0f);
            _originalLightColor = color;

            LightR = color.r;
            LightG = color.g;
            LightB = color.b;
            LightIntensity = light ? light.intensity : LightIntensityDefault;
            _originalLightIntensity = LightIntensity;

            Reset();
        }

        public sealed override void Reset()
        {
            LightR = _originalLightColor.r;
            LightG = _originalLightColor.g;
            LightB = _originalLightColor.b;
            LightIntensity = _originalLightIntensity;

            TexTilingX = 1;
            TexTilingY = 1;

            TexOffsetX = 0;
            TexOffsetY = 0;
        }
    }
}