using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoCabinet.Utility
{
    public class MediaTypeUtils
    {
        private static string jpgExtension = ".jpg";
        private static string pngExtension = ".png";
        private static string mp4Extension = ".mp4";
        private static string movExtension = ".mp4";

        private static readonly List<string> imageExtensions = new List<string> { jpgExtension, pngExtension };
        private static readonly List<string> videoExtensions = new List<string> { movExtension, mp4Extension };

        public static bool IsImage(string extension)
        {
            return imageExtensions.Contains(extension.ToLower());
        }

        public static bool IsMovVideo(string extension)
        {
            return movExtension == extension.ToLower();
        }

        public static bool IsVideo(string extension)
        {
            return videoExtensions.Contains(extension.ToLower());
        }
    }
}
