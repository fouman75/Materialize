#region

using UnityEngine;

#endregion

namespace Materialize.Settings
{
    public class HeightFromDiffuseSettings : TexturePanelSettings
    {
        public float Blur0Contrast;
        public float Blur0Weight;
        public float Blur1Contrast;
        public float Blur1Weight;
        public float Blur2Contrast;
        public float Blur2Weight;
        public float Blur3Contrast;
        public float Blur3Weight;
        public float Blur4Contrast;
        public float Blur4Weight;
        public float Blur5Contrast;
        public float Blur5Weight;
        public float Blur6Contrast;
        public float Blur6Weight;
        public float FinalBias;
        public float FinalContrast;
        public float FinalGain;
        public float HueWeight1;
        public float HueWeight2;
        public bool IsolateSample1;
        public bool IsolateSample2;
        public float LumWeight1;
        public float LumWeight2;
        public float MaskHigh1;
        public float MaskHigh2;
        public float MaskLow1;
        public float MaskLow2;
        public float Sample1Height;
        public float Sample2Height;
        public float SampleBlend;
        public Color SampleColor1;
        public Color SampleColor2;
        public Vector2 SampleUv1;
        public Vector2 SampleUv2;
        public float SatWeight1;
        public float SatWeight2;
        public float Spread;
        public float SpreadBoost;
        public bool UseAdjustedDiffuse;
        public bool UseNormal;
        public bool UseOriginalDiffuse;
        public bool UseSample1;
        public bool UseSample2;

        public HeightFromDiffuseSettings()
        {
            UseAdjustedDiffuse = true;
            UseOriginalDiffuse = false;
            UseNormal = false;
            Reset();
        }

        public sealed override void Reset()
        {
            Blur0Weight = 0.15f;
            Blur1Weight = 0.19f;
            Blur2Weight = 0.3f;
            Blur3Weight = 0.5f;
            Blur4Weight = 0.7f;
            Blur5Weight = 0.9f;
            Blur6Weight = 1.0f;

            Blur0Contrast = 1.0f;
            Blur1Contrast = 1.0f;
            Blur2Contrast = 1.0f;
            Blur3Contrast = 1.0f;
            Blur4Contrast = 1.0f;
            Blur5Contrast = 1.0f;
            Blur6Contrast = 1.0f;

            SampleColor1 = Color.black;
            SampleUv1 = Vector2.zero;
            UseSample1 = false;
            IsolateSample1 = false;
            HueWeight1 = 1.0f;
            SatWeight1 = 0.5f;
            LumWeight1 = 0.2f;
            MaskLow1 = 0.0f;
            MaskHigh1 = 1.0f;
            Sample1Height = 0.5f;

            SampleColor2 = Color.black;
            SampleUv2 = Vector2.zero;
            UseSample2 = false;
            IsolateSample2 = false;
            HueWeight2 = 1.0f;
            SatWeight2 = 0.5f;
            LumWeight2 = 0.2f;
            MaskLow2 = 0.0f;
            MaskHigh2 = 1.0f;
            Sample2Height = 0.3f;

            FinalContrast = 1.5f;
            FinalBias = 0.0f;
            FinalGain = 0.0f;
            SampleBlend = 0.5f;
            Spread = 50.0f;
            SpreadBoost = 1.0f;
        }
    }
}