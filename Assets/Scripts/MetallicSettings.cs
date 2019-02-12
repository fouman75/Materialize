#region

using System.ComponentModel;
using UnityEngine;

#endregion

public class MetallicSettings
{
    [DefaultValue(1.0f)] public float BlurOverlay;
    [DefaultValue(0)] public int BlurSize;
    [DefaultValue(0.0f)] public float FinalBias;
    [DefaultValue(1.0f)] public float FinalContrast;
    [DefaultValue(1.0f)] public float HueWeight;
    [DefaultValue(0.2f)] public float LumWeight;
    [DefaultValue(1.0f)] public float MaskHigh;
    [DefaultValue(0.0f)] public float MaskLow;

    public Color MetalColor;

    [DefaultValue(30)] public int OverlayBlurSize;
    public Vector2 SampleUv;

    [DefaultValue(0.5f)] public float SatWeight;

    [DefaultValue(false)] public bool UseAdjustedDiffuse;

    [DefaultValue(true)] public bool UseOriginalDiffuse;

    public MetallicSettings()
    {
        MetalColor = Color.black;
        SampleUv = Vector2.zero;
        HueWeight = 1.0f;
        SatWeight = 0.5f;
        LumWeight = 0.2f;
        MaskLow = 0.0f;
        MaskHigh = 1.0f;
        BlurSize = 0;
        OverlayBlurSize = 30;
        BlurOverlay = 1.0f;
        FinalContrast = 1.0f;
        FinalBias = 0.0f;
        UseAdjustedDiffuse = false;
        UseOriginalDiffuse = true;
    }
}