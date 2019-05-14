namespace Materialize.General
{
    public static class ProgramEnums
    {
        public enum FileFormat
        {
            Png,
            Jpg,
            Tga,
            Exr,
            Bmp,
            Invalid
        }

        public enum GraphicsQuality
        {
            High = 3,
            Medium = 2,
            Low = 1,
            Minimal = 0
        }

        public enum MapType
        {
            None,
            Any,
            Height,
            Diffuse,
            DiffuseOriginal,
            AnyDiffuse,
            Metallic,
            Smoothness,
            Normal,
            Ao,
            Property,
            MaskMap
        }

        public enum PropChannelMap
        {
            None,
            Height,
            Metallic,
            Smoothness,
            Ao,
            MaskMap
        }

        public enum ScreenMode
        {
            FullScreen,
            Windowed
        }
    }
}