// Homecare.Application/Services/CloudinaryService.cs
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Homecare.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Homecare.Application.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration config)
    {
        var account = new Account(
            config["Cloudinary:CloudName"],
            config["Cloudinary:ApiKey"],
            config["Cloudinary:ApiSecret"]
        );
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder)
    {
        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = $"homecare/{folder}",
            UniqueFilename = true,
            Overwrite = false,
        };
        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.Error != null)
            throw new Exception($"Cloudinary upload failed: {result.Error.Message}");
        return result.SecureUrl.ToString();
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder)
    {
        await using var stream = file.OpenReadStream();
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = $"homecare/{folder}",
            UniqueFilename = true,
            Overwrite = false,
        };
        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.Error != null)
            throw new Exception($"Cloudinary upload failed: {result.Error.Message}");
        return result.SecureUrl.ToString();
    }

    public async Task DeleteAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        await _cloudinary.DestroyAsync(deleteParams);
    }

    public string? ExtractPublicId(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        try
        {
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/');
            var uploadIndex = Array.IndexOf(segments, "upload");
            if (uploadIndex < 0) return null;

            var start = uploadIndex + 1;
            if (start < segments.Length && System.Text.RegularExpressions.Regex.IsMatch(segments[start], @"^v\d+$"))
                start++;

            var publicIdWithExt = string.Join("/", segments[start..]);
            var dot = publicIdWithExt.LastIndexOf('.');
            return dot > 0 ? publicIdWithExt[..dot] : publicIdWithExt;
        }
        catch { return null; }
    }
    public async Task<string> UploadRawFileAsync(byte[] fileBytes, string fileName, string folder)
    {
        using var stream = new MemoryStream(fileBytes);

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = folder,
            PublicId = Path.GetFileNameWithoutExtension(fileName),
            Overwrite = true,
            UseFilename = false,
            UniqueFilename = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new Exception($"Cloudinary upload failed: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }
}