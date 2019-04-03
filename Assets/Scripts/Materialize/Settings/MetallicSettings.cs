#region

using UnityEngine;

#endregion

namespace Materialize.Settings
{
    public class MetallicSettings : TexturePanelSettings
    {
        public float BlurOverlay;
        public int BlurSize;
        public float FinalBias;
        public float FinalContrast;
        public float HueWeight;
        public float LumWeight;
        public float MaskHigh;
        public float MaskLow;
        public Color MetalColor;
        public int OverlayBlurSize;
        public Vector2 SampleUv;
        public float SatWeight;
        public bool UseAdjustedDiffuse;
        public bool UseOriginalDiffuse;

        public MetallicSettings()
        {
            UseAdjustedDiffuse = false;
            UseOriginalDiffuse = true;
            Reset();
        }

        public sealed override void Reset()
        {
            MetalColor = Color.black;
            SampleUv = Vector2.zero;
            HueWeight = 1.0f;
            SatWeight = 0.5f;
            LumWeight = 0.2f;
            MaskLow = 0.0f;
            MaskHigh = 1.0f;
            BlurSize = 1;
            OverlayBlurSize = 30;
            BlurOverlay = 1.0f;
            FinalContrast = 1.0f;
            FinalBias = 0.0f;
        }
    }
}