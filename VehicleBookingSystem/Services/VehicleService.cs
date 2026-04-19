using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Services;

public sealed class VehicleService : IVehicleService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly IHtmlSanitizerService _htmlSanitizer;
    private readonly IWebHostEnvironment _environment;

    public VehicleService(
        ApplicationDbContext context,
        IFileStorageService fileStorageService,
        IHtmlSanitizerService htmlSanitizer,
        IWebHostEnvironment environment)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _htmlSanitizer = htmlSanitizer;
        _environment = environment;
    }

    public async Task<VehicleFormViewModel?> BuildEditViewModelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.FindAsync([id], cancellationToken);
        if (vehicle is null)
        {
            return null;
        }

        return new VehicleFormViewModel
        {
            Id = vehicle.Id,
            VehicleCategoryId = vehicle.VehicleCategoryId,
            Code = vehicle.Code,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            LicensePlate = vehicle.LicensePlate,
            SeatCount = vehicle.SeatCount,
            Color = vehicle.Color,
            Transmission = vehicle.Transmission,
            FuelType = vehicle.FuelType,
            DailyRate = vehicle.DailyRate,
            Status = vehicle.Status,
            ImageUrl = vehicle.ImageUrl,
            Description = vehicle.Description,
            BookingPolicyHtml = vehicle.BookingPolicyHtml,
            ExistingGalleryUrls = await BuildGalleryUrlsAsync(vehicle.Id, cancellationToken)
        };
    }

    public Task<IReadOnlyList<string>> BuildGalleryUrlsAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        var root = Path.Combine(_environment.WebRootPath, "Content", "Images", "Vehicles", vehicleId.ToString("N"));
        if (!Directory.Exists(root))
        {
            return Task.FromResult<IReadOnlyList<string>>([]);
        }

        var urls = Directory.GetFiles(root)
            .OrderBy(path => path)
            .Select(path => $"/Content/Images/Vehicles/{vehicleId:N}/{Path.GetFileName(path)}")
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(urls);
    }

    public async Task<VehicleCommandResult> CreateAsync(VehicleFormViewModel form, CancellationToken cancellationToken = default)
    {
        if (await _context.Vehicles.AnyAsync(vehicle => vehicle.Code == form.Code || vehicle.LicensePlate == form.LicensePlate, cancellationToken))
        {
            return new VehicleCommandResult(false, "Vehicle code or license plate already exists.");
        }

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid()
        };

        ApplyFormValues(vehicle, form);

        try
        {
            if (form.GalleryFiles is { Count: > 0 })
            {
                var galleryUrls = await _fileStorageService.SaveVehicleGalleryAsync(vehicle.Id, form.GalleryFiles, cancellationToken);
                vehicle.ImageUrl = galleryUrls.FirstOrDefault();
            }
            else
            {
                vehicle.ImageUrl = form.ImageUrl;
            }
        }
        catch (InvalidOperationException ex)
        {
            return new VehicleCommandResult(false, ex.Message);
        }

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync(cancellationToken);
        return new VehicleCommandResult(true);
    }

    public async Task<VehicleCommandResult> UpdateAsync(Guid id, VehicleFormViewModel form, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.FindAsync([id], cancellationToken);
        if (vehicle is null)
        {
            return new VehicleCommandResult(false, "Vehicle not found.");
        }

        var duplicateExists = await _context.Vehicles.AnyAsync(item =>
            item.Id != id &&
            (item.Code == form.Code || item.LicensePlate == form.LicensePlate), cancellationToken);

        if (duplicateExists)
        {
            return new VehicleCommandResult(false, "Vehicle code or license plate already exists.");
        }

        ApplyFormValues(vehicle, form);

        try
        {
            if (form.GalleryFiles is { Count: > 0 })
            {
                var galleryUrls = await _fileStorageService.SaveVehicleGalleryAsync(vehicle.Id, form.GalleryFiles, cancellationToken);
                vehicle.ImageUrl = galleryUrls.FirstOrDefault();
            }
            else
            {
                vehicle.ImageUrl = form.ImageUrl;
            }
        }
        catch (InvalidOperationException ex)
        {
            return new VehicleCommandResult(false, ex.Message);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return new VehicleCommandResult(true);
    }

    public async Task<VehicleCommandResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.FindAsync([id], cancellationToken);
        if (vehicle is null)
        {
            return new VehicleCommandResult(false, "Vehicle not found.");
        }

        var hasActiveBookings = await _context.Bookings.AnyAsync(booking =>
            booking.VehicleId == id &&
            (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed) &&
            booking.ReturnDateTime >= DateTime.UtcNow, cancellationToken);

        if (hasActiveBookings)
        {
            return new VehicleCommandResult(false, "Cannot delete this vehicle because it has active bookings.");
        }

        _context.Vehicles.Remove(vehicle);
        await _fileStorageService.DeleteVehicleGalleryAsync(id, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return new VehicleCommandResult(true);
    }

    private void ApplyFormValues(Vehicle vehicle, VehicleFormViewModel form)
    {
        vehicle.VehicleCategoryId = form.VehicleCategoryId;
        vehicle.Code = form.Code.Trim();
        vehicle.Brand = form.Brand.Trim();
        vehicle.Model = form.Model.Trim();
        vehicle.LicensePlate = form.LicensePlate.Trim();
        vehicle.SeatCount = form.SeatCount;
        vehicle.Color = form.Color.Trim();
        vehicle.Transmission = form.Transmission.Trim();
        vehicle.FuelType = form.FuelType.Trim();
        vehicle.DailyRate = form.DailyRate;
        vehicle.Status = form.Status;
        vehicle.Description = _htmlSanitizer.Sanitize(form.Description);
        vehicle.BookingPolicyHtml = _htmlSanitizer.Sanitize(form.BookingPolicyHtml);
    }
}