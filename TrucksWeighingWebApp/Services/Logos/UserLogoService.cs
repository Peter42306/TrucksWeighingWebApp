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

        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UserLogoService> _logger;

        public UserLogoService(
            ApplicationDbContext db,
            IWebHostEnvironment environment,
            ILogger<UserLogoService> logger)
        {
            _db = db;
            _environment = environment;
            _logger=logger;
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
            if (file is null || file.Length == 0)
                throw new InvalidOperationException("Please select a file to upload.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext == ".jpeg")
            {
                ext = ".jpg";
            }

            var normalizedContentType = file.ContentType.ToLowerInvariant();
            if (normalizedContentType != "image/png" && normalizedContentType != "image/jpeg")
            {
                normalizedContentType = ext == ".png" ? "image/png" : "image/jpeg";
            }

            if (!AllowedExt.Contains(ext))
            {
                throw new InvalidOperationException("Only .png, .jpg, or .jpeg files are supported.");
            }

            if (!AllowedMime.Contains(normalizedContentType))
            {
                throw new InvalidOperationException("Unsupported file type.");
            }

            imageName = imageName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(imageName))
            {
                throw new InvalidOperationException("Logo name is required.");
            }

            if (file.Length > MaxBytes)
            {
                throw new InvalidOperationException($"File size exceeds the limit of {MaxBytes / 1024} KB");
            }

            if (imageHeight is < 20 or > 100)
            {
                throw new InvalidOperationException("Logo height must be between 20 and 100");
            }

            if (imagePaddingBottom is <1 or >100)
            {
                throw new InvalidOperationException("Bottom padding must be between 1 and 100");
            }

            if (await _db.UserLogos.CountAsync(l => l.ApplicationUserId == userId, ct) >= 5)
            {
                throw new InvalidOperationException("You can upload up to 5 logos.");
            }
                        

            var relativeFolder = $"images/uploads/logos/{userId}";
            var absFolder = Path.Combine(_environment.WebRootPath, relativeFolder);
            Directory.CreateDirectory(absFolder);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var absPath = Path.Combine(absFolder, fileName);
            var relativePath = $"{relativeFolder}/{fileName}".Replace("\\", "/");

            try
            {
                await using var fs = new FileStream(absPath, FileMode.CreateNew);
                await file.CopyToAsync(fs, ct);

                var entity = new UserLogo
                {
                    ApplicationUserId = userId,
                    Name = imageName.Trim(),
                    Height = imageHeight,
                    PaddingBottom = imagePaddingBottom,
                    Position = imagePosition,
                    FilePath = relativePath,
                    FileSizeBytes = file.Length,
                    OriginalFileName = Path.GetFileName(file.FileName),
                    ContentType = normalizedContentType,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                _db.UserLogos.Add(entity);
                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("User {UserId} uploaded logo '{Name}' ({SizeKb} KB)", userId, imageName, file.Length / 1024);                

                return entity.Id;
            }
            catch (Exception ex)
            {
                if (System.IO.File.Exists(absPath))
                {
                    try
                    {
                        System.IO.File.Delete(absPath);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Delete failed for logo {Path}", absPath);                        
                    }
                    
                }

                _logger.LogError(ex, "Failed to upload logo '{Name}' for user {UserId}", imageName, userId);
                throw;
            }


        }

        public async Task<List<UserLogo>> ListAsync(string userId, CancellationToken ct)
        {
            var logos = await _db.UserLogos
                .AsNoTracking()
                .Where(l => l.ApplicationUserId == userId)
                .OrderBy(l => l.Name)
                .ToListAsync(ct);

            return logos;
        }

        public async Task<UserLogo> GetAsync(int id, string userId, CancellationToken ct)
        {
            var logo = await _db.UserLogos
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id && l.ApplicationUserId == userId, ct);

            if (logo is null)
                throw new InvalidOperationException("Logo not found");

            return logo;
        }

        public async Task<bool> DeleteAsync(int id, string userId, CancellationToken ct)
        {
            var logo = await _db.UserLogos
                .FirstOrDefaultAsync(l => l.Id == id && l.ApplicationUserId == userId, ct);

            if (logo is null)
                throw new InvalidOperationException("Logo not found");

            try
            {
                _db.UserLogos.Remove(logo);
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Database error while deleting logo {LogoId} for user {UserId}", id, userId);
                throw new InvalidOperationException("Failed to delete logo from database");
            }


            try
            {
                var absPath = Path.Combine(_environment.WebRootPath, logo.FilePath);
                if (System.IO.File.Exists(absPath))
                    System.IO.File.Delete(absPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete logo file {Path} for user {UserId}", logo.FilePath, userId);
            }

            return true;
        }
    }
}
