// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 📁 FILE: FixConnect.BLL/Helpers/FileUploadHelper.cs
//     انشئ فولدر Helpers جوه BLL وحط الفايل فيه
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace FixConnect.BLL.Helpers
{
    public static class FileUploadHelper
    {
        // ✅ Static Helper - مش محتاج DI
        // بتستخدمه من أي Service بس تمرر IWebHostEnvironment
        public static async Task<string> SaveFileAsync(
            IFormFile file,
            string folder,
            IWebHostEnvironment env)
        {
            var uploadsPath = Path.Combine(env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(uploadsPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{folder}/{fileName}";
        }

        public static void DeleteFile(string? relativeUrl, IWebHostEnvironment env)
        {
            if (string.IsNullOrEmpty(relativeUrl)) return;

            var fullPath = Path.Combine(env.WebRootPath, relativeUrl.TrimStart('/'));
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}