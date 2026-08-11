using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SmartFarmSEP490.Service.Interfaces.Commons
{
    public interface ICloudinaryService
    {
        /// <summary>
        /// Upload a file (image) to Cloudinary and return its secure URL.
        /// </summary>
        Task<string> UploadImageAsync(IFormFile file, string folder, CancellationToken ct = default);

        /// <summary>
        /// Delete a Cloudinary asset by its public_id.
        /// </summary>
        Task<bool> DeleteAsync(string publicId, CancellationToken ct = default);
    }
}
