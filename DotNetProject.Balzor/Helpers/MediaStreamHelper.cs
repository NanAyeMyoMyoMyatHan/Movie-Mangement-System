using System;
using System.Text.RegularExpressions;

namespace DotNetProject.Balzor.Helpers
{
    public enum MediaSourceType
    {
        Local,
        YouTube,
        DirectStream,
        Unknown
    }

    public static class MediaStreamHelper
    {
        private static readonly Regex YouTubeRegex = new(
            @"(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^""&?\/\s]{11})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsYouTube(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            return url.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) ||
                   url.Contains("youtu.be", StringComparison.OrdinalIgnoreCase);
        }

        public static string? ExtractYouTubeVideoId(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            var match = YouTubeRegex.Match(url);
            return match.Success ? match.Groups[1].Value : null;
        }

        public static string? ToYouTubeEmbedUrl(string? rawUrl, bool autoplay = true)
        {
            if (string.IsNullOrWhiteSpace(rawUrl)) return null;

            var videoId = ExtractYouTubeVideoId(rawUrl);
            if (string.IsNullOrWhiteSpace(videoId))
            {
                // If it's already an embed URL, append parameters
                if (rawUrl.Contains("/embed/", StringComparison.OrdinalIgnoreCase))
                {
                    return rawUrl.Contains("?") ? $"{rawUrl}&rel=0" : $"{rawUrl}?rel=0";
                }
                return rawUrl;
            }

            var autoplayParam = autoplay ? "1" : "0";
            return $"https://www.youtube-nocookie.com/embed/{videoId}?autoplay={autoplayParam}&rel=0&modestbranding=1";
        }

        public static bool IsLocalStream(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            return url.StartsWith("/videos", StringComparison.OrdinalIgnoreCase) ||
                   url.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                   url.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) ||
                   url.EndsWith(".webm", StringComparison.OrdinalIgnoreCase);
        }

        public static MediaSourceType GetStreamSourceType(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return MediaSourceType.Unknown;
            if (IsYouTube(url)) return MediaSourceType.YouTube;
            if (IsLocalStream(url)) return MediaSourceType.Local;
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return MediaSourceType.DirectStream;
            }
            return MediaSourceType.Unknown;
        }
    }
}
