#region

using System.ComponentModel;

#endregion

public class AoSettings
{
    [DefaultValue(1.0f)] public float Blend;

    [DefaultValue(100.0f)] public float Depth;

    [DefaultValue(0.0f)] public float FinalBias;

    [DefaultValue(1.0f)] public float FinalContrast;

    [DefaultValue(5.0f)] public float Spread;


    public AoSettings()
    {
        Spread = 50.0f;
        Depth = 100.0f;
        FinalBias = 0.0f;
        FinalContrast = 1.0f;
        Blend = 1.0f;
    }
}