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
        public float AoMultiplier;
        public float DisplacementStrength;
        public float LightB;
        public float LightExposure;
        public float LightG;
        public float LightR;
        public float MetallicMultiplier;
        public float NormalScale;
        public float SmoothnessMultiplier;
        public float TexOffsetX;
        public float TexOffsetY;
        public float TexTilingX;
        public float TexTilingY;

        public MaterialSettings()
        {
            ProgramManager.Instance.SceneVolume.profile.TryGet(out HDRISky hdriSky);
            ProgramManager.Instance.SceneVolume.profile.TryGet(out ColorAdjustments colorAdjustments);

            var color = colorAdjustments ? colorAdjustments.colorFilter.value : new Color(1.0f, 1.0f, 1.0f);
            _originalLightColor = color;

            LightR = color.r;
            LightG = color.g;
            LightB = color.b;
            LightExposure = hdriSky ? hdriSky.exposure.value : LightIntensityDefault;
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