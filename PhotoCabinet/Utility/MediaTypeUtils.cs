using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoCabinet.Utility
{
    public enum MediaType
    {
        ImageBmp,
        ImageJpg,
        ImageJpeg,
        ImagePng,
        ImageNef,
        VideoMp4,
        VideoMov,
        Unknown
    }

    public static class MediaTypeUtils
    {
        private static Dictionary<string, MediaType> extensionToMediaTypeMap = new Dictionary<string, MediaType>
        {
            { ".bmp", MediaType.ImageBmp },
            { ".jpg", MediaType.ImageJpg },
            { ".jpeg", MediaType.ImageJpeg },
            { ".png", MediaType.ImagePng },
            { ".nef", MediaType.ImageNef },
            { ".mp4", MediaType.VideoMp4 },
            { ".mov", MediaType.VideoMov },
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
