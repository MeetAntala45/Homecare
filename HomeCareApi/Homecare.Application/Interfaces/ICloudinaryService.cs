// Homecare.Application/Interfaces/ICloudinaryService.cs
using Microsoft.AspNetCore.Http;

namespace Homecare.Application.Interfaces;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file, string folder);
    Task<string> UploadFileAsync(IFormFile file, string folder);
    Task DeleteAsync(string publicId);
    string? ExtractPublicId(string? url);
    Task<string> UploadRawFileAsync(byte[] fileBytes, string fileName, string folder);

}