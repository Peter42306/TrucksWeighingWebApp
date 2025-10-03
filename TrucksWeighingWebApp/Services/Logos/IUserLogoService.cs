using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.Services.Logos
{
    public interface IUserLogoService
    {
        Task<int> UploadAsync(
            string userId,
            IFormFile file,
            string imageName,
            int imageHeight,
            int imagePaddingBottom,
            LogoPosition imagePosition,
            CancellationToken ct
            );

        Task<List<UserLogo>> ListAsync(string userId, CancellationToken ct);
        Task<UserLogo> GetAsync(int id, string userId, CancellationToken ct);
        Task<bool> DeleteAsync(int id, string userId, CancellationToken ct);
    }
}
