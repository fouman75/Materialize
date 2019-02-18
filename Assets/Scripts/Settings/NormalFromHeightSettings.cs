#region

using System.ComponentModel;

#endregion

namespace Settings
{
    public class NormalFromHeightSettings
    {
        [DefaultValue(0.5f)] public float AngularIntensity;

        [DefaultValue(0.0f)] public float Angularity;

        [DefaultValue(20.0f)] public float Blur0Contrast;

        [DefaultValue(0.3f)] public float Blur0Weight;

        [DefaultValue(0.35f)] public float Blur1Weight;

        [DefaultValue(0.5f)] public float Blur2Weight;

        [DefaultValue(0.8f)] public float Blur3Weight;

        [DefaultValue(1.0f)] public float Blur4Weight;

        [DefaultValue(0.95f)] public float Blur5Weight;

        [DefaultValue(0.8f)] public float Blur6Weight;

        [DefaultValue(5.0f)] public float FinalContrast;

        [DefaultValue(0.0f)] public float LightRotation;

        [DefaultValue(0.5f)] public float ShapeBias;

        [DefaultValue(0.0f)] public float ShapeRecognition;

        [DefaultValue(50.0f)] public int SlopeBlur;

        [DefaultValue(true)] public bool UseDiffuse;

        public NormalFromHeightSettings()
        {
            Blur0Weight = 0.3f;
            Blur1Weight = 0.35f;
            Blur2Weight = 0.5f;
            Blur3Weight = 0.8f;
            Blur4Weight = 1.0f;
            Blur5Weight = 0.95f;
            Blur6Weight = 0.8f;

            Blur0Contrast = 20.0f;
            FinalContrast = 5.0f;
        
            Angularity = 0.0f;
            AngularIntensity = 0.5f;
            UseDiffuse = true;

            ShapeRecognition = 0.0f;

            LightRotation = 0.0f;

            SlopeBlur = 50;

            ShapeBias = 0.5f;
        }
    }
}