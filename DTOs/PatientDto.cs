namespace MedicalRecordService.DTOs;

// Dùng để hứng data khi Lễ tân/Bệnh nhân tạo mới hồ sơ
public class CreatePatientDto
{
    public string FullName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = null!;
    public string? MedicalHistory { get; set; }
    public string? Allergies { get; set; }
    public string? BloodGroup { get; set; }
    public int? GatewayPatientId { get; set; }
}

// Dùng để trả data về cho client xem
public class PatientResponseDto
{
    public Guid Id { get; set; }
    public int? GatewayPatientId { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = null!;
    public string? MedicalHistory { get; set; }
    public string? Allergies { get; set; }
    public string? BloodGroup { get; set; }
}