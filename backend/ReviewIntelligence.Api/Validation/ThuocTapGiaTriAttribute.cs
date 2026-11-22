using System.ComponentModel.DataAnnotations;

namespace ReviewIntelligence.Api.Validation;

/// <summary>
/// Kiểm tra chuỗi thuộc tập giá trị cho phép (đối chiếu hằng trong Models/Entities/GiaTriHopLe.cs).
/// NULL được coi là hợp lệ – kết hợp [Required] nếu bắt buộc.
/// </summary>
/// <example>[ThuocTapGiaTri(TrangThaiDanhGia.DaDuyet, TrangThaiDanhGia.DaAn)]</example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Field)]
public sealed class ThuocTapGiaTriAttribute(params string[] giaTriHopLe) : ValidationAttribute
{
    public IReadOnlyList<string> GiaTriHopLe { get; } = giaTriHopLe;

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is null) return ValidationResult.Success;
        if (value is string s && GiaTriHopLe.Contains(s, StringComparer.Ordinal)) return ValidationResult.Success;

        var thongBao = ErrorMessage ?? $"Giá trị '{value}' không hợp lệ. Chấp nhận: {string.Join(", ", GiaTriHopLe)}.";
        return new ValidationResult(thongBao, context.MemberName is null ? null : [context.MemberName]);
    }
}
