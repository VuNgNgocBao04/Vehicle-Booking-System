using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<VehicleCategory> VehicleCategories => Set<VehicleCategory>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<VehicleCategory>(entity =>
        {
            entity.HasIndex(category => category.Name).IsUnique();
            entity.Property(category => category.Name).HasMaxLength(100).IsRequired();
            entity.Property(category => category.Description).HasMaxLength(300);
        });

        builder.Entity<Vehicle>(entity =>
        {
            entity.Property(vehicle => vehicle.Code).HasMaxLength(20).IsRequired();
            entity.Property(vehicle => vehicle.Brand).HasMaxLength(100).IsRequired();
            entity.Property(vehicle => vehicle.Model).HasMaxLength(100).IsRequired();
            entity.Property(vehicle => vehicle.LicensePlate).HasMaxLength(20).IsRequired();
            entity.Property(vehicle => vehicle.Color).HasMaxLength(50).IsRequired();
            entity.Property(vehicle => vehicle.Transmission).HasMaxLength(50).IsRequired();
            entity.Property(vehicle => vehicle.FuelType).HasMaxLength(50).IsRequired();
            entity.Property(vehicle => vehicle.ImageUrl).HasMaxLength(500);
            entity.Property(vehicle => vehicle.Description).HasMaxLength(500);
            entity.HasIndex(vehicle => vehicle.Code).IsUnique();
            entity.HasIndex(vehicle => vehicle.LicensePlate).IsUnique();
        });

        builder.Entity<Booking>(entity =>
        {
            entity.Property(booking => booking.BookingCode).HasMaxLength(20).IsRequired();
            entity.Property(booking => booking.PickupLocation).HasMaxLength(200).IsRequired();
            entity.Property(booking => booking.DropoffLocation).HasMaxLength(200).IsRequired();
            entity.Property(booking => booking.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(booking => booking.RowVersion).IsRowVersion();
            entity.HasIndex(booking => booking.BookingCode).IsUnique();
            entity.HasOne(booking => booking.User)
                .WithMany(user => user.Bookings)
                .HasForeignKey(booking => booking.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(booking => booking.Vehicle)
                .WithMany(vehicle => vehicle.Bookings)
                .HasForeignKey(booking => booking.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Payment>(entity =>
        {
            entity.Property(payment => payment.PaymentMethod).HasConversion<string>().HasMaxLength(30);
            entity.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(payment => payment.TransactionReference).HasMaxLength(100);
            entity.HasOne(payment => payment.Booking)
                .WithOne(booking => booking.Payment)
                .HasForeignKey<Payment>(payment => payment.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}