#region

using Materialize.General;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;

#endregion

namespace Materialize.Settings
{
    public class MaterialSettings : TexturePanelSettings
    {
        private const float LightIntensityDefault = 1f;
        private readonly Color _originalLightColor;
        private readonly float _originalLightIntensity;
        public float AoRemapMax;
        public float AoRemapMin;
        public float DisplacementAmplitude;
        public float DisplacementCenter;
        public float LightB;
        public float LightG;
        public float LightExposure;
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
            ProgramManager.Instance.SceneVolume.profile.TryGet(out HDRISky hdriSky);
            ProgramManager.Instance.SceneVolume.profile.TryGet(out ColorAdjustments colorAdjustments);

            var color = colorAdjustments ? colorAdjustments.colorFilter : new Color(1.0f, 1.0f, 1.0f);
            _originalLightColor = color;

            LightR = color.r;
            LightG = color.g;
            LightB = color.b;
            LightExposure = hdriSky ? hdriSky.exposure : LightIntensityDefault;
            _originalLightIntensity = LightExposure;

            Reset();
        }

        public sealed override void Reset()
        {
            LightR = _originalLightColor.r;
            LightG = _originalLightColor.g;
            LightB = _originalLightColor.b;
            LightExposure = _originalLightIntensity;

            TexTilingX = 1;
            TexTilingY = 1;

            TexOffsetX = 0;
            TexOffsetY = 0;
        }
    }
}