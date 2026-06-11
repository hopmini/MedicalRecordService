namespace MedicalRecordService.DTOs;

public class CreateMedicalRecordDto
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; } 
    
    public string Symptoms { get; set; } = null!;
    public string Diagnosis { get; set; } = null!;
    public string? Notes { get; set; }

    // [Cross-Service] Tự động cập nhật Appointment → "Đã khám xong" khi ghi bệnh án
    public Guid? AppointmentId { get; set; }
    // Tên bệnh nhân để Self-Heal tạo hồ sơ chính xác (thay vì "Bệnh nhân Medicare")
    public string? PatientName { get; set; }
    // Gateway Patient ID (int) để đồng bộ với các service khác
    public int? GatewayPatientId { get; set; }
}

public class MedicalRecordResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public string Symptoms { get; set; } = null!;
    public string Diagnosis { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public PrescriptionResponseDto? Prescription { get; set; }
}

public class PrescriptionResponseDto
{
    public Guid Id { get; set; }
    public string? Instructions { get; set; }
    public DateTime PrescribedAt { get; set; }
    public List<PrescriptionDetailResponseDto> Details { get; set; } = new();
}

public class PrescriptionDetailResponseDto
{
    public Guid Id { get; set; }
    public Guid MedicationId { get; set; }
    public string MedicationName { get; set; } = null!;
    public int Quantity { get; set; }
    public string Dosage { get; set; } = null!;
}