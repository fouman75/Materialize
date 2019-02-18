using System.ComponentModel;

namespace Settings
{
    public class EditDiffuseSettings
    {
        [DefaultValue(50)] public int AvgColorBlurSize;
        [DefaultValue(0.0f)] public float BlurContrast;
        [DefaultValue(20)] public int BlurSize;
        [DefaultValue(0.5f)] public float ColorLerp;
        [DefaultValue(0.5f)] public float DarkMaskPow;
        [DefaultValue(0.0f)] public float DarkPow;
        [DefaultValue(0.0f)] public float DarkSpot;
        [DefaultValue(0.0f)] public float FinalBias;
        [DefaultValue(1.0f)] public float FinalContrast;
        [DefaultValue(0.0f)] public float HotSpot;
        [DefaultValue(0.5f)] public float LightMaskPow;
        [DefaultValue(0f)] public float LightPow;
        [DefaultValue(1.0f)] public float Saturation;


        public EditDiffuseSettings()
        {
            AvgColorBlurSize = 50;
            BlurSize = 20;
            BlurContrast = 0.0f;
            LightMaskPow = 0.5f;
            LightPow = 0.0f;
            DarkMaskPow = 0.5f;
            DarkPow = 0.0f;
            HotSpot = 0.0f;
            DarkSpot = 0.0f;
            FinalContrast = 1.0f;
            FinalBias = 0.0f;
            ColorLerp = 0.5f;
            Saturation = 1.0f;
        }
    }
}