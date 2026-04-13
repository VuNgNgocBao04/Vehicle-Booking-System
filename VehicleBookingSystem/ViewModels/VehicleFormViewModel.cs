using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.Resources;

namespace VehicleBookingSystem.ViewModels;

public class VehicleFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.VehicleCategoryRequired))]
    [Display(Name = "Danh mục xe")]
    public Guid VehicleCategoryId { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.CodeRequired))]
    [StringLength(20, MinimumLength = 3, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.StringLength20))]
    [RegularExpression("^[A-Z0-9-]+$", ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.CodePattern))]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.BrandRequired))]
    [StringLength(100, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.StringLength100))]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.ModelRequired))]
    [StringLength(100, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.StringLength100))]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.LicensePlateRequired))]
    [StringLength(20, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.StringLength20))]
    [RegularExpression("^[0-9]{2}[A-Z]-[0-9]{3}\\.[0-9]{2}$", ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.LicensePlatePattern))]
    [Display(Name = "Biển số")]
    public string LicensePlate { get; set; } = string.Empty;

    [Range(2, 60, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.SeatRange))]
    [Display(Name = "Số chỗ")]
    public int SeatCount { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.ColorRequired))]
    [StringLength(50, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.StringLength50))]
    public string Color { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.TransmissionRequired))]
    [StringLength(50, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.StringLength50))]
    public string Transmission { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.FuelTypeRequired))]
    [StringLength(50, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.StringLength50))]
    [Display(Name = "Nhiên liệu")]
    public string FuelType { get; set; } = string.Empty;

    [Range(100000, 100000000, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.DailyRateRange))]
    [DataType(DataType.Currency)]
    [Display(Name = "Giá thuê/ngày")]
    public decimal DailyRate { get; set; }

    [Display(Name = "Trạng thái")]
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;

    [StringLength(500)]
    [Display(Name = "Ảnh đại diện")]
    public string? ImageUrl { get; set; }

    [StringLength(2000, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.StringLength2000))]
    [Display(Name = "Mô tả xe")]
    public string? Description { get; set; }

    [StringLength(6000, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.StringLength6000))]
    [Display(Name = "Điều khoản đặt xe / Chính sách")]
    public string? BookingPolicyHtml { get; set; }

    [Display(Name = "Ảnh xe (nhiều ảnh)")]
    public List<IFormFile>? GalleryFiles { get; set; }

    public IReadOnlyList<string> ExistingGalleryUrls { get; set; } = [];
}
