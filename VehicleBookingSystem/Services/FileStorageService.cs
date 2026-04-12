using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace VehicleBookingSystem.Services;

public sealed class FileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024;

    private readonly IWebHostEnvironment _environment;

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<IReadOnlyList<string>> SaveVehicleGalleryAsync(Guid vehicleId, IReadOnlyList<IFormFile> files, CancellationToken cancellationToken = default)
    {
        if (files.Count == 0)
        {
            return [];
        }

        ValidateFiles(files);

        var rootPath = EnsureVehicleDirectory(vehicleId);
        await DeleteDirectoryContentAsync(rootPath, cancellationToken);

        var result = new List<string>(files.Count);
        for (var index = 0; index < files.Count; index++)
        {
            var relative = $"/Content/Images/Vehicles/{vehicleId:N}/{index + 1}.jpg";
            var absolute = Path.Combine(_environment.WebRootPath, "Content", "Images", "Vehicles", vehicleId.ToString("N"), $"{index + 1}.jpg");
            await SaveResizedJpegAsync(files[index], absolute, cancellationToken);
            result.Add(relative);
        }

        return result;
    }

    public async Task<string?> SaveAvatarAsync(Guid userId, IFormFile? file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        ValidateFiles([file]);

        var avatarFolder = Path.Combine(_environment.WebRootPath, "Content", "Images", "Avatars");
        Directory.CreateDirectory(avatarFolder);

        var avatarPath = Path.Combine(avatarFolder, $"{userId:N}.jpg");
        await SaveResizedJpegAsync(file, avatarPath, cancellationToken, 320, 320);
        return $"/Content/Images/Avatars/{userId:N}.jpg";
    }

    public Task DeleteVehicleGalleryAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        var folder = Path.Combine(_environment.WebRootPath, "Content", "Images", "Vehicles", vehicleId.ToString("N"));
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, true);
        }

        return Task.CompletedTask;
    }

    private static void ValidateFiles(IReadOnlyList<IFormFile> files)
    {
        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Chỉ cho phép file ảnh .jpg, .png hoặc .webp.");
            }

            if (file.Length == 0 || file.Length > MaxFileSizeInBytes)
            {
                throw new InvalidOperationException("Kích thước ảnh phải lớn hơn 0 và tối đa 5MB.");
            }
        }
    }

    private string EnsureVehicleDirectory(Guid vehicleId)
    {
        var path = Path.Combine(_environment.WebRootPath, "Content", "Images", "Vehicles", vehicleId.ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static Task DeleteDirectoryContentAsync(string path, CancellationToken cancellationToken)
    {
        foreach (var file in Directory.GetFiles(path))
        {
            cancellationToken.ThrowIfCancellationRequested();
            File.Delete(file);
        }

        return Task.CompletedTask;
    }

    private static async Task SaveResizedJpegAsync(IFormFile file, string outputPath, CancellationToken cancellationToken, int maxWidth = 1280, int maxHeight = 960)
    {
        await using var input = file.OpenReadStream();
        using var image = await Image.LoadAsync(input, cancellationToken);

        image.Mutate(context =>
            context.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxWidth, maxHeight)
            }));

        await using var output = File.Create(outputPath);
        await image.SaveAsJpegAsync(output, new JpegEncoder { Quality = 82 }, cancellationToken);
    }
}
