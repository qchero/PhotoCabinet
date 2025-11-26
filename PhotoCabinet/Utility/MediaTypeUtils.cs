using System.Collections.Generic;

namespace PhotoCabinet.Utility
{
    public enum MediaType
    {
        ImageBmp,
        ImageHeic,
        ImageJpg,
        ImageJpeg,
        ImagePng,
        ImageNef,
        ImageGif,
        VideoMp4,
        VideoMov,
        VideoM4v,
        Unknown
    }

    public static class MediaTypeUtils
    {
        private static Dictionary<string, MediaType> extensionToMediaTypeMap = new Dictionary<string, MediaType>
        {
            { ".bmp", MediaType.ImageBmp },
            { ".jpg", MediaType.ImageJpg },
            { ".jpeg", MediaType.ImageJpeg },
            { ".heic", MediaType.ImageHeic },
            { ".png", MediaType.ImagePng },
            { ".nef", MediaType.ImageNef },
            { ".gif", MediaType.ImageGif },
            { ".mp4", MediaType.VideoMp4 },
            { ".mov", MediaType.VideoMov },
            { ".m4v", MediaType.VideoM4v },
        };

        public static bool IsImage(this MediaType mediaType)
        {
            return mediaType.ToString().StartsWith("Image");
        }

        public static bool IsVideo(this MediaType mediaType)
        {
            return mediaType.ToString().StartsWith("Video");
        }

        public static MediaType GetMediaType(string extension)
        {
            if (extensionToMediaTypeMap.TryGetValue(extension.ToLower(), out var mediaType))
            {
                return mediaType;
            }

            return MediaType.Unknown;
        }
    }
}
