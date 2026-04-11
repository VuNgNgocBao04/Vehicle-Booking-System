using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.Data;

public static class DbSeeder
{
    private const string AdminEmail = "admin@vehiclebooking.local";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        var adminPassword = configuration["SeedData:AdminPassword"];
        var adminEmail = configuration["SeedData:AdminEmail"] ?? AdminEmail;

        var adminRole = await EnsureRoleAsync(roleManager, "Admin", "Administrator role");
        var customerRole = await EnsureRoleAsync(roleManager, "Customer", "Customer role");

        var adminUser = await userManager.Users.FirstOrDefaultAsync(user => user.Email == adminEmail);
        if (adminUser is null)
        {
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException("SeedData:AdminPassword must be configured when startup seeding is enabled.");
            }

            adminUser = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                var messages = string.Join(", ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Unable to create admin user: {messages}");
            }

            await userManager.AddToRoleAsync(adminUser, adminRole.Name!);
        }

        if (!await userManager.IsInRoleAsync(adminUser, adminRole.Name!))
        {
            await userManager.AddToRoleAsync(adminUser, adminRole.Name!);
        }

        var categoryMap = new Dictionary<string, VehicleCategory>(StringComparer.OrdinalIgnoreCase);
        foreach (var category in new[]
                 {
                     new VehicleCategory { Id = Guid.NewGuid(), Name = "Sedan", Description = "Sedan for city and business travel" },
                     new VehicleCategory { Id = Guid.NewGuid(), Name = "SUV", Description = "Spacious sport utility vehicle" },
                     new VehicleCategory { Id = Guid.NewGuid(), Name = "MPV", Description = "Multi-purpose vehicle for families" },
                     new VehicleCategory { Id = Guid.NewGuid(), Name = "Hatchback", Description = "Compact city car" }
                 })
        {
            var existing = await context.VehicleCategories.FirstOrDefaultAsync(item => item.Name == category.Name);
            if (existing is null)
            {
                context.VehicleCategories.Add(category);
                categoryMap[category.Name] = category;
            }
            else
            {
                categoryMap[category.Name] = existing;
            }
        }

        await context.SaveChangesAsync();

        if (!await context.Vehicles.AnyAsync())
        {
            context.Vehicles.AddRange(
                new Vehicle
                {
                    Id = Guid.NewGuid(),
                    VehicleCategoryId = categoryMap["Sedan"].Id,
                    Code = "VHC-001",
                    Brand = "Toyota",
                    Model = "Camry",
                    LicensePlate = "51A-123.45",
                    SeatCount = 5,
                    Color = "White",
                    Transmission = "Automatic",
                    FuelType = "Hybrid",
                    DailyRate = 1200000,
                    Status = VehicleStatus.Available,
                    Description = "Comfort sedan for business trips"
                },
                new Vehicle
                {
                    Id = Guid.NewGuid(),
                    VehicleCategoryId = categoryMap["SUV"].Id,
                    Code = "VHC-002",
                    Brand = "Ford",
                    Model = "Everest",
                    LicensePlate = "51B-678.90",
                    SeatCount = 7,
                    Color = "Black",
                    Transmission = "Automatic",
                    FuelType = "Diesel",
                    DailyRate = 1600000,
                    Status = VehicleStatus.Available,
                    Description = "Family SUV for long-distance travel"
                },
                new Vehicle
                {
                    Id = Guid.NewGuid(),
                    VehicleCategoryId = categoryMap["MPV"].Id,
                    Code = "VHC-003",
                    Brand = "Kia",
                    Model = "Carnival",
                    LicensePlate = "51C-246.80",
                    SeatCount = 8,
                    Color = "Gray",
                    Transmission = "Automatic",
                    FuelType = "Gasoline",
                    DailyRate = 1800000,
                    Status = VehicleStatus.Maintenance,
                    Description = "Premium MPV for group bookings"
                });

            await context.SaveChangesAsync();
        }

        _ = customerRole;
    }

    private static async Task<AppRole> EnsureRoleAsync(RoleManager<AppRole> roleManager, string roleName, string description)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is not null)
        {
            return role;
        }

        role = new AppRole
        {
            Id = Guid.NewGuid(),
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant(),
            Description = description
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var messages = string.Join(", ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Unable to create role '{roleName}': {messages}");
        }

        return role;
    }
}