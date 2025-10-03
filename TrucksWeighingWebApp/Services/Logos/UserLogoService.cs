using Microsoft.EntityFrameworkCore;
using TrucksWeighingWebApp.Data;
using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.Services.Logos
{
    public class UserLogoService : IUserLogoService
    {
        private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg" };
        private static readonly HashSet<string> AllowedMime = new(StringComparer.OrdinalIgnoreCase) { "image/png", "image/jpeg" };
        private const int MaxBytes = 512 * 1024;

        public readonly ApplicationDbContext _db;
        public readonly IWebHostEnvironment _environment;

        public UserLogoService(ApplicationDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }


        public async Task<int> UploadAsync(
            string userId, 
            IFormFile file, 
            string imageName, 
            int imageHeight,
            int imagePaddingBottom,
            LogoPosition imagePosition,
            CancellationToken ct)
        {
            if (file is null || file.Length == 0) throw new InvalidOperationException("No file selected.");
            if (!AllowedMime.Contains(file.ContentType)) throw new InvalidOperationException("Only PNG and JPEG formats are allowed");
            if (file.Length > MaxBytes) throw new InvalidOperationException($"File is too large, must be up to {MaxBytes / 1024} KB");
            if (imageHeight is < 20 or > 100) throw new InvalidOperationException("Height must be between 20 and 100");
            if (imagePaddingBottom is <1 or >100) throw new InvalidOperationException("Bottom padding must be between 1 and 100");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext==".jpeg") ext = ".jpg";
            if (!AllowedExt.Contains(ext)) throw new InvalidOperationException("Only .png, .jpg, or .jpeg files are allowed.");

            var relativeFolder = $"images/uploads/logos/{userId}";
            var absFolder = Path.Combine(_environment.WebRootPath, relativeFolder);
            Directory.CreateDirectory(absFolder);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var absPath = Path.Combine(absFolder, fileName);
            var relativePath = $"{relativeFolder}/{fileName}".Replace("\\", "/");

            try
            {
                using (var fs = new FileStream(absPath, FileMode.CreateNew))
                {
                    await file.CopyToAsync(fs, ct);
                }

                var entity = new UserLogo
                {
                    ApplicationUserId = userId,
                    Name = imageName,
                    Height = imageHeight,
                    PaddingBottom = imagePaddingBottom,
                    Position = imagePosition,
                    FilePath = relativePath,
                    FileSizeBytes = file.Length,
                    OriginalFileName = Path.GetFileName(file.FileName),
                    ContentType = file.ContentType.ToLowerInvariant(),
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                _db.UserLogos.Add(entity);
                await _db.SaveChangesAsync(ct);
                return entity.Id;
            }
            catch
            {
                if (System.IO.File.Exists(absPath))
                {
                    System.IO.File.Delete(absPath);
                }
                throw;
            }

            
        }

        public async Task<List<UserLogo>> ListAsync(string userId, CancellationToken ct)
        {
            var query = _db.UserLogos
                .Where(l => l.ApplicationUserId == userId)
                .OrderBy(l => l.Name);

            var logos = await query.ToListAsync(ct);

            return logos;
        }

        public Task<UserLogo> GetAsync(int id, string userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteAsync(int id, string userId, CancellationToken ct)
        {
            var logo = await _db.UserLogos
                .FirstOrDefaultAsync(l => l.Id == id && l.ApplicationUserId == userId, ct);

            if(logo is null)
            {
                return false;
            }

            _db.UserLogos .Remove(logo);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
