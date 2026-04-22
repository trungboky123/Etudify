using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace back_end.Components;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var account = new Account(
            configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:ApiKey"],
            configuration["Cloudinary:ApiSecret"]
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadUserAvatar(IFormFile file, string userId)
    {
        if (file == null || file.Length == 0) return null;

        var publicId = $"user_{userId}";
        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            PublicId = publicId,
            Overwrite = true,
            Folder = "user_avt"
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception("Upload avatar failed!");
        }

        return result.SecureUrl.ToString();
    }

    public async Task<string> UploadCourseThumbnail(IFormFile file, int courseId)
    {
        if (file == null || file.Length == 0) return null;
        
        var publicId = $"course_{courseId}";
        await using var stream = file.OpenReadStream();
        
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            PublicId = publicId,
            Overwrite = true,
            Folder = "course_thumbnail"
        };
        
        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception("Upload thumbnail failed!");
        }

        return result.SecureUrl.ToString();
    }
}