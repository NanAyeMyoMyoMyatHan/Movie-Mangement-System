namespace DotNetProject.WebApi.Features.Shared
{
    /// <summary>
    /// Handles all physical file operations: save, delete, and validate.
    /// All files are stored under the root folder configured in appsettings.json → "FileStorage:RootPath".
    /// Sub-folders:  images/  → posters, banners, thumbnails
    ///               videos/  → episode video files
    /// </summary>
    public class FileStorageService
    {
        private readonly string _rootPath;

        // Allowed MIME types / extensions
        private static readonly HashSet<string> AllowedImageExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        private static readonly HashSet<string> AllowedVideoExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".mp4", ".mkv", ".webm", ".avi", ".mov" };

        // 500 MB max for video, 10 MB max for image
        private const long MaxImageBytes = 10L * 1024 * 1024;
        private const long MaxVideoBytes = 500L * 1024 * 1024;

        public FileStorageService(IConfiguration configuration)
        {
            _rootPath = configuration["FileStorage:RootPath"]
                        ?? Path.Combine(Directory.GetCurrentDirectory(), "AppStorage");
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Saves an image file (poster / banner / thumbnail).
        /// Returns the stored filename (e.g. "a7b9f3.jpg") or throws on validation failure.
        /// </summary>
        public async Task<string> SaveImageAsync(IFormFile file, CancellationToken ct = default)
        {
            ValidateImage(file);
            return await SaveFileAsync(file, "images", ct);
        }

        /// <summary>
        /// Saves a video file (episode video).
        /// Returns the stored filename (e.g. "e1c2d3.mp4") or throws on validation failure.
        /// </summary>
        public async Task<string> SaveVideoAsync(IFormFile file, CancellationToken ct = default)
        {
            ValidateVideo(file);
            return await SaveFileAsync(file, "videos", ct);
        }

        /// <summary>
        /// Deletes a previously stored image by its filename. Safe – does nothing if file not found.
        /// </summary>
        public void DeleteImage(string? fileName)
            => DeleteFile("images", fileName);

        /// <summary>
        /// Deletes a previously stored video by its filename. Safe – does nothing if file not found.
        /// </summary>
        public void DeleteVideo(string? fileName)
            => DeleteFile("videos", fileName);

        /// <summary>
        /// Returns true when the value references a file managed in AppStorage (either bare filename "a7b9.jpg"
        /// or relative storage route "/images/a7b9.jpg" / "/storage/images/a7b9.jpg").
        /// Ignores external URLs ("https://...") and non-GUID bundled static assets.
        /// </summary>
        public static bool IsLocalFile(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (value.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return false;

            if (!value.Contains('/')) return true;

            if (value.StartsWith("/images/", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("/storage/images/", StringComparison.OrdinalIgnoreCase))
            {
                var nameWithoutExt = Path.GetFileNameWithoutExtension(value);
                return Guid.TryParseExact(nameWithoutExt, "N", out _);
            }

            return false;
        }

        // ── Internals ──────────────────────────────────────────────────────────

        private async Task<string> SaveFileAsync(IFormFile file, string subFolder, CancellationToken ct)
        {
            var dir = Path.Combine(_rootPath, subFolder);
            Directory.CreateDirectory(dir); // creates if not exists

            var ext = Path.GetExtension(file.FileName);
            var uniqueName = $"{Guid.NewGuid():N}{ext}"; // e.g. "a7b9f3c1...jpg"
            var fullPath = Path.Combine(dir, uniqueName);

            await using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            await file.CopyToAsync(stream, ct);

            return uniqueName;
        }

        private void DeleteFile(string subFolder, string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return;
            var safeFileName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(safeFileName)) return;

            var fullPath = Path.Combine(_rootPath, subFolder, safeFileName);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        private static void ValidateImage(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName);
            if (!AllowedImageExtensions.Contains(ext))
                throw new InvalidOperationException(
                    $"Image file type '{ext}' is not allowed. Allowed: {string.Join(", ", AllowedImageExtensions)}");

            if (file.Length > MaxImageBytes)
                throw new InvalidOperationException(
                    $"Image file size exceeds the 10 MB limit. Received: {file.Length / 1024 / 1024} MB");
        }

        private static void ValidateVideo(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName);
            if (!AllowedVideoExtensions.Contains(ext))
                throw new InvalidOperationException(
                    $"Video file type '{ext}' is not allowed. Allowed: {string.Join(", ", AllowedVideoExtensions)}");

            if (file.Length > MaxVideoBytes)
                throw new InvalidOperationException(
                    $"Video file size exceeds the 500 MB limit. Received: {file.Length / 1024 / 1024} MB");
        }
    }
}
