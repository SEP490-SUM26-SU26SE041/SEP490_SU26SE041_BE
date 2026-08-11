using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Service.Interfaces.Commons;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmSEP490.Service.Services.Commons
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _settings;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IOptions<CloudinarySettings> options, ILogger<CloudinaryService> logger)
        {
            _settings = options.Value;
            _logger = logger;

            var account = new Account(
                _settings.CloudName,
                _settings.ApiKey,
                _settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder, CancellationToken ct = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.", nameof(file));

            var targetFolder = string.IsNullOrWhiteSpace(folder) ? _settings.Folder : folder;

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = targetFolder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams, ct);
            if (uploadResult.Error != null)
                throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error.Message}");

            _logger.LogInformation(
                "Uploaded file {FileName} to Cloudinary as {PublicId} in folder {Folder}",
                file.FileName, uploadResult.PublicId, targetFolder);

            return uploadResult.SecureUrl?.ToString()
                   ?? uploadResult.Url?.ToString()
                   ?? throw new InvalidOperationException("Cloudinary returned no URL.");
        }

        public async Task<bool> DeleteAsync(string publicId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(publicId)) return false;

            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);
            return result.Result == "ok";
        }
    }
}
