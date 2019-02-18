using Gui;

namespace Settings
{
    public class ProjectObject
    {
        public string AoMapPath;

        public AoSettings AoSettings;
        public string DiffuseMapOriginalPath;
        public string DiffuseMapPath;
        public string MaskMapPath;

        public EditDiffuseSettings EditDiffuseSettings;
        public HeightFromDiffuseSettings HeightFromDiffuseSettings;
        public string HeightMapPath;

        public MaterialSettings MaterialSettings;
        public string MetallicMapPath;

        public MetallicSettings MetallicSettings;

        public NormalFromHeightSettings NormalFromHeightSettings;
        public string NormalMapPath;
        public string SmoothnessMapPath;

        public SmoothnessSettings SmoothnessSettings;
    }
}