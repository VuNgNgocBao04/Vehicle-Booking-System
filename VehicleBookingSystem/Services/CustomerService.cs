using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public CustomerService(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<CustomerVehicleIndexViewModel> BuildIndexAsync(VehicleFilterViewModel filter, IReadOnlyList<VehicleSearchHistoryItem> history, CancellationToken cancellationToken = default)
    {
        filter.Page = Math.Max(filter.Page, 1);
        filter.PageSize = filter.PageSize <= 0 ? 9 : Math.Min(filter.PageSize, 24);
        filter.SortBy = NormalizeSort(filter.SortBy);
        filter.ViewMode = NormalizeViewMode(filter.ViewMode);

        var query = _context.Vehicles
            .AsNoTracking()
            .Include(vehicle => vehicle.VehicleCategory)
            .Where(vehicle => vehicle.Status == VehicleStatus.Available)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var keyword = filter.SearchTerm.Trim();
            query = query.Where(vehicle =>
                (vehicle.Brand + " " + vehicle.Model).Contains(keyword) ||
                vehicle.LicensePlate.Contains(keyword));
        }

        if (filter.VehicleCategoryId.HasValue)
        {
            query = query.Where(vehicle => vehicle.VehicleCategoryId == filter.VehicleCategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Brand))
        {
            query = query.Where(vehicle => vehicle.Brand == filter.Brand.Trim());
        }

        if (filter.MinDailyRate.HasValue)
        {
            query = query.Where(vehicle => vehicle.DailyRate >= filter.MinDailyRate.Value);
        }

        if (filter.MaxDailyRate.HasValue)
        {
            query = query.Where(vehicle => vehicle.DailyRate <= filter.MaxDailyRate.Value);
        }

        if (filter.SeatCount.HasValue)
        {
            query = query.Where(vehicle => vehicle.SeatCount >= filter.SeatCount.Value);
        }

        query = filter.SortBy switch
        {
            "priceDesc" => query.OrderByDescending(vehicle => vehicle.DailyRate).ThenBy(vehicle => vehicle.Brand),
            "newest" => query.OrderByDescending(vehicle => vehicle.Id),
            "popular" => query.OrderByDescending(vehicle => vehicle.Bookings.Count).ThenBy(vehicle => vehicle.Brand),
            _ => query.OrderBy(vehicle => vehicle.DailyRate).ThenBy(vehicle => vehicle.Brand)
        };

        var totalItems = await query.CountAsync(cancellationToken);
        var vehicles = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new CustomerVehicleIndexViewModel
        {
            Filter = filter,
            Vehicles = new PagedResult<Vehicle>
            {
                Items = vehicles,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalItems = totalItems
            },
            CategoryOptions = await _context.VehicleCategories
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .Select(category => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = category.Name,
                    Value = category.Id.ToString(),
                    Selected = filter.VehicleCategoryId == category.Id
                })
                .ToListAsync(cancellationToken),
            BrandOptions = await _context.Vehicles
                .AsNoTracking()
                .Where(vehicle => vehicle.Status == VehicleStatus.Available)
                .Select(vehicle => vehicle.Brand)
                .Distinct()
                .OrderBy(item => item)
                .ToListAsync(cancellationToken),
            PriceRangeMin = await _context.Vehicles.AsNoTracking().MinAsync(item => (decimal?)item.DailyRate, cancellationToken) ?? 0,
            PriceRangeMax = await _context.Vehicles.AsNoTracking().MaxAsync(item => (decimal?)item.DailyRate, cancellationToken) ?? 0,
            SearchHistory = history
        };
    }

    public async Task<VehicleDetailsViewModel?> BuildDetailsAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Include(item => item.VehicleCategory)
            .FirstOrDefaultAsync(item => item.Id == vehicleId, cancellationToken);

        if (vehicle is null)
        {
            return null;
        }

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var endDate = startDate.AddDays(20);

        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(item => item.VehicleId == vehicleId &&
                           item.Status != BookingStatus.Cancelled &&
                           item.Status != BookingStatus.Rejected &&
                           item.Status != BookingStatus.Completed &&
                           item.ReturnDateTime.Date >= startDate.ToDateTime(TimeOnly.MinValue) &&
                           item.PickupDateTime.Date <= endDate.ToDateTime(TimeOnly.MinValue))
            .Select(item => new { item.PickupDateTime, item.ReturnDateTime })
            .ToListAsync(cancellationToken);

        var availability = Enumerable.Range(0, 21)
            .Select(offset => startDate.AddDays(offset))
            .Select(date => new VehicleAvailabilitySlotViewModel
            {
                Date = date,
                IsAvailable = bookings.All(item => !(date.ToDateTime(TimeOnly.MinValue) >= item.PickupDateTime.Date && date.ToDateTime(TimeOnly.MinValue) <= item.ReturnDateTime.Date))
            })
            .ToList();

        return new VehicleDetailsViewModel
        {
            Vehicle = vehicle,
            GalleryUrls = BuildGalleryUrls(vehicle),
            AvailabilitySlots = availability
        };
    }

    public async Task<IReadOnlyList<string>> SearchSuggestionsAsync(string? term, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < 2)
        {
            return [];
        }

        var keyword = term.Trim();
        return await _context.Vehicles
            .AsNoTracking()
            .Where(item => item.Status == VehicleStatus.Available &&
                           (item.Brand.Contains(keyword) ||
                            item.Model.Contains(keyword) ||
                            item.LicensePlate.Contains(keyword)))
            .OrderBy(item => item.Brand)
            .ThenBy(item => item.Model)
            .Select(item => $"{item.Brand} {item.Model} ({item.LicensePlate})")
            .Distinct()
            .Take(8)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> VehicleExistsAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return _context.Vehicles.AsNoTracking().AnyAsync(item => item.Id == vehicleId, cancellationToken);
    }

    private IReadOnlyList<string> BuildGalleryUrls(Vehicle vehicle)
    {
        var urls = new List<string>();

        if (!string.IsNullOrWhiteSpace(vehicle.ImageUrl))
        {
            urls.Add(vehicle.ImageUrl);
        }

        var relativeFolder = Path.Combine("Content", "Images", "Vehicles", vehicle.Id.ToString("N"));
        var absoluteFolder = Path.Combine(_environment.WebRootPath, relativeFolder);
        if (Directory.Exists(absoluteFolder))
        {
            urls.AddRange(Directory
                .GetFiles(absoluteFolder)
                .OrderBy(path => path)
                .Select(path => $"/{relativeFolder.Replace("\\", "/")}/{Path.GetFileName(path)}"));
        }

        return urls.Distinct().Take(8).ToList();
    }

    private static string NormalizeSort(string? sortBy)
    {
        return sortBy switch
        {
            "priceDesc" => "priceDesc",
            "newest" => "newest",
            "popular" => "popular",
            _ => "priceAsc"
        };
    }

    private static string NormalizeViewMode(string? viewMode)
    {
        return string.Equals(viewMode, "list", StringComparison.OrdinalIgnoreCase) ? "list" : "grid";
    }
}