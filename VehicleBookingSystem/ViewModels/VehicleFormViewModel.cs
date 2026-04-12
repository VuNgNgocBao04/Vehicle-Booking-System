using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.ViewModels;

public class VehicleFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Danh mục xe là bắt buộc.")]
    [Display(Name = "Danh mục xe")]
    public Guid VehicleCategoryId { get; set; }

    [Required(ErrorMessage = "Mã xe là bắt buộc.")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Mã xe từ 3 đến 20 ký tự.")]
    [RegularExpression("^[A-Z0-9-]+$", ErrorMessage = "Mã xe chỉ gồm chữ in hoa, số hoặc dấu gạch ngang.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hãng xe là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Hãng xe tối đa 100 ký tự.")]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mẫu xe là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Mẫu xe tối đa 100 ký tự.")]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "Biển số xe là bắt buộc.")]
    [StringLength(20, ErrorMessage = "Biển số tối đa 20 ký tự.")]
    [RegularExpression("^[0-9]{2}[A-Z]-[0-9]{3}\\.[0-9]{2}$", ErrorMessage = "Biển số không đúng định dạng (VD: 30A-123.45).")]
    [Display(Name = "Biển số")]
    public string LicensePlate { get; set; } = string.Empty;

    [Range(2, 60, ErrorMessage = "Số chỗ phải từ 2 đến 60.")]
    [Display(Name = "Số chỗ")]
    public int SeatCount { get; set; }

    [Required(ErrorMessage = "Màu xe là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Màu xe tối đa 50 ký tự.")]
    public string Color { get; set; } = string.Empty;

    [Required(ErrorMessage = "Loại hộp số là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Loại hộp số tối đa 50 ký tự.")]
    public string Transmission { get; set; } = string.Empty;

    [Required(ErrorMessage = "Loại nhiên liệu là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Loại nhiên liệu tối đa 50 ký tự.")]
    [Display(Name = "Nhiên liệu")]
    public string FuelType { get; set; } = string.Empty;

    [Range(100000, 100000000, ErrorMessage = "Giá thuê/ngày phải từ 100.000 đến 100.000.000.")]
    [DataType(DataType.Currency)]
    [Display(Name = "Giá thuê/ngày")]
    public decimal DailyRate { get; set; }

    [Display(Name = "Trạng thái")]
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;

    [StringLength(500)]
    [Display(Name = "Ảnh đại diện")]
    public string? ImageUrl { get; set; }

    [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
    [Display(Name = "Mô tả xe")]
    public string? Description { get; set; }

    [StringLength(6000, ErrorMessage = "Điều khoản tối đa 6000 ký tự.")]
    [Display(Name = "Điều khoản đặt xe / Chính sách")]
    public string? BookingPolicyHtml { get; set; }

    [Display(Name = "Ảnh xe (nhiều ảnh)")]
    public List<IFormFile>? GalleryFiles { get; set; }

    public IReadOnlyList<string> ExistingGalleryUrls { get; set; } = [];
}
