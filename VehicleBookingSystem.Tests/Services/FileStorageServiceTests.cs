using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using VehicleBookingSystem.Services;

namespace VehicleBookingSystem.Tests.Services;

public sealed class FileStorageServiceTests : IDisposable
{
    private readonly string _webRootPath;

    public FileStorageServiceTests()
    {
        _webRootPath = Path.Combine(Path.GetTempPath(), "vbs-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_webRootPath);
    }

    [Fact]
    public async Task SaveAvatarAsync_ThrowsInvalidOperation_WhenImageContentIsInvalid()
    {
        var service = new FileStorageService(BuildEnvironment(_webRootPath));
        var file = BuildFormFile("avatar.jpg", "not-an-image");

        var action = async () => await service.SaveAvatarAsync(Guid.NewGuid(), file);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Contains("không hợp lệ", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveVehicleGalleryAsync_ThrowsInvalidOperation_WhenReadStreamThrowsIOException()
    {
        var service = new FileStorageService(BuildEnvironment(_webRootPath));
        var file = new Mock<IFormFile>();
        file.SetupGet(item => item.FileName).Returns("gallery.jpg");
        file.SetupGet(item => item.Length).Returns(1024);
        file.Setup(item => item.OpenReadStream()).Throws(new IOException("disk busy"));

        var action = async () => await service.SaveVehicleGalleryAsync(Guid.NewGuid(), [file.Object]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Contains("Không thể lưu ảnh", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        if (Directory.Exists(_webRootPath))
        {
            Directory.Delete(_webRootPath, recursive: true);
        }
    }

    private static IWebHostEnvironment BuildEnvironment(string webRootPath)
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(item => item.WebRootPath).Returns(webRootPath);
        return environment.Object;
    }

    private static IFormFile BuildFormFile(string fileName, string textContent)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(textContent);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName);
    }
}
