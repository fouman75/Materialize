using System.ComponentModel;
using UnityEngine;

namespace Gui
{
    public class MaterialSettings
    {
        [DefaultValue(1.0f)] public float DisplacementAmplitude;

        public float LightB;

        public float LightG;

        public float LightIntensity;

        public float LightR;

        public TrackableProperty Metallic;
        [DefaultValue(0)] public float AoRemapMin;
        [DefaultValue(1)] public float AoRemapMax;
        [DefaultValue(0)] public float SmoothnessRemapMin;
        [DefaultValue(1)] public float SmoothnessRemapMax;
        [DefaultValue(1)] public float TexTilingX;
        [DefaultValue(1)] public float TexTilingY;
        [DefaultValue(0)] public float TexOffsetX;
        [DefaultValue(0)] public float TexOffsetY;

        public MaterialSettings(Light light)
        {
            Metallic.Value = 0.5f;
            DisplacementAmplitude = 0.2f;
            AoRemapMax = 1.0f;
            SmoothnessRemapMax = 1.0f;

            var color = light != null ? light.color : new Color();

            LightR = color.r;
            LightG = color.g;
            LightB = color.b;
            LightIntensity = light != null ? light.intensity : 20f;

            TexTilingX = 1;
            TexTilingY = 1;

            TexOffsetX = 0;
            TexOffsetY = 0;
        }

        public MaterialSettings() : this(null)
        {
        }
    }
}