using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace HUNting.API.Services
{
    /// <summary>
    /// Képfeltöltések közös kezelése a wwwroot/uploads/&lt;almappa&gt; alatt.
    /// Korábban az Ads/Societies/HuntingLogs controllerek külön-külön tartalmazták ugyanezt a logikát.
    /// </summary>
    public static class UploadStorage
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public static async Task<string> SaveImageAsync(IWebHostEnvironment env, string subfolder, IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Csak JPG, JPEG, PNG vagy WEBP fajl tamogatott.");
            }

            var directory = EnsureDirectory(env, subfolder);
            var fileName = $"{Guid.NewGuid():N}{extension}";

            await using var stream = File.Create(Path.Combine(directory, fileName));
            await file.CopyToAsync(stream);

            return $"/uploads/{subfolder}/{fileName}";
        }

        public static void DeleteImage(IWebHostEnvironment env, string subfolder, string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return;

            var directory = EnsureDirectory(env, subfolder);
            var absolutePath = Path.Combine(directory, Path.GetFileName(imageUrl));
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
        }

        private static string EnsureDirectory(IWebHostEnvironment env, string subfolder)
        {
            var root = env.WebRootPath;
            if (string.IsNullOrWhiteSpace(root))
            {
                root = Path.Combine(AppContext.BaseDirectory, "wwwroot");
                Directory.CreateDirectory(root);
                env.WebRootPath = root;
            }

            var path = Path.Combine(root, "uploads", subfolder);
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
