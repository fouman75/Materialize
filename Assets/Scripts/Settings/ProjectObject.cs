#region

using System;
using System.Xml.Serialization;
using General;
using Gui;
using Utility;

#endregion

namespace Settings
{
    public class ProjectObject
    {
        public string AoMapPath;

        public AoSettings AoSettings;
        public string DiffuseMapOriginalPath;
        public string DiffuseMapPath;

        public EditDiffuseSettings EditDiffuseSettings;
        public HeightFromDiffuseSettings HeightFromDiffuseSettings;
        public string HeightMapPath;
        public string MaskMapPath;

        public MaterialSettings MaterialSettings;
        public string MetallicMapPath;

        public MetallicSettings MetallicSettings;

        public NormalFromHeightSettings NormalFromHeightSettings;
        public string NormalMapPath;
        [XmlIgnore] public string ProjectPath;
        public string SmoothnessMapPath;

        public SmoothnessSettings SmoothnessSettings;

        public string GetSavePath(ProgramEnums.MapType mapType)
        {
            string path = null;
            switch (mapType)
            {
                case ProgramEnums.MapType.None:
                    break;
                case ProgramEnums.MapType.Any:
                    break;
                case ProgramEnums.MapType.Height:
                    path = HeightMapPath;
                    break;
                case ProgramEnums.MapType.Diffuse:
                    return DiffuseMapPath;
                case ProgramEnums.MapType.DiffuseOriginal:
                    path = DiffuseMapOriginalPath;
                    break;
                case ProgramEnums.MapType.AnyDiffuse:
                    path = !DiffuseMapPath.IsNullOrEmpty() && DiffuseMapPath != "null"
                        ? DiffuseMapPath
                        : DiffuseMapOriginalPath;
                    break;
                case ProgramEnums.MapType.Metallic:
                    path = MetallicMapPath;
                    break;
                case ProgramEnums.MapType.Smoothness:
                    path = SmoothnessMapPath;
                    break;
                case ProgramEnums.MapType.Normal:
                    path = NormalMapPath;
                    break;
                case ProgramEnums.MapType.Ao:
                    path = AoMapPath;
                    break;
                case ProgramEnums.MapType.Property:
                    break;
                case ProgramEnums.MapType.MaskMap:
                    path = MaskMapPath;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }

            if (path == "null") return null;
            if (ProjectPath.IsNullOrEmpty()) return null;
            return ProjectPath + path;
        }
    }
}