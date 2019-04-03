#region

using UnityEngine;

#endregion

// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace Materialize.Settings
{
    public class SmoothnessSettings : TexturePanelSettings
    {
        public float BaseSmoothness;
        public float BlurOverlay;
        public int BlurSize;
        public float FinalBias;
        public float FinalContrast;

        public float HueWeight1;
        public float HueWeight2;
        public float HueWeight3;

        public bool IsolateSample1;
        public bool IsolateSample2;
        public bool IsolateSample3;

        public float LumWeight1;
        public float LumWeight2;
        public float LumWeight3;

        public float MaskHigh1;
        public float MaskHigh2;
        public float MaskHigh3;

        public float MaskLow1;
        public float MaskLow2;
        public float MaskLow3;

        public float MetalSmoothness;
        public int OverlayBlurSize;

        public float Sample1Smoothness;
        public float Sample2Smoothness;
        public float Sample3Smoothness;

        public Color SampleColor1;
        public Color SampleColor2;
        public Color SampleColor3;

        public Vector2 SampleUv1;
        public Vector2 SampleUv2;
        public Vector2 SampleUv3;

        public float SatWeight1;
        public float SatWeight2;
        public float SatWeight3;

        public bool UseAdjustedDiffuse;
        public bool UseOriginalDiffuse;

        public bool UseSample1;
        public bool UseSample2;
        public bool UseSample3;

        public SmoothnessSettings()
        {
            UseAdjustedDiffuse = false;
            UseOriginalDiffuse = true;
            Reset();
        }

        public sealed override void Reset()
        {
            SampleColor1 = Color.black;
            SampleUv1 = Vector2.zero;

            SampleColor2 = Color.black;
            SampleUv2 = Vector2.zero;

            SampleColor3 = Color.black;
            SampleUv3 = Vector2.zero;

            MetalSmoothness = 0.7f;
            UseSample1 = false;
            IsolateSample1 = false;
            HueWeight1 = 1.0f;
            SatWeight1 = 0.5f;
            LumWeight1 = 0.2f;
            MaskLow1 = 0.0f;
            MaskHigh1 = 1.0f;
            Sample1Smoothness = 0.5f;

            UseSample2 = false;
            IsolateSample2 = false;
            HueWeight2 = 1.0f;
            SatWeight2 = 0.5f;
            LumWeight2 = 0.2f;
            MaskLow2 = 0.0f;
            MaskHigh2 = 1.0f;
            Sample2Smoothness = 0.3f;

            UseSample3 = false;
            IsolateSample3 = false;
            HueWeight3 = 1.0f;
            SatWeight3 = 0.5f;
            LumWeight3 = 0.2f;
            MaskLow3 = 0.0f;
            MaskHigh3 = 1.0f;
            Sample3Smoothness = 0.2f;

            BaseSmoothness = 0.1f;
            BlurSize = 1;
            OverlayBlurSize = 30;
            BlurOverlay = 3.0f;
            FinalContrast = 1.0f;
            FinalBias = 0.0f;
        }
    }
}